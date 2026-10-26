using CP5.Application.DTOs;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Results;
using CP5.Application.Services;
using CP5.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CP5.UnitTests.Services;

public class PerfilCompetitivoServiceTests
{
    private readonly Mock<IPerfilCompetitivoRepository> _perfilRepositoryMock;
    private readonly Mock<IJogadorRepository> _jogadorRepositoryMock;
    private readonly PerfilCompetitivoService _service;

    public PerfilCompetitivoServiceTests()
    {
        _perfilRepositoryMock =
            new Mock<IPerfilCompetitivoRepository>();

        _jogadorRepositoryMock =
            new Mock<IJogadorRepository>();

        _service = new PerfilCompetitivoService(
            _perfilRepositoryMock.Object,
            _jogadorRepositoryMock.Object,
            NullLogger<PerfilCompetitivoService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_DeveRetornarJogadorNaoEncontrado_QuandoJogadorNaoExistir()
    {
        var dto = new PerfilCompetitivoCreateDto
        {
            KDA = 1.32,
            WinRate = 68,
            HorasJogadas = 12000,
            JogadorId = 999
        };

        _jogadorRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(dto.JogadorId))
            .ReturnsAsync((Jogador?)null);

        var resultado = await _service.CreateAsync(dto);

        Assert.Equal(
            PerfilCompetitivoResultStatus.JogadorNaoEncontrado,
            resultado.Status);

        Assert.Null(resultado.Perfil);

        _perfilRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<PerfilCompetitivo>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DeveRetornarPerfilJaExiste_QuandoJogadorJaPossuirPerfil()
    {
        var dto = new PerfilCompetitivoCreateDto
        {
            KDA = 1.32,
            WinRate = 68,
            HorasJogadas = 12000,
            JogadorId = 1
        };

        var jogador = CriarJogador();

        _jogadorRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(dto.JogadorId))
            .ReturnsAsync(jogador);

        _perfilRepositoryMock
            .Setup(repository =>
                repository.ExistsByJogadorAsync(
                    dto.JogadorId,
                    null))
            .ReturnsAsync(true);

        var resultado = await _service.CreateAsync(dto);

        Assert.Equal(
            PerfilCompetitivoResultStatus.PerfilJaExiste,
            resultado.Status);

        Assert.Null(resultado.Perfil);

        _perfilRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<PerfilCompetitivo>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DeveCriarPerfil_QuandoDadosForemValidos()
    {
        var dto = new PerfilCompetitivoCreateDto
        {
            KDA = 1.32,
            WinRate = 68,
            HorasJogadas = 12000,
            JogadorId = 1
        };

        var jogador = CriarJogador();

        _jogadorRepositoryMock
            .Setup(repository =>
                repository.GetByIdAsync(dto.JogadorId))
            .ReturnsAsync(jogador);

        _perfilRepositoryMock
            .Setup(repository =>
                repository.ExistsByJogadorAsync(
                    dto.JogadorId,
                    null))
            .ReturnsAsync(false);

        _perfilRepositoryMock
            .Setup(repository =>
                repository.AddAsync(
                    It.IsAny<PerfilCompetitivo>()))
            .Callback<PerfilCompetitivo>(perfil =>
                perfil.Id = 10)
            .Returns(Task.CompletedTask);

        var resultado = await _service.CreateAsync(dto);

        Assert.Equal(
            PerfilCompetitivoResultStatus.Sucesso,
            resultado.Status);

        Assert.NotNull(resultado.Perfil);

        Assert.Equal(10, resultado.Perfil.Id);
        Assert.Equal(dto.KDA, resultado.Perfil.KDA);
        Assert.Equal(dto.WinRate, resultado.Perfil.WinRate);
        Assert.Equal(
            dto.HorasJogadas,
            resultado.Perfil.HorasJogadas);

        Assert.Equal(
            dto.JogadorId,
            resultado.Perfil.JogadorId);

        _perfilRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<PerfilCompetitivo>(perfil =>
                    perfil.KDA == dto.KDA &&
                    perfil.WinRate == dto.WinRate &&
                    perfil.HorasJogadas == dto.HorasJogadas &&
                    perfil.JogadorId == dto.JogadorId)),
            Times.Once);
    }

    private static Jogador CriarJogador()
    {
        return new Jogador
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
    }
}