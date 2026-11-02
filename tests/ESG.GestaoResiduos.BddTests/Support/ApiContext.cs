using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace ESG.GestaoResiduos.BddTests.Support;

/// <summary>
/// Estado de UM cenário: cliente HTTP, token JWT e a última resposta da API.
/// O Reqnroll cria uma instância nova por cenário (isolamento entre testes).
/// </summary>
public class ApiContext : IDisposable
{
    public static readonly string BaseUrl =
        Environment.GetEnvironmentVariable("API_BASE_URL") ?? "http://localhost:8080";

    private readonly HttpClient _http = new()
    {
        BaseAddress = new Uri(BaseUrl),
        Timeout = TimeSpan.FromSeconds(30)
    };

    public string? Token { get; set; }
    public HttpResponseMessage? Response { get; private set; }
    public string ResponseBody { get; private set; } = string.Empty;
    public JsonDocument? Json { get; private set; }

    public int StatusCode => (int)(Response?.StatusCode ?? 0);

    public Task GetAsync(string caminho) => EnviarAsync(HttpMethod.Get, caminho, null);
    public Task PostAsync(string caminho, object corpo) => EnviarAsync(HttpMethod.Post, caminho, corpo);
    public Task PutAsync(string caminho) => EnviarAsync(HttpMethod.Put, caminho, null);

    private async Task EnviarAsync(HttpMethod metodo, string caminho, object? corpo)
    {
        using var requisicao = new HttpRequestMessage(metodo, caminho);

        if (!string.IsNullOrEmpty(Token))
            requisicao.Headers.Authorization = new AuthenticationHeaderValue("Bearer", Token);

        if (corpo is not null)
            requisicao.Content = new StringContent(
                JsonSerializer.Serialize(corpo), Encoding.UTF8, "application/json");

        Response?.Dispose();
        Response = await _http.SendAsync(requisicao);
        ResponseBody = await Response.Content.ReadAsStringAsync();

        Json?.Dispose();
        Json = TentarLerJson(ResponseBody);
    }

    private static JsonDocument? TentarLerJson(string texto)
    {
        try { return string.IsNullOrWhiteSpace(texto) ? null : JsonDocument.Parse(texto); }
        catch (JsonException) { return null; }
    }

    public string? ObterCampo(string nome)
    {
        if (Json is null || Json.RootElement.ValueKind != JsonValueKind.Object) return null;

        foreach (var propriedade in Json.RootElement.EnumerateObject())
        {
            if (string.Equals(propriedade.Name, nome, StringComparison.OrdinalIgnoreCase))
                return propriedade.Value.ValueKind == JsonValueKind.String
                    ? propriedade.Value.GetString()
                    : propriedade.Value.GetRawText();
        }
        return null;
    }
        public bool TemCampo(string nome) => ObterCampo(nome) is not null;

        public List<JsonElement> ObterLista(string nome)
        {
            if (Json is null || Json.RootElement.ValueKind != JsonValueKind.Object)
                return new List<JsonElement>();

            foreach (var propriedade in Json.RootElement.EnumerateObject())
            {
                if (string.Equals(propriedade.Name, nome, StringComparison.OrdinalIgnoreCase)
                    && propriedade.Value.ValueKind == JsonValueKind.Array)
                    return propriedade.Value.EnumerateArray().ToList();
            }
            return new List<JsonElement>();
        }

    public void Dispose()
    {
        Json?.Dispose();
        Response?.Dispose();
        _http.Dispose();
    }
}