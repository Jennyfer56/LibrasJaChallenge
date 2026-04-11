using LibrasJa.Application.Interfaces;
using LibrasJa.Domain.Entities;
using Moq;

namespace LibrasJa.Tests.Unit;

public class UserRepositoryTests
{
    private readonly Mock<IUserRepository> _repoMock;

    public UserRepositoryTests()
    {
        _repoMock = new Mock<IUserRepository>();
    }

    [Fact]
    public async Task GetByIdAsync_UsuarioExistente_RetornaUsuario()
    {
        // Arrange
        var user = new User { Id = 1, Nome = "Jennyfer", Email = "jenny@test.com", Tipo = "SURDO" };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(user);

        // Act
        var result = await _repoMock.Object.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("Jennyfer", result.Nome);
        Assert.Equal("SURDO", result.Tipo);
    }

    [Fact]
    public async Task GetByIdAsync_UsuarioInexistente_RetornaNull()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((User?)null);

        // Act
        var result = await _repoMock.Object.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_RetornaListaDeUsuarios()
    {
        // Arrange
        var users = new List<User>
        {
            new User { Id = 1, Nome = "Jennyfer", Email = "jenny@test.com", Tipo = "SURDO" },
            new User { Id = 2, Nome = "Ivanildo", Email = "ivan@test.com", Tipo = "INTERPRETE" }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(users);

        // Act
        var result = await _repoMock.Object.GetAllAsync();

        // Assert
        Assert.NotNull(result);
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AddAsync_UsuarioValido_ChamadaRealizada()
    {
        // Arrange
        var user = new User { Nome = "Letícia", Email = "leti@test.com", Tipo = "SURDO" };
        _repoMock.Setup(r => r.AddAsync(user)).Returns(Task.CompletedTask);

        // Act
        await _repoMock.Object.AddAsync(user);

        // Assert
        _repoMock.Verify(r => r.AddAsync(user), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_UsuarioExistente_ChamadaRealizada()
    {
        // Arrange
        var user = new User { Id = 1, Nome = "Jennyfer", Email = "jenny@test.com", Tipo = "SURDO" };
        _repoMock.Setup(r => r.DeleteAsync(user)).Returns(Task.CompletedTask);

        // Act
        await _repoMock.Object.DeleteAsync(user);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(user), Times.Once);
    }
}
