using ESG.GestaoResiduos.API.ViewModels;

namespace ESG.GestaoResiduos.API.Services.Interfaces;

public interface IAuthService
{
    Task<LoginResponse> AutenticarAsync(LoginRequest request, CancellationToken ct = default);
}
