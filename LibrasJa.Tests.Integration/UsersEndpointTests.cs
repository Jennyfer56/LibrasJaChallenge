using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LibrasJa.Infrastructure.Data;
using System.Net;
using System.Net.Http.Headers;
using System.Text;
using System.Text.Json;

namespace LibrasJa.Tests.Integration;

public class UsersEndpointTests : IClassFixture<WebApplicationFactory<Program>>
{
    private readonly HttpClient _client;

    public UsersEndpointTests(WebApplicationFactory<Program> factory)
    {
        _client = factory.WithWebHostBuilder(builder =>
        {
            builder.ConfigureServices(services =>
            {
                var toRemove = services
                    .Where(d => d.ServiceType.FullName != null &&
                        (d.ServiceType.FullName.Contains("DbContext") ||
                         d.ServiceType.FullName.Contains("EntityFramework") ||
                         d.ServiceType.FullName.Contains("Oracle") ||
                         d.ServiceType.FullName.Contains("Database")))
                    .ToList();

                foreach (var s in toRemove)
                    services.Remove(s);

                services.AddDbContext<AppDbContext>(options =>
                    options.UseInMemoryDatabase("TestDb_" + Guid.NewGuid()));
            });
        }).CreateClient();
    }

    // --------- Helper: faz login e retorna o token JWT ---------
    private async Task<string> ObterTokenAsync(string username = "jennyfer", string password = "1234")
    {
        var payload = new { username, password };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8, "application/json");

        var response = await _client.PostAsync("/api/auth/login", content);
        response.EnsureSuccessStatusCode();

        var body = await response.Content.ReadAsStringAsync();
        var doc  = JsonDocument.Parse(body);
        return doc.RootElement.GetProperty("token").GetString()!;
    }

    [Fact]
    public async Task GetUsers_SemFiltros_RetornaOk()
    {
        // Arrange + Act
        var response = await _client.GetAsync("/api/users");

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
    }

    [Fact]
    public async Task GetUser_IdInexistente_RetornaNotFound()
    {
        // Arrange + Act
        var response = await _client.GetAsync("/api/users/9999");

        // Assert
        Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_DadosValidos_ComToken_RetornaCreated()
    {
        // Arrange
        var token = await ObterTokenAsync();
        var payload = new { nome = "Leticia", email = "leticia@test.com", tipo = "SURDO" };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8, "application/json");

        var request = new HttpRequestMessage(HttpMethod.Post, "/api/users") { Content = content };
        request.Headers.Authorization = new AuthenticationHeaderValue("Bearer", token);

        // Act
        var response = await _client.SendAsync(request);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task CreateUser_SemToken_RetornaUnauthorized()
    {
        // Arrange
        var payload = new { nome = "Bloqueado", email = "x@test.com", tipo = "SURDO" };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Login_CredenciaisValidas_RetornaToken()
    {
        // Arrange
        var payload = new { username = "jennyfer", password = "1234" };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var body = await response.Content.ReadAsStringAsync();
        Assert.Contains("token", body);
    }

    [Fact]
    public async Task Login_SenhaInvalida_RetornaUnauthorized()
    {
        // Arrange
        var payload = new { username = "jennyfer", password = "senha-errada" };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/auth/login", content);

        // Assert
        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_RetornaStatus()
    {
        // Arrange + Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.ServiceUnavailable);
    }
}
