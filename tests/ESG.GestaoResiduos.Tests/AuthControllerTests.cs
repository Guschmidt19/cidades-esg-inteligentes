using System.Net.Http.Json;
using ESG.GestaoResiduos.API.ViewModels;
using Xunit;

namespace ESG.GestaoResiduos.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public AuthControllerTests(CustomWebApplicationFactory factory) => _client = factory.CreateClient();

    [Fact]
    public async Task Login_ReturnsHttpStatusCode200()
    {
        // Arrange
        var request = "/api/auth/login";
        var body = new LoginRequest { Username = "admin", Senha = "Admin@123" };

        // Act
        var response = await _client.PostAsJsonAsync(request, body);

        // Assert
        response.EnsureSuccessStatusCode(); // Verifica se o status code é 200
    }
}
