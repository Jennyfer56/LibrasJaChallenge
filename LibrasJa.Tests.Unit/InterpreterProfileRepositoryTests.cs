using LibrasJa.Application.Interfaces;
using LibrasJa.Domain.Entities;
using Moq;

namespace LibrasJa.Tests.Unit;

public class InterpreterProfileRepositoryTests
{
    private readonly Mock<IInterpreterProfileRepository> _repoMock;

    public InterpreterProfileRepositoryTests()
    {
        _repoMock = new Mock<IInterpreterProfileRepository>();
    }

    [Fact]
    public async Task GetByIdAsync_PerfilExistente_RetornaPerfil()
    {
        // Arrange
        var profile = new InterpreterProfile
        {
            Id = 1, UserId = 1,
            Especialidades = "MEDICA",
            Disponivel = "SEGUNDA"
        };
        _repoMock.Setup(r => r.GetByIdAsync(1)).ReturnsAsync(profile);

        // Act
        var result = await _repoMock.Object.GetByIdAsync(1);

        // Assert
        Assert.NotNull(result);
        Assert.Equal("MEDICA", result.Especialidades);
    }

    [Fact]
    public async Task GetByIdAsync_PerfilInexistente_RetornaNull()
    {
        // Arrange
        _repoMock.Setup(r => r.GetByIdAsync(999)).ReturnsAsync((InterpreterProfile?)null);

        // Act
        var result = await _repoMock.Object.GetByIdAsync(999);

        // Assert
        Assert.Null(result);
    }

    [Fact]
    public async Task GetAllAsync_RetornaListaDePerfis()
    {
        // Arrange
        var profiles = new List<InterpreterProfile>
        {
            new InterpreterProfile { Id = 1, UserId = 1, Especialidades = "MEDICA" },
            new InterpreterProfile { Id = 2, UserId = 2, Especialidades = "JURIDICA" }
        };
        _repoMock.Setup(r => r.GetAllAsync()).ReturnsAsync(profiles);

        // Act
        var result = await _repoMock.Object.GetAllAsync();

        // Assert
        Assert.Equal(2, result.Count);
    }

    [Fact]
    public async Task AddAsync_PerfilValido_ChamadaRealizada()
    {
        // Arrange
        var profile = new InterpreterProfile { UserId = 1, Especialidades = "EDUCACAO" };
        _repoMock.Setup(r => r.AddAsync(profile)).Returns(Task.CompletedTask);

        // Act
        await _repoMock.Object.AddAsync(profile);

        // Assert
        _repoMock.Verify(r => r.AddAsync(profile), Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_IdExistente_ChamadaRealizada()
    {
        // Arrange
        _repoMock.Setup(r => r.DeleteAsync(1)).Returns(Task.CompletedTask);

        // Act
        await _repoMock.Object.DeleteAsync(1);

        // Assert
        _repoMock.Verify(r => r.DeleteAsync(1), Times.Once);
    }
}
