using ESG.GestaoResiduos.API.Common.Pagination;
using ESG.GestaoResiduos.API.ViewModels;

namespace ESG.GestaoResiduos.API.Services.Interfaces;

public interface IDescarteService
{
    Task<DescarteResultadoResponse> RegistrarAsync(RegistrarDescarteRequest request, CancellationToken ct = default);
    Task<PagedResult<DescarteResponse>> ListarAsync(PaginationParams pagination, int? idPonto, CancellationToken ct = default);
}
