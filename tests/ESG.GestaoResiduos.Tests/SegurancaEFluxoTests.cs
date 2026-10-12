using System.Net;
using System.Net.Http.Headers;
using System.Net.Http.Json;
using ESG.GestaoResiduos.API.ViewModels;
using Xunit;

namespace ESG.GestaoResiduos.Tests;

/// <summary>
/// Testes de segurança (JWT) e do fluxo crítico de registro de descarte.
/// </summary>
public class SegurancaEFluxoTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public SegurancaEFluxoTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    private static object NovoDescarte() => new
    {
        idPonto = 1,
        idGerador = 1,
        tipoResiduo = "PLASTICO",
        pesoKg = 10.5m
    };

    [Fact]
    public async Task RegistrarDescarte_SemToken_Returns401()
    {
        var response = await _client.PostAsJsonAsync("/api/descartes", NovoDescarte());

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_SenhaInvalida_Returns422()
    {
        var body = new LoginRequest { Username = "admin", Senha = "senha-errada" };

        var response = await _client.PostAsJsonAsync("/api/auth/login", body);

        Assert.Equal(HttpStatusCode.UnprocessableEntity, response.StatusCode);
    }

    [Fact]
    public async Task RegistrarDescarte_ComToken_Returns201()
    {
        // Arrange — autentica e obtém o token JWT
        var login = await _client.PostAsJsonAsync("/api/auth/login",
            new LoginRequest { Username = "admin", Senha = "Admin@123" });
        login.EnsureSuccessStatusCode();
        var token = (await login.Content.ReadFromJsonAsync<LoginResponse>())!.Token;

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/descartes")
        {
            Content = JsonContent.Create(NovoDescarte())
        };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }
}
