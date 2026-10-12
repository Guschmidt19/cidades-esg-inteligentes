using ESG.GestaoResiduos.API.Common.Exceptions;
using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Data;
using ESG.GestaoResiduos.API.Models;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESG.GestaoResiduos.API.Services;

public class AlertaService : IAlertaService
{
    private readonly AppDbContext _db;

    public AlertaService(AppDbContext db) => _db = db;

    public async Task<PagedResult<AlertaResponse>> ListarAsync(AlertaFiltro filtro, CancellationToken ct = default)
    {
        var query = _db.Alertas.AsNoTracking().AsQueryable();

        if (filtro.Status.HasValue)
            query = query.Where(a => a.StatusAlerta == filtro.Status.Value);

        if (filtro.IdPonto.HasValue)
            query = query.Where(a => a.IdPonto == filtro.IdPonto.Value);

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(a => a.DataAlerta)
            .Skip((filtro.PageNumber - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(a => new
            {
                a.IdAlerta,
                a.IdPonto,
                PontoNome = a.Ponto!.Nome,
                a.DataAlerta,
                a.PercentualOcupacao,
                a.StatusAlerta,
                a.Mensagem
            })
            .ToListAsync(ct);

        var items = rows.Select(a => new AlertaResponse
        {
            IdAlerta = a.IdAlerta,
            IdPonto = a.IdPonto,
            PontoNome = a.PontoNome,
            DataAlerta = a.DataAlerta,
            PercentualOcupacao = a.PercentualOcupacao,
            StatusAlerta = a.StatusAlerta.ToString(),
            Mensagem = a.Mensagem
        }).ToList();

        return new PagedResult<AlertaResponse>(items, total, filtro.PageNumber, filtro.PageSize);
    }

    /// <summary>
    /// Atende um alerta: marca como ATENDIDO e simula a coleta no ponto, zerando a
    /// quantidade acumulada e reativando o ponto (espelha a reversão do TRG_STATUS_PONTO_CHEIO).
    /// Endpoint crítico — exige autenticação.
    /// </summary>
    public async Task<AtenderAlertaResponse> AtenderAsync(int idAlerta, CancellationToken ct = default)
    {
        await using var transaction = await _db.Database.BeginTransactionAsync(ct);

        var alerta = await _db.Alertas.FirstOrDefaultAsync(a => a.IdAlerta == idAlerta, ct)
            ?? throw new NotFoundException($"Alerta {idAlerta} não encontrado.");

        if (alerta.StatusAlerta == StatusAlerta.ATENDIDO)
            throw new BusinessRuleException("Este alerta já foi atendido.");

        var ponto = await _db.PontosColeta.FirstOrDefaultAsync(p => p.IdPonto == alerta.IdPonto, ct)
            ?? throw new NotFoundException($"Ponto de coleta {alerta.IdPonto} não encontrado.");

        alerta.StatusAlerta = StatusAlerta.ATENDIDO;
        ponto.QtdeAtualKg = 0;
        if (ponto.Status == StatusPonto.CHEIO)
            ponto.Status = StatusPonto.ATIVO;

        await _db.SaveChangesAsync(ct);
        await transaction.CommitAsync(ct);

        return new AtenderAlertaResponse
        {
            IdAlerta = alerta.IdAlerta,
            StatusAlerta = alerta.StatusAlerta.ToString(),
            IdPonto = ponto.IdPonto,
            QtdeAtualPonto = ponto.QtdeAtualKg,
            StatusPonto = ponto.Status.ToString(),
            Mensagem = $"Coleta registrada no ponto \"{ponto.Nome}\". Capacidade liberada."
        };
    }
}
