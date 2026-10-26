using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.Exceptions;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Results;
using CP5.Application.Services;
using CP5.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CP5.UnitTests.Services;

public class PerfilCompetitivoServiceTests
{
    private readonly Mock<IPerfilCompetitivoRepository> _perfis = new();
    private readonly Mock<IJogadorRepository> _jogadores = new();
    private readonly PerfilCompetitivoService _service;

    public PerfilCompetitivoServiceTests()
    {
        _service = new PerfilCompetitivoService(
            _perfis.Object,
            _jogadores.Object,
            NullLogger<PerfilCompetitivoService>.Instance);
    }

    private static Jogador JogadorValido() => new()
    {
        Id = 1,
        Nickname = "KSCERATO",
        Funcao = "Rifler",
        Idade = 25,
        TimeId = 1,
        Time = new Time
        {
            Id = 1,
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        }
    };

    private static PerfilCompetitivoCreateDto NovoPerfil() => new()
    {
        JogadorId = 1,
        KDA = 1.32,
        WinRate = 68,
        HorasJogadas = 12000
    };

    private static PerfilCompetitivo PerfilValido() => new()
    {
        Id = 10,
        JogadorId = 1,
        KDA = 1.32,
        WinRate = 68,
        HorasJogadas = 12000,
        Jogador = JogadorValido()
    };

    private static RepositoryException DbFailure() =>
        new("Falha simulada",
            new InvalidOperationException("Erro simulado"));

    [Fact]
    public async Task Create_DeveRetornar404_QuandoJogadorNaoExistir()
    {
        _jogadores
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Jogador?)null);

        var result = await _service.CreateAsync(NovoPerfil());

        Assert.Equal("JOGADOR_NOT_FOUND", result.ErrorCode);

        _perfis.Verify(
            x => x.AddAsync(It.IsAny<PerfilCompetitivo>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_DeveRetornar409_QuandoPerfilJaExistir()
    {
        _jogadores
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(JogadorValido());

        _perfis
            .Setup(x => x.ExistsByJogadorAsync(1, null))
            .ReturnsAsync(true);

        var result = await _service.CreateAsync(NovoPerfil());

        Assert.Equal(OperationStatus.Conflict, result.Status);
        Assert.Equal("PERFIL_ALREADY_EXISTS", result.ErrorCode);

        _perfis.Verify(
            x => x.AddAsync(It.IsAny<PerfilCompetitivo>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_DeveCadastrarPerfil()
    {
        _jogadores
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(JogadorValido());

        _perfis
            .Setup(x => x.ExistsByJogadorAsync(1, null))
            .ReturnsAsync(false);

        _perfis
            .Setup(x => x.AddAsync(It.IsAny<PerfilCompetitivo>()))
            .Callback<PerfilCompetitivo>(p => p.Id = 10)
            .Returns(Task.CompletedTask);

        var result = await _service.CreateAsync(NovoPerfil());

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Data!.Id);
        Assert.Equal(1.32, result.Data.KDA);
        Assert.Equal("KSCERATO", result.Data.Jogador!.Nickname);

        _perfis.Verify(
            x => x.AddAsync(It.Is<PerfilCompetitivo>(
                p => p.JogadorId == 1)),
            Times.Once);
    }

    [Fact]
    public async Task Create_DeveTratarFalhaDoBanco()
    {
        _jogadores
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(JogadorValido());

        _perfis
            .Setup(x => x.AddAsync(It.IsAny<PerfilCompetitivo>()))
            .ThrowsAsync(DbFailure());

        var result = await _service.CreateAsync(NovoPerfil());

        Assert.Equal("DATABASE_ERROR", result.ErrorCode);
    }

    [Fact]
    public async Task Update_DeveRetornar404_QuandoPerfilNaoExistir()
    {
        _perfis
            .Setup(x => x.GetByIdForUpdateAsync(99))
            .ReturnsAsync((PerfilCompetitivo?)null);

        var result = await _service.UpdateAsync(
            99, NovoPerfil());

        Assert.Equal("PERFIL_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Update_DeveRetornar404_QuandoJogadorNaoExistir()
    {
        _perfis
            .Setup(x => x.GetByIdForUpdateAsync(10))
            .ReturnsAsync(PerfilValido());

        _jogadores
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync((Jogador?)null);

        var result = await _service.UpdateAsync(
            10, NovoPerfil());

        Assert.Equal("JOGADOR_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task Update_DeveRetornar409_QuandoOutroPerfilExistir()
    {
        _perfis
            .Setup(x => x.GetByIdForUpdateAsync(10))
            .ReturnsAsync(PerfilValido());

        _jogadores
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(JogadorValido());

        _perfis
            .Setup(x => x.ExistsByJogadorAsync(1, 10))
            .ReturnsAsync(true);

        var result = await _service.UpdateAsync(
            10, NovoPerfil());

        Assert.Equal(OperationStatus.Conflict, result.Status);

        _perfis.Verify(
            x => x.UpdateAsync(It.IsAny<PerfilCompetitivo>()),
            Times.Never);
    }

    [Fact]
    public async Task Update_DeveAtualizarPerfil()
    {
        var perfil = PerfilValido();
        var dto = NovoPerfil();
        dto.KDA = 2.5;

        _perfis
            .Setup(x => x.GetByIdForUpdateAsync(10))
            .ReturnsAsync(perfil);

        _jogadores
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(JogadorValido());

        _perfis
            .Setup(x => x.ExistsByJogadorAsync(1, 10))
            .ReturnsAsync(false);

        _perfis
            .Setup(x => x.UpdateAsync(perfil))
            .Returns(Task.CompletedTask);

        var result = await _service.UpdateAsync(10, dto);

        Assert.True(result.IsSuccess);
        Assert.Equal(2.5, result.Data!.KDA);

        _perfis.Verify(
            x => x.UpdateAsync(perfil),
            Times.Once);
    }

    [Fact]
    public async Task Update_DeveTratarFalhaDoBanco()
    {
        var perfil = PerfilValido();

        _perfis
            .Setup(x => x.GetByIdForUpdateAsync(10))
            .ReturnsAsync(perfil);

        _jogadores
            .Setup(x => x.GetByIdAsync(1))
            .ReturnsAsync(JogadorValido());

        _perfis
            .Setup(x => x.UpdateAsync(perfil))
            .ThrowsAsync(DbFailure());

        var result = await _service.UpdateAsync(
            10, NovoPerfil());

        Assert.Equal("DATABASE_ERROR", result.ErrorCode);
    }

    [Fact]
    public async Task Delete_DeveRetornar404_QuandoPerfilNaoExistir()
    {
        _perfis
            .Setup(x => x.GetByIdForUpdateAsync(99))
            .ReturnsAsync((PerfilCompetitivo?)null);

        var result = await _service.DeleteAsync(99);

        Assert.Equal(OperationStatus.NotFound, result.Status);

        _perfis.Verify(
            x => x.DeleteAsync(It.IsAny<PerfilCompetitivo>()),
            Times.Never);
    }

    [Fact]
    public async Task Delete_DeveRemoverPerfil()
    {
        var perfil = PerfilValido();

        _perfis
            .Setup(x => x.GetByIdForUpdateAsync(10))
            .ReturnsAsync(perfil);

        _perfis
            .Setup(x => x.DeleteAsync(perfil))
            .Returns(Task.CompletedTask);

        var result = await _service.DeleteAsync(10);

        Assert.True(result.IsSuccess);
        Assert.True(result.Data);

        _perfis.Verify(
            x => x.DeleteAsync(perfil),
            Times.Once);
    }

    [Fact]
    public async Task Delete_DeveTratarFalhaDoBanco()
    {
        var perfil = PerfilValido();

        _perfis
            .Setup(x => x.GetByIdForUpdateAsync(10))
            .ReturnsAsync(perfil);

        _perfis
            .Setup(x => x.DeleteAsync(perfil))
            .ThrowsAsync(DbFailure());

        var result = await _service.DeleteAsync(10);

        Assert.Equal("DATABASE_ERROR", result.ErrorCode);
    }

    [Fact]
    public async Task GetById_DeveRetornar404()
    {
        _perfis
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync((PerfilCompetitivo?)null);

        var result = await _service.GetByIdAsync(99);

        Assert.Equal("PERFIL_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task GetById_DeveRetornarPerfil()
    {
        _perfis
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(PerfilValido());

        var result = await _service.GetByIdAsync(10);

        Assert.True(result.IsSuccess);
        Assert.Equal(10, result.Data!.Id);
    }

    [Fact]
    public async Task GetByJogador_DeveRetornar404()
    {
        _perfis
            .Setup(x => x.GetByJogadorAsync(99))
            .ReturnsAsync((PerfilCompetitivo?)null);

        var result = await _service.GetByJogadorAsync(99);

        Assert.Equal("PERFIL_NOT_FOUND", result.ErrorCode);
    }

    [Fact]
    public async Task GetAll_DeveRetornarLista()
    {
        _perfis
            .Setup(x => x.GetAllAsync(1, 10))
            .ReturnsAsync(new PagedResult<PerfilCompetitivo>
            {
                Items = new[] { PerfilValido() },
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 1,
                TotalPages = 1
            });

        var result = await _service.GetAllAsync(1, 10);

        Assert.True(result.IsSuccess);
        Assert.Single(result.Data!.Items);
    }
}