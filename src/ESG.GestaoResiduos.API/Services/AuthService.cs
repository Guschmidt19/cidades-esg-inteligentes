using ESG.GestaoResiduos.API.Common.Exceptions;
using ESG.GestaoResiduos.API.Data;
using ESG.GestaoResiduos.API.Security;
using ESG.GestaoResiduos.API.Services.Interfaces;
using ESG.GestaoResiduos.API.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;

namespace ESG.GestaoResiduos.API.Services;

public class AuthService : IAuthService
{
    private readonly AppDbContext _db;
    private readonly ITokenService _tokenService;
    private readonly JwtSettings _jwt;

    public AuthService(AppDbContext db, ITokenService tokenService, IOptions<JwtSettings> jwt)
    {
        _db = db;
        _tokenService = tokenService;
        _jwt = jwt.Value;
    }

    public async Task<LoginResponse> AutenticarAsync(LoginRequest request, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Username == request.Username, ct);

        if (usuario is null || !BCrypt.Net.BCrypt.Verify(request.Senha, usuario.SenhaHash))
            throw new BusinessRuleException("Usuário ou senha inválidos.");

        var token = _tokenService.GerarToken(usuario);

        return new LoginResponse
        {
            Token = token,
            ExpiraEm = DateTime.UtcNow.AddMinutes(_jwt.ExpirationMinutes),
            Username = usuario.Username,
            Role = usuario.Role
        };
    }
}
