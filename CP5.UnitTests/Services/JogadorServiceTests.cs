using CP5.Application.DTOs;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Services;
using CP5.Domain.Entities;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;

namespace CP5.UnitTests.Services;

public class JogadorServiceTests
{
    private readonly Mock<IJogadorRepository> _jogadorRepositoryMock;
    private readonly Mock<ITimeRepository> _timeRepositoryMock;
    private readonly JogadorService _service;

    public JogadorServiceTests()
    {
        _jogadorRepositoryMock = new Mock<IJogadorRepository>();
        _timeRepositoryMock = new Mock<ITimeRepository>();

        _service = new JogadorService(
            _jogadorRepositoryMock.Object,
            _timeRepositoryMock.Object,
            NullLogger<JogadorService>.Instance);
    }

    [Fact]
    public async Task CreateAsync_DeveRetornarNull_QuandoTimeNaoExistir()
    {
        var dto = new JogadorCreateDto
        {
            Nickname = "KSCERATO",
            Funcao = "Rifler",
            Idade = 25,
            TimeId = 999
        };

        _timeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(dto.TimeId))
            .ReturnsAsync((Time?)null);

        var resultado = await _service.CreateAsync(dto);

        Assert.Null(resultado);

        _jogadorRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.IsAny<Jogador>()),
            Times.Never);
    }

    [Fact]
    public async Task CreateAsync_DeveCriarJogador_QuandoTimeExistir()
    {
        var dto = new JogadorCreateDto
        {
            Nickname = "KSCERATO",
            Funcao = "Rifler",
            Idade = 25,
            TimeId = 1
        };

        var time = new Time
        {
            Id = 1,
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        _timeRepositoryMock
            .Setup(repository => repository.GetByIdAsync(dto.TimeId))
            .ReturnsAsync(time);

        _jogadorRepositoryMock
            .Setup(repository => repository.AddAsync(
                It.IsAny<Jogador>()))
            .Returns(Task.CompletedTask);

        var resultado = await _service.CreateAsync(dto);

        Assert.NotNull(resultado);
        Assert.Equal("KSCERATO", resultado.Nickname);
        Assert.Equal("Rifler", resultado.Funcao);
        Assert.Equal(25, resultado.Idade);

        Assert.NotNull(resultado.Time);
        Assert.Equal(1, resultado.Time.Id);
        Assert.Equal("FURIA", resultado.Time.Nome);

        _jogadorRepositoryMock.Verify(
            repository => repository.AddAsync(
                It.Is<Jogador>(jogador =>
                    jogador.Nickname == dto.Nickname &&
                    jogador.Funcao == dto.Funcao &&
                    jogador.Idade == dto.Idade &&
                    jogador.TimeId == dto.TimeId)),
            Times.Once);
    }

    [Fact]
    public async Task DeleteAsync_DeveRetornarFalse_QuandoJogadorNaoExistir()
    {
        const int jogadorId = 999;

        _jogadorRepositoryMock
            .Setup(repository =>
                repository.GetByIdForUpdateAsync(jogadorId))
            .ReturnsAsync((Jogador?)null);

        var resultado = await _service.DeleteAsync(jogadorId);

        Assert.False(resultado);

        _jogadorRepositoryMock.Verify(
            repository => repository.DeleteAsync(
                It.IsAny<Jogador>()),
            Times.Never);
    }
}