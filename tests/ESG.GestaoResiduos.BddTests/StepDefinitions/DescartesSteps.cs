using System.Globalization;
using ESG.GestaoResiduos.BddTests.Support;
using Reqnroll;

namespace ESG.GestaoResiduos.BddTests.StepDefinitions;

[Binding]
public class DescartesSteps
{
    private readonly ApiContext _ctx;

    public DescartesSteps(ApiContext ctx) => _ctx = ctx;


    [Given("que estou autenticado como {string} com a senha {string}")]
    public async Task QueEstouAutenticadoComo(string usuario, string senha)
    {
        await _ctx.PostAsync("/api/auth/login", new { username = usuario, senha });
        Assert.True(_ctx.StatusCode == 200,
            $"Falha no login de preparação ({_ctx.StatusCode}). Corpo: {_ctx.ResponseBody}");
        _ctx.Token = _ctx.ObterCampo("token");
    }

    [Given("que não estou autenticado")]
    public void QueNaoEstouAutenticado() => _ctx.Token = null;

    [Given("que o ponto de coleta {int} não possui alerta pendente")]
    public async Task QueOPontoNaoPossuiAlertaPendente(int idPonto)
    {
        await _ctx.GetAsync($"/api/alertas?idPonto={idPonto}&status=PENDENTE&pageSize=100");

        var pendentes = _ctx.ObterLista("items")
            .Select(alerta => alerta.GetProperty("idAlerta").GetInt32())
            .ToList();

        foreach (var idAlerta in pendentes)
        {
            await _ctx.PutAsync($"/api/alertas/{idAlerta}/atender");
            Assert.True(_ctx.StatusCode == 200,
                $"Não foi possível atender o alerta {idAlerta} ({_ctx.StatusCode}). Corpo: {_ctx.ResponseBody}");
        }
    }


    [When("consulto a lista de descartes")]
    public Task ConsultoAListaDeDescartes() => _ctx.GetAsync("/api/descartes");

    [When("registro um descarte de {int} kg de {string} no ponto {int} pelo gerador {int}")]
    public Task RegistroUmDescarte(int pesoKg, string tipoResiduo, int idPonto, int idGerador) =>
        _ctx.PostAsync("/api/descartes", new { idPonto, idGerador, tipoResiduo, pesoKg });

    [When("registro um descarte que leva o ponto {int} a {int}% da capacidade, de {string} pelo gerador {int}")]
    public async Task RegistroUmDescarteQueLevaOPontoA(int idPonto, int percentual, string tipoResiduo, int idGerador)
    {
        await _ctx.GetAsync($"/api/pontos-coleta/{idPonto}");
        var capacidade = LerDecimal("capacidadeMaxKg");
        var atual = LerDecimal("qtdeAtualKg");

        var pesoNecessario = capacidade * percentual / 100m - atual;
        var pesoKg = Math.Ceiling(pesoNecessario * 100m) / 100m;
        Assert.True(pesoKg > 0, $"O ponto {idPonto} já está acima de {percentual}% (atual: {atual} kg).");

        await _ctx.PostAsync("/api/descartes", new { idPonto, idGerador, tipoResiduo, pesoKg });
    }


    [Then("existe um alerta pendente para o ponto {int}")]
    public async Task ExisteUmAlertaPendenteParaOPonto(int idPonto)
    {
        await _ctx.GetAsync($"/api/alertas?idPonto={idPonto}&status=PENDENTE");
        Assert.NotEmpty(_ctx.ObterLista("items"));
    }

    private decimal LerDecimal(string campo)
    {
        var texto = _ctx.ObterCampo(campo);
        Assert.False(string.IsNullOrEmpty(texto), $"Campo '{campo}' ausente. Corpo: {_ctx.ResponseBody}");
        return decimal.Parse(texto!, CultureInfo.InvariantCulture);
    }
}