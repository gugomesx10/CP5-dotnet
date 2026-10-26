using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.Exceptions;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Results;
using CP5.Application.Services;
using CP5.Domain.Entities;
using CP5.Domain.Exceptions;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CP5.UnitTests.Services;

public class JogadorServiceTests
{
    private readonly Mock<IJogadorRepository> _jogadores = new();
    private readonly Mock<ITimeRepository> _times = new();
    private readonly JogadorService _service;

    public JogadorServiceTests()
    {
        _service = new JogadorService(
            _jogadores.Object,
            _times.Object,
            NullLogger<JogadorService>.Instance);
    }

    private static Time TimeValido() => new()
    {
        Id = 1,
        Nome = "FURIA",
        Jogo = "CS2",
        Pais = "Brasil",
        Ranking = 3
    };

    private static JogadorCreateDto NovoJogador() => new()
    {
        Nickname = "KSCERATO",
        Funcao = "Rifler",
        Idade = 25,
        TimeId = 1
    };

    private static Jogador JogadorValido() => new()
    {
        Id = 12,
        Nickname = "KSCERATO",
        Funcao = "Rifler",
        Idade = 25,
        TimeId = 1,
        Time = TimeValido()
    };

    private static RepositoryException DbFailure() =>
        new("Falha simulada",
            new InvalidOperationException("Erro simulado"));

    [Fact]
    public async Task Create_DeveRetornar404_QuandoTimeNaoExistir()
    {
        _times
            .Setup(x => x.GetRequiredByIdAsync(1))
            .ThrowsAsync(new EntityNotFoundException("Time", 1));

        var result = await _service.CreateAsync(NovoJogador());

        Assert.Equal(OperationStatus.NotFound, result.Status);
        Assert.Equal("TIME_NOT_FOUND", result.ErrorCode);

        _jogadores.Verify(
            x => x.AddAsync(It.IsAny<Jogador>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_DeveSalvarEMapearJogador()
    {
        _times
            .Setup(x => x.GetRequiredByIdAsync(1))
            .ReturnsAsync(TimeValido());

        _jogadores
            .Setup(x => x.AddAsync(It.IsAny<Jogador>()))
            .Callback<Jogador>(j => j.Id = 12)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(NovoJogador());

        Assert.True(result.IsSuccess);
        Assert.Equal(12, result.Data!.Id);
        Assert.Equal("KSCERATO", result.Data.Nickname);
        Assert.Equal("FURIA", result.Data.Time!.Nome);

        _jogadores.Verify(
            x => x.AddAsync(It.Is<Jogador>(
                j => j.TimeId == 1 &&
                     j.Nickname == "KSCERATO")),
            Times.Once);
    }

    [Fact]
    public async Task Create_DeveRetornarDatabaseError()
    {
        _times
            .Setup(x => x.GetRequiredByIdAsync(1))
            .ReturnsAsync(TimeValido());

        _jogadores
            .Setup(x => x.AddAsync(It.IsAny<Jogador>()))
            .ThrowsAsync(DbFailure());

        var result = await _service.CreateAsync(NovoJogador());

        Assert.Equal(OperationStatus.UnexpectedError, result.Status);
        Assert.Equal("DATABASE_ERROR", result.ErrorCode);
    }

    [Fact]
    public async Task Update_DeveRetornar404_QuandoJogadorNaoExistir()
    {
        _jogadores
            .Setup(x => x.GetByIdForUpdateAsync(99))
            .ReturnsAsync((Jogador?)null);

        var result = await _service.UpdateAsync(
            99, NovoJogador());

        Assert.Equal("JOGADOR_NOT_FOUND", result.ErrorCode);

        _jogadores.Verify(
            x => x.UpdateAsync(It.IsAny<Jogador>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_DeveRetornar404_QuandoTimeNaoExistir()
    {
        _jogadores
            .Setup(x => x.GetByIdForUpdateAsync(12))
            .ReturnsAsync(JogadorValido());

        _times
            .Setup(x => x.GetRequiredByIdAsync(1))
            .ThrowsAsync(new EntityNotFoundException("Time", 1));

        var result = await _service.UpdateAsync(
            12, NovoJogador());

        Assert.Equal("TIME_NOT_FOUND", result.ErrorCode);

        _jogadores.Verify(
            x => x.UpdateAsync(It.IsAny<Jogador>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_DeveAlterarDados()
    {
        var jogador = JogadorValido();
        var dto = NovoJogador();
        dto.Nickname = "NovoNick";

        _jogadores
            .Setup(x => x.GetByIdForUpdateAsync(12))
            .ReturnsAsync(jogador);

        _times
            .Setup(x => x.GetRequiredByIdAsync(1))
            .ReturnsAsync(TimeValido());

        _jogadores
            .Setup(x => x.UpdateAsync(jogador))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(12, dto);

        Assert.True(result.IsSuccess);
        Assert.Equal("NovoNick", result.Data!.Nickname);

        _jogadores.Verify(
            x => x.UpdateAsync(jogador),
            Times.Once);
    }

    [Fact]
    public async Task Update_DeveTratarFalhaDoBanco()
    {
        var jogador = JogadorValido();

        _jogadores
            .Setup(x => x.GetByIdForUpdateAsync(12))
            .ReturnsAsync(jogador);

        _times
            .Setup(x => x.GetRequiredByIdAsync(1))
            .ReturnsAsync(TimeValido());

        _jogadores
            .Setup(x => x.UpdateAsync(jogador))
            .ThrowsAsync(DbFailure());

        var result = await _service.UpdateAsync(
            12, NovoJogador());

        Assert.Equal("DATABASE_ERROR", result.ErrorCode);
    }

    [Fact]
    public async Task Delete_DeveRetornar404_QuandoJogadorNaoExistir()
    {
        _jogadores
            .Setup(x => x.GetByIdForUpdateAsync(99))
            .ReturnsAsync((Jogador?)null);

        var result = await _service.DeleteAsync(99);

        Assert.Equal(OperationStatus.NotFound, result.Status);

        _jogadores.Verify(
            x => x.DeleteAsync(It.IsAny<Jogador>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_DeveRemoverJogador()
    {
        var jogador = JogadorValido();

        _jogadores
            .Setup(x => x.GetByIdForUpdateAsync(12))
            .ReturnsAsync(jogador);

        _jogadores
            .Setup(x => x.DeleteAsync(jogador))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(12);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        _jogadores.Verify(
            x => x.DeleteAsync(jogador),
            Times.Once);
    }

    [Fact]
    public async Task Delete_DeveTratarFalhaDoBanco()
    {
        var jogador = JogadorValido();

        _jogadores
            .Setup(x => x.GetByIdForUpdateAsync(12))
            .ReturnsAsync(jogador);

        _jogadores
            .Setup(x => x.DeleteAsync(jogador))
            .ThrowsAsync(DbFailure());

        var result = await _service.DeleteAsync(12);

        Assert.Equal("DATABASE_ERROR", result.ErrorCode);
    }

    [Fact]
    public async Task GetById_DeveRetornarJogador()
    {
        _jogadores
            .Setup(x => x.GetByIdAsync(12))
            .ReturnsAsync(JogadorValido());

        var result = await _service.GetByIdAsync(12);

        Assert.True(result.IsSuccess);
        Assert.Equal("KSCERATO", result.Data!.Nickname);
    }

    [Fact]
    public async Task GetById_DeveRetornar404()
    {
        _jogadores
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((Jogador?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Equal("JOGADOR_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task GetAll_DeveRetornarListaPaginada()
    {
        _jogadores
            .Setup(x => x.GetAllAsync(1, 10))
            .ReturnsAsync(new PagedResult<Jogador>
            {
                Items = new[] { JogadorValido() },
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 1,
                TotalPages = 1
            });

        var result = await _service.GetAllAsync(1, 10);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!.Items);
    }

    [Fact]
    public async Task GetAll_DeveTratarFalhaDoBanco()
    {
        _jogadores
            .Setup(x => x.GetAllAsync(1, 10))
            .ThrowsAsync(DbFailure());

        var result = await _service.GetAllAsync(1, 10);

        Assert.Equal("DATABASE_ERROR", result.ErrorCode);
    }
}