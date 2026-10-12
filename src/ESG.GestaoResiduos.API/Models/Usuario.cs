namespace ESG.GestaoResiduos.API.Models;

/// <summary>
/// Usuário usado exclusivamente para autenticação/autorização (emissão de JWT).
/// Não há CRUD exposto desta entidade — apenas o fluxo de login.
/// </summary>
public class Usuario
{
    public int IdUsuario { get; set; }
    public string Username { get; set; } = string.Empty;
    public string SenhaHash { get; set; } = string.Empty;
    public string Role { get; set; } = "Operador";
}
