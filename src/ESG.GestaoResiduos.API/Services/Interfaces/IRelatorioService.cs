using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.ViewModels;

namespace ESG.GestaoResiduos.API.Services.Interfaces;

public interface IRelatorioService
{
    Task<PagedResult<RelatorioComplianceResponse>> ListarAsync(RelatorioFiltro filtro, CancellationToken ct = default);
    Task<IReadOnlyList<RelatorioConsolidadoResponse>> ConsolidarAsync(int? ano, CancellationToken ct = default);
}
