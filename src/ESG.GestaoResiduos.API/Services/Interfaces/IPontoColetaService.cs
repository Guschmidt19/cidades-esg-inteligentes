using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.ViewModels;

namespace ESG.GestaoResiduos.API.Services.Interfaces;

public interface IPontoColetaService
{
    Task<PagedResult<PontoColetaResponse>> ListarAsync(PontoColetaFiltro filtro, CancellationToken ct = default);
    Task<PontoColetaResponse> ObterPorIdAsync(int id, CancellationToken ct = default);
}
