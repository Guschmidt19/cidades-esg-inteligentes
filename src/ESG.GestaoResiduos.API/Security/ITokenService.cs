using ESG.GestaoResiduos.API.Models;

namespace ESG.GestaoResiduos.API.Security;

public interface ITokenService
{
    string GerarToken(Usuario usuario);
}
