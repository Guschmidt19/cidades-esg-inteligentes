namespace ESG.GestaoResiduos.API.Security;

/// <summary>Configurações de geração/validação do token JWT (seção "Jwt").</summary>
public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpirationMinutes { get; set; } = 120;
}
