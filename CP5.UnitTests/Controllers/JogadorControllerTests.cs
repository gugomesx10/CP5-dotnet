using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Interfaces.Services;
using CP5.Application.Results;
using CP5.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CP5.UnitTests.Controllers;

public class JogadorControllerTests
{
    private readonly Mock<IJogadorService> _service = new();
    private readonly JogadorController _controller;

    public JogadorControllerTests()
    {
        _controller = new JogadorController(_service.Object);
    }

    private static JogadorCreateDto Dto() => new()
    {
        Nickname = "KSCERATO",
        Funcao = "Rifler",
        Idade = 25,
        TimeId = 1
    };

    private static JogadorResumoDto Data() => new()
    {
        Id = 10,
        Nickname = "KSCERATO",
        Funcao = "Rifler",
        Idade = 25
    };

    private static OperationResult<JogadorResumoDto> Ok() =>
        OperationResult<JogadorResumoDto>.Success(Data());

    private static OperationResult<JogadorResumoDto> Missing() =>
        OperationResult<JogadorResumoDto>.Failure(
            OperationStatus.NotFound,
            "JOGADOR_NOT_FOUND",
            "Não encontrado");

    private static OperationResult<JogadorResumoDto> DbFailure() =>
        OperationResult<JogadorResumoDto>.Failure(
            OperationStatus.UnexpectedError,
            "DATABASE_ERROR",
            "Erro");

    [Fact]
    public async Task GetById_DeveRetornar200()
    {
        _service
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(Ok());

        Assert.IsType<OkObjectResult>(
            await _controller.GetById(10));
    }

    [Fact]
    public async Task GetById_DeveRetornar404()
    {
        _service
            .Setup(x => x.GetByIdAsync(99))
            .ReturnsAsync(Missing());

        Assert.IsType<NotFoundObjectResult>(
            await _controller.GetById(99));
    }

    [Fact]
    public async Task GetById_DeveRetornar500()
    {
        _service
            .Setup(x => x.GetByIdAsync(10))
            .ReturnsAsync(DbFailure());

        var result = Assert.IsType<ObjectResult>(
            await _controller.GetById(10));

        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task Create_DeveRetornar201()
    {
        var dto = Dto();

        _service
            .Setup(x => x.CreateAsync(dto))
            .ReturnsAsync(Ok());

        var result = Assert.IsType<CreatedAtActionResult>(
            await _controller.Create(dto));

        Assert.Equal(201, result.StatusCode);
        Assert.Equal(
            nameof(JogadorController.GetById),
            result.ActionName);
    }

    [Fact]
    public async Task Create_DeveRetornar404_QuandoTimeNaoExistir()
    {
        var dto = Dto();

        _service
            .Setup(x => x.CreateAsync(dto))
            .ReturnsAsync(
                OperationResult<JogadorResumoDto>.Failure(
                    OperationStatus.NotFound,
                    "TIME_NOT_FOUND",
                    "Time inexistente"));

        Assert.IsType<NotFoundObjectResult>(
            await _controller.Create(dto));
    }

    [Fact]
    public async Task Create_DeveRetornar400_QuandoModelStateInvalido()
    {
        _controller.ModelState.AddModelError(
            "Nickname", "Obrigatório");

        Assert.IsType<BadRequestObjectResult>(
            await _controller.Create(Dto()));

        _service.Verify(
            x => x.CreateAsync(It.IsAny<JogadorCreateDto>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_DeveRetornar500()
    {
        var dto = Dto();

        _service
            .Setup(x => x.CreateAsync(dto))
            .ReturnsAsync(DbFailure());

        var result = Assert.IsType<ObjectResult>(
            await _controller.Create(dto));

        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task Update_DeveRetornar200()
    {
        var dto = Dto();

        _service
            .Setup(x => x.UpdateAsync(10, dto))
            .ReturnsAsync(Ok());

        Assert.IsType<OkObjectResult>(
            await _controller.Update(10, dto));
    }

    [Fact]
    public async Task Update_DeveRetornar404()
    {
        var dto = Dto();

        _service
            .Setup(x => x.UpdateAsync(99, dto))
            .ReturnsAsync(Missing());

        Assert.IsType<NotFoundObjectResult>(
            await _controller.Update(99, dto));
    }

    [Fact]
    public async Task Update_DeveRetornar500()
    {
        var dto = Dto();

        _service
            .Setup(x => x.UpdateAsync(10, dto))
            .ReturnsAsync(DbFailure());

        var result = Assert.IsType<ObjectResult>(
            await _controller.Update(10, dto));

        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task Delete_DeveRetornar204()
    {
        _service
            .Setup(x => x.DeleteAsync(10))
            .ReturnsAsync(OperationResult<bool>.Success(true));

        Assert.IsType<NoContentResult>(
            await _controller.Delete(10));
    }

    [Fact]
    public async Task Delete_DeveRetornar404()
    {
        _service
            .Setup(x => x.DeleteAsync(99))
            .ReturnsAsync(
                OperationResult<bool>.Failure(
                    OperationStatus.NotFound,
                    "JOGADOR_NOT_FOUND",
                    "Não encontrado"));

        Assert.IsType<NotFoundObjectResult>(
            await _controller.Delete(99));
    }

    [Fact]
    public async Task Delete_DeveRetornar500()
    {
        _service
            .Setup(x => x.DeleteAsync(10))
            .ReturnsAsync(
                OperationResult<bool>.Failure(
                    OperationStatus.UnexpectedError,
                    "DATABASE_ERROR",
                    "Erro"));

        var result = Assert.IsType<ObjectResult>(
            await _controller.Delete(10));

        Assert.Equal(500, result.StatusCode);
    }

    [Fact]
    public async Task GetAll_DeveRetornar204_QuandoListaVazia()
    {
        _service
            .Setup(x => x.GetAllAsync(1, 10))
            .ReturnsAsync(
                OperationResult<PagedResult<JogadorResumoDto>>.Success(
                    new PagedResult<JogadorResumoDto>
                    {
                        Items = Array.Empty<JogadorResumoDto>(),
                        PageNumber = 1,
                        PageSize = 10,
                        TotalItems = 0,
                        TotalPages = 0
                    }));

        Assert.IsType<NoContentResult>(
            await _controller.GetAll());
    }

    [Fact]
    public async Task GetAll_DeveRetornar400_QuandoPaginaInvalida()
    {
        Assert.IsType<BadRequestObjectResult>(
            await _controller.GetAll(0, 10));

        _service.Verify(
            x => x.GetAllAsync(It.IsAny<int>(), It.IsAny<int>()),
            Times.Never);
    }
}