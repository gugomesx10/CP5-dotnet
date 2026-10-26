using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Interfaces.Services;
using CP5.Application.Results;
using CP5.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CP5.UnitTests.Controllers;

public class PerfilCompetitivoControllerTests
{
    private readonly Mock<IPerfilCompetitivoService> _service = new();
    private readonly PerfilCompetitivoController _controller;

    public PerfilCompetitivoControllerTests()
    {
        _controller = new PerfilCompetitivoController(_service.Object);
    }

    private static PerfilCompetitivoCreateDto Dto() => new()
    {
        JogadorId = 1,
        KDA = 1.32,
        WinRate = 68,
        HorasJogadas = 12000
    };

    private static PerfilCompetitivoResponseDto Data() => new()
    {
        Id = 10,
        JogadorId = 1,
        KDA = 1.32,
        WinRate = 68,
        HorasJogadas = 12000
    };

    private static OperationResult<PerfilCompetitivoResponseDto> Ok() =>
        OperationResult<PerfilCompetitivoResponseDto>.Success(Data());

    private static OperationResult<PerfilCompetitivoResponseDto> Missing() =>
        OperationResult<PerfilCompetitivoResponseDto>.Failure(
            OperationStatus.NotFound,
            "PERFIL_NOT_FOUND",
            "Não encontrado");

    private static OperationResult<PerfilCompetitivoResponseDto> Conflict() =>
        OperationResult<PerfilCompetitivoResponseDto>.Failure(
            OperationStatus.Conflict,
            "PERFIL_ALREADY_EXISTS",
            "Duplicado");

    private static OperationResult<PerfilCompetitivoResponseDto> DbFailure() =>
        OperationResult<PerfilCompetitivoResponseDto>.Failure(
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
    public async Task GetByJogador_DeveRetornar200()
    {
        _service
            .Setup(x => x.GetByJogadorAsync(1))
            .ReturnsAsync(Ok());

        Assert.IsType<OkObjectResult>(
            await _controller.GetByJogador(1));
    }

    [Fact]
    public async Task GetByJogador_DeveRetornar404()
    {
        _service
            .Setup(x => x.GetByJogadorAsync(99))
            .ReturnsAsync(Missing());

        Assert.IsType<NotFoundObjectResult>(
            await _controller.GetByJogador(99));
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
            nameof(PerfilCompetitivoController.GetById),
            result.ActionName);
    }

    [Fact]
    public async Task Create_DeveRetornar404_QuandoJogadorNaoExistir()
    {
        var dto = Dto();

        _service
            .Setup(x => x.CreateAsync(dto))
            .ReturnsAsync(Missing());

        Assert.IsType<NotFoundObjectResult>(
            await _controller.Create(dto));
    }

    [Fact]
    public async Task Create_DeveRetornar409_QuandoPerfilDuplicado()
    {
        var dto = Dto();

        _service
            .Setup(x => x.CreateAsync(dto))
            .ReturnsAsync(Conflict());

        Assert.IsType<ConflictObjectResult>(
            await _controller.Create(dto));
    }

    [Fact]
    public async Task Create_DeveRetornar400_QuandoModelStateInvalido()
    {
        _controller.ModelState.AddModelError(
            "JogadorId", "Obrigatório");

        Assert.IsType<BadRequestObjectResult>(
            await _controller.Create(Dto()));

        _service.Verify(
            x => x.CreateAsync(It.IsAny<PerfilCompetitivoCreateDto>()),
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
    public async Task Update_DeveRetornar409()
    {
        var dto = Dto();

        _service
            .Setup(x => x.UpdateAsync(10, dto))
            .ReturnsAsync(Conflict());

        Assert.IsType<ConflictObjectResult>(
            await _controller.Update(10, dto));
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
                    "PERFIL_NOT_FOUND",
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
    public async Task GetAll_DeveRetornar204_QuandoVazio()
    {
        _service
            .Setup(x => x.GetAllAsync(1, 10))
            .ReturnsAsync(
                OperationResult<PagedResult<PerfilCompetitivoResponseDto>>.Success(
                    new PagedResult<PerfilCompetitivoResponseDto>
                    {
                        Items = Array.Empty<PerfilCompetitivoResponseDto>(),
                        PageNumber = 1,
                        PageSize = 10,
                        TotalItems = 0,
                        TotalPages = 0
                    }));

        Assert.IsType<NoContentResult>(
            await _controller.GetAll());
    }
}