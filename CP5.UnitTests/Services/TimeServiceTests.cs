using CP5.Application.Exceptions;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Results;
using CP5.Application.Services;
using CP5.Application.DTOs;
using CP5.Domain.Entities;
using CP5.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CP5.UnitTests.Services;

public class TimeServiceTests
{
    private readonly Mock<ITimeRepository> _repositoryMock;
    private readonly TimeService _service;

    public TimeServiceTests()
    {
        _repositoryMock = new Mock<ITimeRepository>();

        _service = new TimeService(
            _repositoryMock.Object,
            NullLogger<TimeService>.Instance);
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarSucesso_QuandoTimeExistir()
    {
        // Arrange
        var time = new Time
        {
            Id = 1,
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        _repositoryMock
            .Setup(r => r.GetRequiredByIdAsync(1))
            .ReturnsAsync(time);

        // Act
        var resultado = await _service.GetByIdAsync(1);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(OperationStatus.Success, resultado.Status);
        Assert.NotNull(resultado.Data);
        Assert.Equal("FURIA", resultado.Data.Nome);
        Assert.Equal(1, resultado.Data.Id);
        Assert.Null(resultado.ErrorCode);

        _repositoryMock.Verify(
            r => r.GetRequiredByIdAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarNotFound_QuandoTimeNaoExistir()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetRequiredByIdAsync(999))
            .ThrowsAsync(
                new EntityNotFoundException("Time", 999));

        // Act
        var resultado = await _service.GetByIdAsync(999);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(OperationStatus.NotFound, resultado.Status);
        Assert.Equal("TIME_NOT_FOUND", resultado.ErrorCode);
        Assert.Null(resultado.Data);
    }

    [Fact]
    public async Task GetByIdAsync_DeveRetornarErro_QuandoRepositorioFalhar()
    {
        // Arrange
        var exception = new RepositoryException(
            "Falha ao consultar o banco de dados.",
            new InvalidOperationException("Erro simulado"));

        _repositoryMock
            .Setup(r => r.GetRequiredByIdAsync(1))
            .ThrowsAsync(exception);

        // Act
        var resultado = await _service.GetByIdAsync(1);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            OperationStatus.UnexpectedError,
            resultado.Status);
        Assert.Equal("DATABASE_ERROR", resultado.ErrorCode);
        Assert.Null(resultado.Data);
    }
    
    [Fact]
    public async Task UpdateAsync_DeveRetornarSucesso_QuandoTimeExistir()
    {
        // Arrange
        var time = new Time
        {
            Id = 1,
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        var dto = new CP5.Application.DTOs.TimeCreateDto
        {
            Nome = "FURIA Atualizada",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 1
        };

        _repositoryMock
            .Setup(r => r.GetRequiredByIdAsync(1))
            .ReturnsAsync(time);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Time>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(OperationStatus.Success, resultado.Status);
        Assert.NotNull(resultado.Data);
        Assert.Equal("FURIA Atualizada", resultado.Data.Nome);
        Assert.Equal(1, resultado.Data.Ranking);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.Is<Time>(
                t => t.Nome == dto.Nome && t.Ranking == dto.Ranking)),
            Times.Once);
    }
    
    [Fact]
    public async Task UpdateAsync_DeveRetornarNotFound_QuandoTimeNaoExistir()
    {
        // Arrange
        var dto = new CP5.Application.DTOs.TimeCreateDto
        {
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 1
        };

        _repositoryMock
            .Setup(r => r.GetRequiredByIdAsync(999))
            .ThrowsAsync(
                new EntityNotFoundException("Time", 999));

        // Act
        var resultado = await _service.UpdateAsync(999, dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(OperationStatus.NotFound, resultado.Status);
        Assert.Equal("TIME_NOT_FOUND", resultado.ErrorCode);

        _repositoryMock.Verify(
            r => r.UpdateAsync(It.IsAny<Time>()),
            Times.Never);
    }
    
    [Fact]
    public async Task UpdateAsync_DeveRetornarErro_QuandoPersistenciaFalhar()
    {
        // Arrange
        var time = new Time
        {
            Id = 1,
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        var dto = new CP5.Application.DTOs.TimeCreateDto
        {
            Nome = "FURIA Atualizada",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 1
        };

        _repositoryMock
            .Setup(r => r.GetRequiredByIdAsync(1))
            .ReturnsAsync(time);

        _repositoryMock
            .Setup(r => r.UpdateAsync(It.IsAny<Time>()))
            .ThrowsAsync(new RepositoryException(
                "Falha ao atualizar o time.",
                new InvalidOperationException("Erro simulado")));

        // Act
        var resultado = await _service.UpdateAsync(1, dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            OperationStatus.UnexpectedError,
            resultado.Status);
        Assert.Equal("DATABASE_ERROR", resultado.ErrorCode);
        Assert.Null(resultado.Data);
    }
    
    [Fact]
    public async Task DeleteAsync_DeveRetornarSucesso_QuandoTimeExistir()
    {
        // Arrange
        var time = new Time
        {
            Id = 1,
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        _repositoryMock
            .Setup(r => r.GetRequiredByIdAsync(1))
            .ReturnsAsync(time);

        _repositoryMock
            .Setup(r => r.DeleteAsync(time))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.DeleteAsync(1);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(OperationStatus.Success, resultado.Status);
        Assert.True(resultado.Data);
        Assert.Null(resultado.ErrorCode);

        _repositoryMock.Verify(
            r => r.DeleteAsync(time),
            Times.Once);
    }
    
    [Fact]
    public async Task DeleteAsync_DeveRetornarNotFound_QuandoTimeNaoExistir()
    {
        // Arrange
        _repositoryMock
            .Setup(r => r.GetRequiredByIdAsync(999))
            .ThrowsAsync(
                new EntityNotFoundException("Time", 999));

        // Act
        var resultado = await _service.DeleteAsync(999);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(OperationStatus.NotFound, resultado.Status);
        Assert.Equal("TIME_NOT_FOUND", resultado.ErrorCode);

        _repositoryMock.Verify(
            r => r.DeleteAsync(It.IsAny<Time>()),
            Times.Never);
    }
    
    [Fact]
    public async Task DeleteAsync_DeveRetornarErro_QuandoPersistenciaFalhar()
    {
        // Arrange
        var time = new Time
        {
            Id = 1,
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        _repositoryMock
            .Setup(r => r.GetRequiredByIdAsync(1))
            .ReturnsAsync(time);

        _repositoryMock
            .Setup(r => r.DeleteAsync(time))
            .ThrowsAsync(
                new RepositoryException(
                    "Falha ao excluir o time.",
                    new InvalidOperationException("Erro simulado")));

        // Act
        var resultado = await _service.DeleteAsync(1);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            OperationStatus.UnexpectedError,
            resultado.Status);
        Assert.Equal("DATABASE_ERROR", resultado.ErrorCode);
        Assert.False(resultado.IsSuccess);
    }
    
    [Fact]
    public async Task CreateAsync_DeveRetornarSucesso_QuandoDadosForemValidos()
    {
        // Arrange
        var dto = new TimeCreateDto
        {
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Time>()))
            .Callback<Time>(time => time.Id = 10)
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.CreateAsync(dto);

        // Assert
        Assert.True(resultado.IsSuccess);
        Assert.Equal(OperationStatus.Success, resultado.Status);
        Assert.NotNull(resultado.Data);
        Assert.Equal(10, resultado.Data.Id);
        Assert.Equal("FURIA", resultado.Data.Nome);
        Assert.Equal("CS2", resultado.Data.Jogo);
        Assert.Null(resultado.ErrorCode);
    }
    
    [Fact]
    public async Task CreateAsync_DeveRetornarErro_QuandoBancoFalhar()
    {
        // Arrange
        var dto = new TimeCreateDto
        {
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Time>()))
            .ThrowsAsync(
                new RepositoryException(
                    "Falha ao cadastrar o time.",
                    new InvalidOperationException("Erro simulado")));

        // Act
        var resultado = await _service.CreateAsync(dto);

        // Assert
        Assert.False(resultado.IsSuccess);
        Assert.Equal(
            OperationStatus.UnexpectedError,
            resultado.Status);
        Assert.Equal("DATABASE_ERROR", resultado.ErrorCode);
        Assert.Null(resultado.Data);
    }
    
    [Fact]
    public async Task CreateAsync_DeveEnviarDadosCorretosAoRepositorio()
    {
        // Arrange
        var dto = new TimeCreateDto
        {
            Nome = "Team Liquid",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 5
        };

        _repositoryMock
            .Setup(r => r.AddAsync(It.IsAny<Time>()))
            .Returns(Task.CompletedTask);

        // Act
        var resultado = await _service.CreateAsync(dto);

        // Assert
        Assert.True(resultado.IsSuccess);

        _repositoryMock.Verify(
            r => r.AddAsync(It.Is<Time>(time =>
                time.Nome == dto.Nome &&
                time.Jogo == dto.Jogo &&
                time.Pais == dto.Pais &&
                time.Ranking == dto.Ranking)),
            Times.Once);
    }
    
    [Fact]
    public async Task GetAllAsync_DeveRetornarOperationResultComTimes()
    {
        var time = new Time
        {
            Id = 1,
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        _repositoryMock
            .Setup(r => r.GetAllAsync(1, 10))
            .ReturnsAsync(new CP5.Application.Common.PagedResult<Time>
            {
                Items = new[] { time },
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 1,
                TotalPages = 1
            });

        var result = await _service.GetAllAsync(1, 10);

        Assert.True(result.IsSuccess);
        Assert.Equal(OperationStatus.Success, result.Status);
        Assert.NotNull(result.Data);
        Assert.Single(result.Data.Items);
        Assert.Equal("FURIA", result.Data.Items.Single().Nome);
    }

    [Fact]
    public async Task GetByJogoAsync_DeveRetornarErroQuandoBancoFalhar()
    {
        _repositoryMock
            .Setup(r => r.GetByJogoAsync("CS2", 1, 10))
            .ThrowsAsync(new RepositoryException(
                "Falha no banco.",
                new InvalidOperationException("Erro simulado")));

        var result = await _service.GetByJogoAsync("CS2", 1, 10);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            OperationStatus.UnexpectedError,
            result.Status);
        Assert.Equal("DATABASE_ERROR", result.ErrorCode);
    }
}