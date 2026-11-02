using ESG.GestaoResiduos.BddTests.Support;
using Reqnroll;
using NJsonSchema;
using System.Text.Json;

namespace ESG.GestaoResiduos.BddTests.StepDefinitions;

[Binding]
public class ComumSteps
{
    private readonly ApiContext _ctx;

    public ComumSteps(ApiContext ctx) => _ctx = ctx;

    [Then("o status da resposta deve ser {int}")]
    public void OStatusDaRespostaDeveSer(int esperado)
    {
        Assert.True(_ctx.StatusCode == esperado,
            $"Esperado status {esperado}, mas a API retornou {_ctx.StatusCode}. Corpo: {_ctx.ResponseBody}");
    }

    [Then("o campo {string} da resposta deve ser {string}")]
    public void OCampoDaRespostaDeveSer(string campo, string esperado)
    {
        var valorAtual = _ctx.ObterCampo(campo);
        Assert.Equal(esperado, valorAtual);
    }

        [Then("a resposta deve ser paginada")]
        public void ARespostaDeveSerPaginada()
        {
            foreach (var campo in new[] { "items", "pageNumber", "pageSize", "totalItems", "totalPages" })
            {
                Assert.True(_ctx.TemCampo(campo),
                    $"A resposta não é paginada: falta o campo '{campo}'. Corpo: {_ctx.ResponseBody}");
            }
        }

        [Then("a lista {string} não deve estar vazia")]
        public void AListaNaoDeveEstarVazia(string nomeDaLista)
        {
            Assert.NotEmpty(_ctx.ObterLista(nomeDaLista));
        }

        [Then("todos os itens da lista devem ter {string} igual a {string}")]
        public void TodosOsItensDaListaDevemTerIgualA(string campo, string esperado)
        {
            var itens = _ctx.ObterLista("items");
            Assert.NotEmpty(itens);

            foreach (var item in itens)
            {
                Assert.True(item.TryGetProperty(campo, out var valor), $"Item sem o campo '{campo}'.");
                Assert.Equal(esperado, valor.GetString());
            }
        }

    [Then("a resposta deve respeitar o contrato {string}")]
    public async Task ARespostaDeveRespeitarOContrato(string nomeDoContrato)
    {
        var caminho = Path.Combine(AppContext.BaseDirectory, "Schemas", $"{nomeDoContrato}.json");
        Assert.True(File.Exists(caminho), $"Arquivo de contrato não encontrado: {caminho}");

        var schema = await JsonSchema.FromJsonAsync(await File.ReadAllTextAsync(caminho));
        var violacoes = schema.Validate(_ctx.ResponseBody);

        Assert.True(violacoes.Count == 0,
            "Violações do contrato: " + string.Join("; ", violacoes.Select(v => $"{v.Path} ({v.Kind})")));
    }

    [Then("o campo {string} da resposta deve conter {string}")]
    public void OCampoDaRespostaDeveConter(string campo, string trecho)
    {
        var valor = _ctx.ObterCampo(campo);
        Assert.NotNull(valor);
        Assert.Contains(trecho, valor);
    }
        [Then("a lista {string} deve estar vazia")]
        public void AListaDeveEstarVazia(string nomeDaLista)
        {
            Assert.Empty(_ctx.ObterLista(nomeDaLista));
        }

        [Then("a resposta deve ser uma lista não vazia")]
        public void ARespostaDeveSerUmaListaNaoVazia()
        {
            Assert.NotNull(_ctx.Json);
            Assert.Equal(JsonValueKind.Array, _ctx.Json!.RootElement.ValueKind);
            Assert.True(_ctx.Json.RootElement.GetArrayLength() > 0, "A lista veio vazia.");
        }

        [Then("a resposta deve ser uma lista vazia")]
        public void ARespostaDeveSerUmaListaVazia()
        {
            Assert.NotNull(_ctx.Json);
            Assert.Equal(JsonValueKind.Array, _ctx.Json!.RootElement.ValueKind);
            Assert.Equal(0, _ctx.Json.RootElement.GetArrayLength());
        }
}