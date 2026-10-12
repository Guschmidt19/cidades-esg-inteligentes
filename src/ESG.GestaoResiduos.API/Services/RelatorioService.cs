using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Data;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESG.GestaoResiduos.API.Services;

public class RelatorioService : IRelatorioService
{
    private readonly AppDbContext _db;

    public RelatorioService(AppDbContext db) => _db = db;

    public async Task<PagedResult<RelatorioComplianceResponse>> ListarAsync(RelatorioFiltro filtro, CancellationToken ct = default)
    {
        var query = _db.Relatorios.AsNoTracking().AsQueryable();

        if (filtro.Ano.HasValue) query = query.Where(r => r.AnoReferencia == filtro.Ano.Value);
        if (filtro.Mes.HasValue) query = query.Where(r => r.MesReferencia == filtro.Mes.Value);
        if (filtro.IdPonto.HasValue) query = query.Where(r => r.IdPonto == filtro.IdPonto.Value);
        if (filtro.Conforme.HasValue) query = query.Where(r => r.Conforme == filtro.Conforme.Value);

        var total = await query.CountAsync(ct);

        var items = await query
            .OrderByDescending(r => r.AnoReferencia)
            .ThenByDescending(r => r.MesReferencia)
            .ThenBy(r => r.IdPonto)
            .Skip((filtro.PageNumber - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .Select(r => new RelatorioComplianceResponse
            {
                IdRelatorio = r.IdRelatorio,
                IdPonto = r.IdPonto,
                PontoNome = r.Ponto!.Nome,
                MesReferencia = r.MesReferencia,
                AnoReferencia = r.AnoReferencia,
                TotalKgColetado = r.TotalKgColetado,
                QtdDescartes = r.QtdDescartes,
                Conforme = r.Conforme,
                DataGeracao = r.DataGeracao
            })
            .ToListAsync(ct);

        return new PagedResult<RelatorioComplianceResponse>(items, total, filtro.PageNumber, filtro.PageSize);
    }

    /// <summary>
    /// Consolidação agregada por período usando GROUP BY traduzido para SQL
    /// (otimização de consulta — agregação executada no banco, não em memória).
    /// </summary>
    public async Task<IReadOnlyList<RelatorioConsolidadoResponse>> ConsolidarAsync(int? ano, CancellationToken ct = default)
    {
        var query = _db.Relatorios.AsNoTracking().AsQueryable();
        if (ano.HasValue) query = query.Where(r => r.AnoReferencia == ano.Value);

        var grupos = await query
            .GroupBy(r => new { r.AnoReferencia, r.MesReferencia })
            .Select(g => new
            {
                g.Key.AnoReferencia,
                g.Key.MesReferencia,
                TotalKg = g.Sum(x => x.TotalKgColetado),
                QtdDescartes = g.Sum(x => x.QtdDescartes),
                QtdRelatorios = g.Count(),
                QtdConformes = g.Count(x => x.Conforme),
                QtdNaoConformes = g.Count(x => !x.Conforme)
            })
            .OrderByDescending(g => g.AnoReferencia)
            .ThenByDescending(g => g.MesReferencia)
            .ToListAsync(ct);

        return grupos.Select(g => new RelatorioConsolidadoResponse
        {
            AnoReferencia = g.AnoReferencia,
            MesReferencia = g.MesReferencia,
            TotalKgColetado = g.TotalKg,
            QtdDescartes = g.QtdDescartes,
            QtdRelatorios = g.QtdRelatorios,
            QtdConformes = g.QtdConformes,
            QtdNaoConformes = g.QtdNaoConformes,
            PercentualConformidade = g.QtdRelatorios > 0
                ? Math.Round((decimal)g.QtdConformes / g.QtdRelatorios * 100m, 2)
                : 0m
        }).ToList();
    }
}
