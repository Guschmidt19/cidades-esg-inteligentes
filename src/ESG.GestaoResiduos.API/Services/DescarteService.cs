using ESG.GestaoResiduos.API.Common.Exceptions;
using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Data;
using ESG.GestaoResiduos.API.Models;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESG.GestaoResiduos.API.Services;

/// <summary>
/// Núcleo de negócio do sistema. O método RegistrarAsync reproduz, em código C#
/// e dentro de uma transação, as quatro automações que antes eram triggers PL/SQL:
/// (1) atualização da quantidade acumulada do ponto, (2) geração de alerta aos 80%,
/// (3) mudança de status para CHEIO, e (4) upsert do relatório de compliance mensal.
/// </summary>
public class DescarteService : IDescarteService
{
    private const decimal LimiteAlertaPercentual = 80m;
    private readonly AppDbContext _db;

    public DescarteService(AppDbContext db) => _db = db;

    public async Task<DescarteResultadoResponse> RegistrarAsync(RegistrarDescarteRequest request, CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var ponto = await _db.PontosColeta.FirstOrDefaultAsync(p => p.IdPonto == request.IdPonto, ct)
            ?? throw new NotFoundException($"Ponto de coleta {request.IdPonto} não encontrado.");

        var gerador = await _db.Geradores.FirstOrDefaultAsync(g => g.IdGerador == request.IdGerador, ct)
            ?? throw new NotFoundException($"Gerador {request.IdGerador} não encontrado.");

        if (ponto.Status == StatusPonto.INATIVO)
            throw new BusinessRuleException("Não é possível registrar descarte em um ponto INATIVO.");

        if (ponto.Status == StatusPonto.CHEIO)
            throw new BusinessRuleException("O ponto de coleta está CHEIO. Realize a coleta antes de novos descartes.");

        if (ponto.TipoResiduo != request.TipoResiduo)
            throw new BusinessRuleException(
                $"O ponto aceita apenas resíduo do tipo {ponto.TipoResiduo}, mas foi informado {request.TipoResiduo}.");

        var descarte = new Descarte
        {
            IdPonto = request.IdPonto,
            IdGerador = request.IdGerador,
            TipoResiduo = request.TipoResiduo,
            PesoKg = request.PesoKg,
            DataDescarte = DateTime.UtcNow
        };
        _db.Descartes.Add(descarte);

        // (1) Atualiza quantidade acumulada do ponto (TRG_ATUALIZA_PONTO_COLETA)
        ponto.QtdeAtualKg += request.PesoKg;

        var percentual = ponto.PercentualOcupacao;
        var resultado = new DescarteResultadoResponse();

        // (2) Gera alerta a partir de 80%, sem duplicar alerta PENDENTE (TRG_ALERTA_CAPACIDADE)
        if (percentual >= LimiteAlertaPercentual)
        {
            var jaExistePendente = await _db.Alertas
                .CountAsync(a => a.IdPonto == ponto.IdPonto && a.StatusAlerta == StatusAlerta.PENDENTE, ct) > 0;

            if (!jaExistePendente)
            {
                _db.Alertas.Add(new AlertaColeta
                {
                    IdPonto = ponto.IdPonto,
                    DataAlerta = DateTime.UtcNow,
                    PercentualOcupacao = percentual,
                    StatusAlerta = StatusAlerta.PENDENTE,
                    Mensagem = $"ALERTA AUTOMÁTICO: Ponto \"{ponto.Nome}\" atingiu {percentual:0.##}% da capacidade. Coleta necessária."
                });
                resultado.AlertaGerado = true;
            }
        }

        // (3) Atualiza status para CHEIO quando atinge a capacidade (TRG_STATUS_PONTO_CHEIO)
        if (ponto.QtdeAtualKg >= ponto.CapacidadeMaxKg)
        {
            ponto.Status = StatusPonto.CHEIO;
            resultado.PontoFicouCheio = true;
        }

        // (4) Cria ou acumula o relatório de compliance do mês/ano (TRG_RELATORIO_COMPLIANCE)
        var mes = descarte.DataDescarte.Month;
        var ano = descarte.DataDescarte.Year;
        var relatorio = await _db.Relatorios.FirstOrDefaultAsync(
            r => r.IdPonto == ponto.IdPonto && r.MesReferencia == mes && r.AnoReferencia == ano, ct);

        if (relatorio is null)
        {
            _db.Relatorios.Add(new RelatorioCompliance
            {
                IdPonto = ponto.IdPonto,
                MesReferencia = mes,
                AnoReferencia = ano,
                TotalKgColetado = request.PesoKg,
                QtdDescartes = 1,
                Conforme = true,
                DataGeracao = DateTime.UtcNow
            });
        }
        else
        {
            relatorio.TotalKgColetado += request.PesoKg;
            relatorio.QtdDescartes += 1;
        }

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        resultado.Descarte = new DescarteResponse
        {
            IdDescarte = descarte.IdDescarte,
            IdPonto = ponto.IdPonto,
            PontoNome = ponto.Nome,
            IdGerador = gerador.IdGerador,
            GeradorNome = gerador.Nome,
            DataDescarte = descarte.DataDescarte,
            TipoResiduo = descarte.TipoResiduo.ToString(),
            PesoKg = descarte.PesoKg
        };
        resultado.QtdeAtualPonto = ponto.QtdeAtualKg;
        resultado.PercentualOcupacao = percentual;
        resultado.StatusPonto = ponto.Status.ToString();
        resultado.Mensagem = "Descarte registrado e automações de compliance aplicadas com sucesso.";

        return resultado;
    }

    public async Task<PagedResult<DescarteResponse>> ListarAsync(PaginationParams pagination, int? idPonto, CancellationToken ct = default)
    {
        // Filtro e ordenação aplicados no banco; COUNT e OFFSET/FETCH executados no servidor.
        var baseQuery = _db.Descartes
            .AsNoTracking()
            .Where(d => !idPonto.HasValue || d.IdPonto == idPonto.Value);

        var total = await baseQuery.CountAsync(ct);

        var rows = await baseQuery
            .OrderByDescending(d => d.DataDescarte)
            .Skip((pagination.PageNumber - 1) * pagination.PageSize)
            .Take(pagination.PageSize)
            .Select(d => new
            {
                d.IdDescarte,
                d.IdPonto,
                PontoNome = d.Ponto!.Nome,
                d.IdGerador,
                GeradorNome = d.Gerador!.Nome,
                d.DataDescarte,
                d.TipoResiduo,
                d.PesoKg
            })
            .ToListAsync(ct);

        var items = rows.Select(d => new DescarteResponse
        {
            IdDescarte = d.IdDescarte,
            IdPonto = d.IdPonto,
            PontoNome = d.PontoNome,
            IdGerador = d.IdGerador,
            GeradorNome = d.GeradorNome,
            DataDescarte = d.DataDescarte,
            TipoResiduo = d.TipoResiduo.ToString(),
            PesoKg = d.PesoKg
        }).ToList();

        return new PagedResult<DescarteResponse>(items, total, pagination.PageNumber, pagination.PageSize);
    }
}
