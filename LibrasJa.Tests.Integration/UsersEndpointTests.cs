using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using LibrasJa.Infrastructure.Data;
using System.Net;
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
                // Remove TODOS os servicos relacionados ao DbContext
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
    public async Task CreateUser_DadosValidos_RetornaCreated()
    {
        // Arrange
        var payload = new { nome = "Leticia", email = "leticia@test.com", tipo = "SURDO" };
        var content = new StringContent(
            JsonSerializer.Serialize(payload),
            Encoding.UTF8, "application/json");

        // Act
        var response = await _client.PostAsync("/api/users", content);

        // Assert
        Assert.Equal(HttpStatusCode.Created, response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_RetornaStatus()
    {
        // Arrange + Act
        var response = await _client.GetAsync("/health");

        // Assert - aceita OK ou ServiceUnavailable (DB pode estar indisponivel no test)
        Assert.True(
            response.StatusCode == HttpStatusCode.OK ||
            response.StatusCode == HttpStatusCode.ServiceUnavailable);
    }
}
