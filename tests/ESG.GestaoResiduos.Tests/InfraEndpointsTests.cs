using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ESG.GestaoResiduos.Tests;

/// <summary>
/// Testes dos endpoints de infraestrutura usados pelo pipeline de CI/CD
/// (health check e informações de versão/ambiente do deploy).
/// </summary>
public class InfraEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public InfraEndpointsTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Health_ReturnsHealthy()
    {
        // Act
        var response = await _client.GetAsync("/health");
        var body = await response.Content.ReadAsStringAsync();

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Contains("Healthy", body);
    }

    [Fact]
    public async Task HealthLive_ReturnsHttpStatusCode200()
    {
        var response = await _client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task Info_ReturnsAmbienteEVersao()
    {
        // Act
        var json = await _client.GetFromJsonAsync<JsonElement>("/api/info");

        // Assert
        Assert.Equal("Testing", json.GetProperty("ambiente").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("versao").GetString()));
    }
}
