using System.Globalization;
using ESG.GestaoResiduos.BddTests.Support;
using Reqnroll;

namespace ESG.GestaoResiduos.BddTests.StepDefinitions;

[Binding]
public class AlertasSteps
{
    private readonly ApiContext _ctx;
    private int _idAlertaCriado;

    public AlertasSteps(ApiContext ctx) => _ctx = ctx;


    [Given("que o ponto de coleta {int} possui um alerta pendente")]
    public async Task QueOPontoPossuiUmAlertaPendente(int idPonto)
    {
        await _ctx.GetAsync($"/api/alertas?idPonto={idPonto}&status=PENDENTE&pageSize=100");
        var antigos = _ctx.ObterLista("items")
            .Select(a => a.GetProperty("idAlerta").GetInt32())
            .ToList();

        foreach (var id in antigos)
        {
            await _ctx.PutAsync($"/api/alertas/{id}/atender");
            Assert.True(_ctx.StatusCode == 200, $"Falha ao limpar o alerta {id}. Corpo: {_ctx.ResponseBody}");
        }

        await _ctx.GetAsync($"/api/pontos-coleta/{idPonto}");
        var tipoResiduo = _ctx.ObterCampo("tipoResiduo")!;
        var capacidade = LerDecimal("capacidadeMaxKg");
        var atual = LerDecimal("qtdeAtualKg");

        var pesoKg = Math.Ceiling((capacidade * 0.85m - atual) * 100m) / 100m;
        await _ctx.PostAsync("/api/descartes", new { idPonto, idGerador = 1, tipoResiduo, pesoKg });
        Assert.True(_ctx.StatusCode == 201, $"Falha ao criar o descarte de preparação. Corpo: {_ctx.ResponseBody}");

        await _ctx.GetAsync($"/api/alertas?idPonto={idPonto}&status=PENDENTE");
        var alertas = _ctx.ObterLista("items");
        Assert.NotEmpty(alertas);
        _idAlertaCriado = alertas[0].GetProperty("idAlerta").GetInt32();
    }


    [When("consulto a lista de alertas")]
    public Task ConsultoAListaDeAlertas() => _ctx.GetAsync("/api/alertas");

    [When("consulto os alertas com status {string}")]
    public Task ConsultoOsAlertasComStatus(string status) => _ctx.GetAsync($"/api/alertas?status={status}");

    [When("atendo o alerta de id {int}")]
    public Task AtendoOAlertaDeId(int idAlerta) => _ctx.PutAsync($"/api/alertas/{idAlerta}/atender");

    [When("atendo o alerta pendente")]
    public Task AtendoOAlertaPendente() => _ctx.PutAsync($"/api/alertas/{_idAlertaCriado}/atender");

    [When("atendo novamente o mesmo alerta")]
    public Task AtendoNovamenteOMesmoAlerta() => _ctx.PutAsync($"/api/alertas/{_idAlertaCriado}/atender");


    [Then("a quantidade atual do ponto deve ser zero")]
    public void AQuantidadeAtualDoPontoDeveSerZero()
    {
        Assert.Equal(0m, LerDecimal("qtdeAtualPonto"));
    }

    private decimal LerDecimal(string campo)
    {
        var texto = _ctx.ObterCampo(campo);
        Assert.False(string.IsNullOrEmpty(texto), $"Campo '{campo}' ausente. Corpo: {_ctx.ResponseBody}");
        return decimal.Parse(texto!, CultureInfo.InvariantCulture);
    }
}