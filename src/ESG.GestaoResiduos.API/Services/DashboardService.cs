using ESG.GestaoResiduos.API.Data;
using ESG.GestaoResiduos.API.Models;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESG.GestaoResiduos.API.Services;

/// <summary>
/// Indicadores ESG consolidados. Cada métrica é calculada com uma consulta
/// agregada executada no banco de dados (COUNT/SUM/GROUP BY), evitando trazer
/// grandes volumes de registros para a memória da aplicação.
/// </summary>
public class DashboardService : IDashboardService
{
    private readonly AppDbContext _db;

    public DashboardService(AppDbContext db) => _db = db;

    public async Task<DashboardResponse> ObterAsync(CancellationToken ct = default)
    {
        var totalPontos = await _db.PontosColeta.CountAsync(ct);
        var pontosAtivos = await _db.PontosColeta.CountAsync(p => p.Status == StatusPonto.ATIVO, ct);
        var pontosCheios = await _db.PontosColeta.CountAsync(p => p.Status == StatusPonto.CHEIO, ct);
        var alertasPendentes = await _db.Alertas.CountAsync(a => a.StatusAlerta == StatusAlerta.PENDENTE, ct);

        var totalColetado = await _db.Descartes.CountAsync(ct) > 0
            ? await _db.Descartes.SumAsync(d => d.PesoKg, ct)
            : 0m;

        var totalRelatorios = await _db.Relatorios.CountAsync(ct);
        var conformes = await _db.Relatorios.CountAsync(r => r.Conforme, ct);
        var taxaConformidade = totalRelatorios > 0
            ? Math.Round((decimal)conformes / totalRelatorios * 100m, 2)
            : 100m;

        var coletaPorTipoRaw = await _db.Descartes
            .GroupBy(d => d.TipoResiduo)
            .Select(g => new { Tipo = g.Key, TotalKg = g.Sum(x => x.PesoKg), Qtd = g.Count() })
            .ToListAsync(ct);

        var coletaPorTipo = coletaPorTipoRaw
            .Select(g => new TotalPorTipoResiduo
            {
                TipoResiduo = g.Tipo.ToString(),
                TotalKg = g.TotalKg,
                QtdDescartes = g.Qtd
            })
            .OrderByDescending(x => x.TotalKg)
            .ToList();

        var criticosRaw = await _db.PontosColeta
            .Where(p => p.CapacidadeMaxKg > 0)
            .OrderByDescending(p => p.QtdeAtualKg / p.CapacidadeMaxKg)
            .Take(5)
            .ToListAsync(ct);

        var pontosCriticos = criticosRaw
            .Select(p => new PontoCriticoResponse
            {
                IdPonto = p.IdPonto,
                Nome = p.Nome,
                PercentualOcupacao = p.PercentualOcupacao,
                Status = p.Status.ToString()
            })
            .ToList();

        return new DashboardResponse
        {
            TotalPontos = totalPontos,
            PontosAtivos = pontosAtivos,
            PontosCheios = pontosCheios,
            AlertasPendentes = alertasPendentes,
            TotalColetadoKg = totalColetado,
            TaxaConformidade = taxaConformidade,
            ColetaPorTipo = coletaPorTipo,
            PontosCriticos = pontosCriticos
        };
    }
}
