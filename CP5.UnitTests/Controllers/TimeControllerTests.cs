using CP5.Application.DTOs.Responses;
using CP5.Application.Interfaces.Services;
using CP5.Application.Results;
using CP5.Application.DTOs;
using CP5.Controllers;
using Microsoft.AspNetCore.Mvc;
using Moq;

namespace CP5.UnitTests.Controllers;

public class TimeControllerTests
{
    private readonly Mock<ITimeService> _serviceMock;
    private readonly TimeController _controller;

    public TimeControllerTests()
    {
        _serviceMock = new Mock<ITimeService>();
        _controller = new TimeController(_serviceMock.Object);
    }
    
    [Fact]
    public async Task GetAll_DeveRetornar204QuandoListaVazia()
    {
        var resultado = OperationResult<
            CP5.Application.Common.PagedResult<TimeResumoDto>>.Success(
            new CP5.Application.Common.PagedResult<TimeResumoDto>
            {
                Items = Array.Empty<TimeResumoDto>(),
                PageNumber = 1,
                PageSize = 10,
                TotalItems = 0,
                TotalPages = 0
            });

        _serviceMock
            .Setup(s => s.GetAllAsync(1, 10))
            .ReturnsAsync(resultado);

        var response = await _controller.GetAll();

        Assert.IsType<NoContentResult>(response);
    }

    [Fact]
    public async Task GetByJogo_DeveRetornar500QuandoBancoFalhar()
    {
        var resultado = OperationResult<
            CP5.Application.Common.PagedResult<TimeResumoDto>>.Failure(
            OperationStatus.UnexpectedError,
            "DATABASE_ERROR",
            "Não foi possível consultar os times.");

        _serviceMock
            .Setup(s => s.GetByJogoAsync("CS2", 1, 10))
            .ReturnsAsync(resultado);

        var response = await _controller.GetByJogo("CS2");

        var erro = Assert.IsType<ObjectResult>(response);

        Assert.Equal(500, erro.StatusCode);
    }

    [Fact]
    public async Task GetById_DeveRetornar200_QuandoTimeExistir()
    {
        // Arrange
        var time = new TimeResumoDto
        {
            Id = 1,
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        var resultado = OperationResult<TimeResumoDto>.Success(
            time,
            "Time encontrado com sucesso.");

        _serviceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.GetById(1);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(response);
        Assert.Equal(200, ok.StatusCode);
        Assert.Same(resultado, ok.Value);

        _serviceMock.Verify(
            s => s.GetByIdAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task GetById_DeveRetornar404_QuandoTimeNaoExistir()
    {
        // Arrange
        var resultado = OperationResult<TimeResumoDto>.Failure(
            OperationStatus.NotFound,
            "TIME_NOT_FOUND",
            "O time informado não foi encontrado.");

        _serviceMock
            .Setup(s => s.GetByIdAsync(999))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.GetById(999);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(response);

        Assert.Equal(404, notFound.StatusCode);
        Assert.Same(resultado, notFound.Value);
        Assert.Equal("TIME_NOT_FOUND", resultado.ErrorCode);
    }

    [Fact]
    public async Task GetById_DeveRetornar500_QuandoOcorrerErroNoBanco()
    {
        // Arrange
        var resultado = OperationResult<TimeResumoDto>.Failure(
            OperationStatus.UnexpectedError,
            "DATABASE_ERROR",
            "Não foi possível consultar o time.");

        _serviceMock
            .Setup(s => s.GetByIdAsync(1))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.GetById(1);

        // Assert
        var erro = Assert.IsType<ObjectResult>(response);

        Assert.Equal(500, erro.StatusCode);
        Assert.Same(resultado, erro.Value);
        Assert.Equal("DATABASE_ERROR", resultado.ErrorCode);
    }
    
    [Fact]
    public async Task Create_DeveRetornar201_QuandoTimeForCriado()
    {
        // Arrange
        var dto = new TimeCreateDto
        {
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        var time = new TimeResumoDto
        {
            Id = 10,
            Nome = dto.Nome,
            Jogo = dto.Jogo,
            Pais = dto.Pais,
            Ranking = dto.Ranking
        };

        var resultado = OperationResult<TimeResumoDto>.Success(
            time,
            "Time cadastrado com sucesso.");

        _serviceMock
            .Setup(s => s.CreateAsync(dto))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.Create(dto);

        // Assert
        var created = Assert.IsType<CreatedAtActionResult>(response);

        Assert.Equal(201, created.StatusCode);
        Assert.Equal(nameof(TimeController.GetById), created.ActionName);
        Assert.Equal(10, Assert.IsType<int>(created.RouteValues!["id"]));
        Assert.Same(resultado, created.Value);

        _serviceMock.Verify(
            s => s.CreateAsync(dto),
            Times.Once);
    }

    [Fact]
    public async Task Create_DeveRetornar400_QuandoDadosForemInvalidos()
    {
        // Arrange
        var dto = new TimeCreateDto();

        _controller.ModelState.AddModelError(
            "Nome",
            "O nome é obrigatório.");

        // Act
        var response = await _controller.Create(dto);

        // Assert
        var badRequest = Assert.IsType<BadRequestObjectResult>(response);

        Assert.Equal(400, badRequest.StatusCode);

        _serviceMock.Verify(
            s => s.CreateAsync(It.IsAny<TimeCreateDto>()),
            Times.Never);
    }

    [Fact]
    public async Task Create_DeveRetornar500_QuandoBancoFalhar()
    {
        // Arrange
        var dto = new TimeCreateDto
        {
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        var resultado = OperationResult<TimeResumoDto>.Failure(
            OperationStatus.UnexpectedError,
            "DATABASE_ERROR",
            "Não foi possível cadastrar o time.");

        _serviceMock
            .Setup(s => s.CreateAsync(dto))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.Create(dto);

        // Assert
        var erro = Assert.IsType<ObjectResult>(response);

        Assert.Equal(500, erro.StatusCode);
        Assert.Same(resultado, erro.Value);
        Assert.Equal("DATABASE_ERROR", resultado.ErrorCode);
    }
    
    [Fact]
    public async Task Update_DeveRetornar200_QuandoTimeForAtualizado()
    {
        // Arrange
        var dto = new TimeCreateDto
        {
            Nome = "FURIA Atualizada",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 1
        };

        var time = new TimeResumoDto
        {
            Id = 1,
            Nome = dto.Nome,
            Jogo = dto.Jogo,
            Pais = dto.Pais,
            Ranking = dto.Ranking
        };

        var resultado = OperationResult<TimeResumoDto>.Success(
            time,
            "Time atualizado com sucesso.");

        _serviceMock
            .Setup(s => s.UpdateAsync(1, dto))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.Update(1, dto);

        // Assert
        var ok = Assert.IsType<OkObjectResult>(response);

        Assert.Equal(200, ok.StatusCode);
        Assert.Same(resultado, ok.Value);
        Assert.Equal("FURIA Atualizada", resultado.Data!.Nome);
    }

    [Fact]
    public async Task Update_DeveRetornar404_QuandoTimeNaoExistir()
    {
        // Arrange
        var dto = new TimeCreateDto
        {
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        var resultado = OperationResult<TimeResumoDto>.Failure(
            OperationStatus.NotFound,
            "TIME_NOT_FOUND",
            "O time informado não foi encontrado.");

        _serviceMock
            .Setup(s => s.UpdateAsync(999, dto))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.Update(999, dto);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(response);

        Assert.Equal(404, notFound.StatusCode);
        Assert.Same(resultado, notFound.Value);
        Assert.Equal("TIME_NOT_FOUND", resultado.ErrorCode);
    }

    [Fact]
    public async Task Update_DeveRetornar500_QuandoPersistenciaFalhar()
    {
        // Arrange
        var dto = new TimeCreateDto
        {
            Nome = "FURIA",
            Jogo = "CS2",
            Pais = "Brasil",
            Ranking = 3
        };

        var resultado = OperationResult<TimeResumoDto>.Failure(
            OperationStatus.UnexpectedError,
            "DATABASE_ERROR",
            "Não foi possível atualizar o time.");

        _serviceMock
            .Setup(s => s.UpdateAsync(1, dto))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.Update(1, dto);

        // Assert
        var erro = Assert.IsType<ObjectResult>(response);

        Assert.Equal(500, erro.StatusCode);
        Assert.Same(resultado, erro.Value);
    }
    
    [Fact]
    public async Task Delete_DeveRetornar204_QuandoTimeForExcluido()
    {
        // Arrange
        var resultado = OperationResult<bool>.Success(
            true,
            "Time excluído com sucesso.");

        _serviceMock
            .Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.Delete(1);

        // Assert
        var noContent = Assert.IsType<NoContentResult>(response);

        Assert.Equal(204, noContent.StatusCode);

        _serviceMock.Verify(
            s => s.DeleteAsync(1),
            Times.Once);
    }

    [Fact]
    public async Task Delete_DeveRetornar404_QuandoTimeNaoExistir()
    {
        // Arrange
        var resultado = OperationResult<bool>.Failure(
            OperationStatus.NotFound,
            "TIME_NOT_FOUND",
            "O time informado não foi encontrado.");

        _serviceMock
            .Setup(s => s.DeleteAsync(999))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.Delete(999);

        // Assert
        var notFound = Assert.IsType<NotFoundObjectResult>(response);

        Assert.Equal(404, notFound.StatusCode);
        Assert.Same(resultado, notFound.Value);
    }

    [Fact]
    public async Task Delete_DeveRetornar500_QuandoBancoFalhar()
    {
        // Arrange
        var resultado = OperationResult<bool>.Failure(
            OperationStatus.UnexpectedError,
            "DATABASE_ERROR",
            "Não foi possível excluir o time.");

        _serviceMock
            .Setup(s => s.DeleteAsync(1))
            .ReturnsAsync(resultado);

        // Act
        var response = await _controller.Delete(1);

        // Assert
        var erro = Assert.IsType<ObjectResult>(response);

        Assert.Equal(500, erro.StatusCode);
        Assert.Same(resultado, erro.Value);
        Assert.Equal("DATABASE_ERROR", resultado.ErrorCode);
    }
}