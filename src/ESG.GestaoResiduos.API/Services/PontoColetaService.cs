using ESG.GestaoResiduos.API.Common.Exceptions;
using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.Data;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.EntityFrameworkCore;

namespace ESG.GestaoResiduos.API.Services;

public class PontoColetaService : IPontoColetaService
{
    private readonly AppDbContext _db;

    public PontoColetaService(AppDbContext db) => _db = db;

    public async Task<PagedResult<PontoColetaResponse>> ListarAsync(PontoColetaFiltro filtro, CancellationToken ct = default)
    {
        var query = _db.PontosColeta.AsNoTracking().AsQueryable();

        if (filtro.Status.HasValue)
            query = query.Where(p => p.Status == filtro.Status.Value);

        if (filtro.TipoResiduo.HasValue)
            query = query.Where(p => p.TipoResiduo == filtro.TipoResiduo.Value);

        if (!string.IsNullOrWhiteSpace(filtro.Busca))
        {
            var termo = filtro.Busca.Trim().ToUpper();
            query = query.Where(p => p.Nome.ToUpper().Contains(termo) || p.Localizacao.ToUpper().Contains(termo));
        }

        // Filtro de ocupação mínima traduzido para SQL (evita divisão por zero).
        if (filtro.OcupacaoMinima.HasValue)
        {
            var min = filtro.OcupacaoMinima.Value;
            query = query.Where(p => p.CapacidadeMaxKg > 0 && (p.QtdeAtualKg / p.CapacidadeMaxKg) * 100m >= min);
        }

        var total = await query.CountAsync(ct);

        var rows = await query
            .OrderByDescending(p => p.CapacidadeMaxKg > 0 ? p.QtdeAtualKg / p.CapacidadeMaxKg : 0m)
            .ThenBy(p => p.Nome)
            .Skip((filtro.PageNumber - 1) * filtro.PageSize)
            .Take(filtro.PageSize)
            .ToListAsync(ct);

        var items = rows.Select(MapToResponse).ToList();
        return new PagedResult<PontoColetaResponse>(items, total, filtro.PageNumber, filtro.PageSize);
    }

    public async Task<PontoColetaResponse> ObterPorIdAsync(int id, CancellationToken ct = default)
    {
        var ponto = await _db.PontosColeta.AsNoTracking().FirstOrDefaultAsync(p => p.IdPonto == id, ct)
            ?? throw new NotFoundException($"Ponto de coleta {id} não encontrado.");

        return MapToResponse(ponto);
    }

    private static PontoColetaResponse MapToResponse(Models.PontoColeta p) => new()
    {
        IdPonto = p.IdPonto,
        Nome = p.Nome,
        Localizacao = p.Localizacao,
        TipoResiduo = p.TipoResiduo.ToString(),
        CapacidadeMaxKg = p.CapacidadeMaxKg,
        QtdeAtualKg = p.QtdeAtualKg,
        PercentualOcupacao = p.PercentualOcupacao,
        Status = p.Status.ToString()
    };
}
