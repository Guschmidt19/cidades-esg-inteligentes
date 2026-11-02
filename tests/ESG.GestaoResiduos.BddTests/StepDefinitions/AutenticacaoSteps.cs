using ESG.GestaoResiduos.BddTests.Support;
using Reqnroll;

namespace ESG.GestaoResiduos.BddTests.StepDefinitions;

[Binding]
public class AutenticacaoSteps
{
    private readonly ApiContext _ctx;
    private string _usuario = string.Empty;
    private string _senha = string.Empty;

    public AutenticacaoSteps(ApiContext ctx) => _ctx = ctx;

    [Given("que informo o usuário {string} e a senha {string}")]
    public void QueInformoOUsuarioEASenha(string usuario, string senha)
    {
        _usuario = usuario;
        _senha = senha;
    }

    [When("envio a requisição de login")]
    public async Task EnvioARequisicaoDeLogin()
    {
        await _ctx.PostAsync("/api/auth/login", new { username = _usuario, senha = _senha});
    }

    [Then("a resposta deve conter um token JWT")]
    public void ARespostaDeveConterUmTokenJWT()
    {
        var token = _ctx.ObterCampo("token");
        Assert.False(string.IsNullOrWhiteSpace(token), "A API não retornou token.");
        Assert.Equal(3, token!.Split('.').Length);
    }

    [Then("o perfil retornado deve ser {string}")]
    public void OPerfilRetornadoDeveSer(string esperado)
    {
        Assert.Equal(esperado, _ctx.ObterCampo("role"));
    }
}