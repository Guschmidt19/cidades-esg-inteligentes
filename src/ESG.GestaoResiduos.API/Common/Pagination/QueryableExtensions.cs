using ESG.GestaoResiduos.API.Common.Pagination;
using Microsoft.EntityFrameworkCore;

namespace ESG.GestaoResiduos.API.Common.Pagination;

/// <summary>
/// Extensões de paginação aplicadas diretamente sobre IQueryable, garantindo que
/// a contagem (COUNT) e o recorte (OFFSET/FETCH NEXT) sejam traduzidos para SQL
/// e executados no banco — otimização essencial para grandes volumes de dados.
/// </summary>
public static class QueryableExtensions
{
    public static async Task<PagedResult<T>> ToPagedResultAsync<T>(
        this IQueryable<T> query,
        int pageNumber,
        int pageSize,
        CancellationToken cancellationToken = default)
    {
        var totalItems = await query.CountAsync(cancellationToken);

        var items = await query
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync(cancellationToken);

        return new PagedResult<T>(items, totalItems, pageNumber, pageSize);
    }
}
