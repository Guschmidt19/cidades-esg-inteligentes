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

        // GROUP BY executado no banco. A coluna CONFORME é gravada como 'S'/'N' (texto), então ela
        // entra na chave do agrupamento em vez de ser usada como condição booleana (FILTER),
        // o que o PostgreSQL rejeita.
        var grupos = await query
            .GroupBy(r => new { r.AnoReferencia, r.MesReferencia, r.Conforme })
            .Select(g => new
            {
                g.Key.AnoReferencia,
                g.Key.MesReferencia,
                g.Key.Conforme,
                TotalKg = g.Sum(x => x.TotalKgColetado),
                QtdDescartes = g.Sum(x => x.QtdDescartes),
                QtdRelatorios = g.Count()
            })
            .ToListAsync(ct);

        // No máximo 2 linhas por período (conforme / não conforme): união leve em memória.
        return grupos
            .GroupBy(g => new { g.AnoReferencia, g.MesReferencia })
            .Select(periodo =>
            {
                var qtdRelatorios = periodo.Sum(x => x.QtdRelatorios);
                var qtdConformes = periodo.Where(x => x.Conforme).Sum(x => x.QtdRelatorios);

                return new RelatorioConsolidadoResponse
                {
                    AnoReferencia = periodo.Key.AnoReferencia,
                    MesReferencia = periodo.Key.MesReferencia,
                    TotalKgColetado = periodo.Sum(x => x.TotalKg),
                    QtdDescartes = periodo.Sum(x => x.QtdDescartes),
                    QtdRelatorios = qtdRelatorios,
                    QtdConformes = qtdConformes,
                    QtdNaoConformes = qtdRelatorios - qtdConformes,
                    PercentualConformidade = qtdRelatorios > 0
                        ? Math.Round((decimal)qtdConformes / qtdRelatorios * 100m, 2)
                        : 0m
                };
            })
            .OrderByDescending(r => r.AnoReferencia)
            .ThenByDescending(r => r.MesReferencia)
            .ToList();
    }
}
