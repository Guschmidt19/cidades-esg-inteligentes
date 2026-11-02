using ESG.GestaoResiduos.BddTests.Support;
using Reqnroll;

namespace ESG.GestaoResiduos.BddTests.StepDefinitions;

[Binding]
public class ComplianceSteps
{
    private readonly ApiContext _ctx;

    public ComplianceSteps(ApiContext ctx) => _ctx = ctx;

    [Given("que existe ao menos um descarte registrado")]
    public async Task QueExisteAoMenosUmDescarteRegistrado()
    {
        await _ctx.PostAsync("/api/auth/login", new { username = "operador", senha = "Operador@123" });
        Assert.True(_ctx.StatusCode == 200, $"Falha no login de preparação. Corpo: {_ctx.ResponseBody}");
        _ctx.Token = _ctx.ObterCampo("token");

        await _ctx.PostAsync("/api/descartes",
            new { idPonto = 1, idGerador = 1, tipoResiduo = "PLASTICO", pesoKg = 1 });
        Assert.True(_ctx.StatusCode == 201, $"Falha no descarte de preparação. Corpo: {_ctx.ResponseBody}");
    }

    [When("consulto o dashboard")]
    public Task ConsultoODashboard() => _ctx.GetAsync("/api/dashboard");

    [When("consulto a lista de relatórios de compliance")]
    public Task ConsultoAListaDeRelatorios() => _ctx.GetAsync("/api/relatorios-compliance");

    [When("consulto os relatórios de compliance do ano {int}")]
    public Task ConsultoOsRelatoriosDoAno(int ano) => _ctx.GetAsync($"/api/relatorios-compliance?ano={ano}");

    [When("consulto o consolidado de compliance")]
    public Task ConsultoOConsolidado() => _ctx.GetAsync("/api/relatorios-compliance/consolidado");

    [When("consulto o consolidado de compliance do ano {int}")]
    public Task ConsultoOConsolidadoDoAno(int ano) => _ctx.GetAsync($"/api/relatorios-compliance/consolidado?ano={ano}");

    [When("consulto o endpoint {string}")]
    public Task ConsultoOEndpoint(string caminho) => _ctx.GetAsync(caminho);
}