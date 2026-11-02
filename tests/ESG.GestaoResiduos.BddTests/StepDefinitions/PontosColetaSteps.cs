using ESG.GestaoResiduos.BddTests.Support;
using Reqnroll;

namespace ESG.GestaoResiduos.BddTests.StepDefinitions;

[Binding]
public class PontosColetaSteps
{
    private readonly ApiContext _ctx;

    public PontosColetaSteps(ApiContext ctx) => _ctx = ctx;

    [When("consulto a lista de pontos de coleta")]
    public async Task ConsultoAListaDePontosDeColeta()
    {
        await _ctx.GetAsync("/api/pontos-coleta");
    }

    [When("consulto o ponto de coleta de id {int}")]
    public async Task ConsultoOPontoDeColetaDeId(int id)
    {
        await _ctx.GetAsync($"/api/pontos-coleta/{id}");
    }

    [When("consulto a lista de pontos filtrando pelo tipo {string}")]
    public async Task ConsultoAListaDePontosFiltrandoPeloTipo(string tipo)
    {
        await _ctx.GetAsync($"/api/pontos-coleta?tipoResiduo={tipo}");
    }
}