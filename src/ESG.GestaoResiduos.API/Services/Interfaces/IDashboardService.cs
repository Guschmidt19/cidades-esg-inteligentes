using ESG.GestaoResiduos.API.ViewModels;

namespace ESG.GestaoResiduos.API.Services.Interfaces;

public interface IDashboardService
{
    Task<DashboardResponse> ObterAsync(CancellationToken ct = default);
}
