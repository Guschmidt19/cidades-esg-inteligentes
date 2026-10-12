using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.ViewModels;

namespace ESG.GestaoResiduos.API.Services.Interfaces;

public interface IAlertaService
{
    Task<PagedResult<AlertaResponse>> ListarAsync(AlertaFiltro filtro, CancellationToken ct = default);
    Task<AtenderAlertaResponse> AtenderAsync(int idAlerta, CancellationToken ct = default);
}
