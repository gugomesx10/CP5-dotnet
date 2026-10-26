using CP5.Application.DTOs;
using CP5.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using CP5.Application.Results;
using Microsoft.AspNetCore.Authorization;

namespace CP5.Controllers;

[ApiController]
[Route("api/[controller]")]
public class TimeController : ControllerBase
{
    private readonly ITimeService _timeService;

    public TimeController(ITimeService timeService)
    {
        _timeService = timeService;
    }

    /// <summary>
    /// Retorna todos os times cadastrados.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista os times",
        Description = "Retorna os times cadastrados de forma paginada.",
        OperationId = "GetTimes")]
    [SwaggerResponse(200, "Times encontrados com sucesso")]
    [SwaggerResponse(204, "Nenhum time encontrado")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        pageSize = Math.Min(pageSize, 100);

        var result = await _timeService.GetAllAsync(
            pageNumber,
            pageSize);

        if (result.IsSuccess &&
            result.Data is not null &&
            !result.Data.Items.Any())
        {
            return NoContent();
        }

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Retorna um time pelo ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Busca um time por ID",
        Description = "Retorna os dados de um time específico.",
        OperationId = "GetTimeById")]
    [SwaggerResponse(200, "Time encontrado com sucesso")]
    [SwaggerResponse(404, "Time não encontrado")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _timeService.GetByIdAsync(id);

        return result.Status switch
        {
            OperationStatus.Success => Ok(result),

            OperationStatus.NotFound => NotFound(result),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                result)
        };
    }

    /// <summary>
    /// Retorna times filtrados pelo jogo.
    /// </summary>
    [HttpGet("jogo/{jogo}")]
    [SwaggerOperation(
        Summary = "Busca times por jogo",
        Description = "Filtra os times pelo jogo e retorna o resultado de forma paginada.",
        OperationId = "GetTimesByJogo")]
    [SwaggerResponse(200, "Times encontrados com sucesso")]
    [SwaggerResponse(204, "Nenhum time encontrado para o jogo")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    public async Task<IActionResult> GetByJogo(
        string jogo,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        pageSize = Math.Min(pageSize, 100);

        var result = await _timeService.GetByJogoAsync(
            jogo,
            pageNumber,
            pageSize);

        if (result.IsSuccess &&
            result.Data is not null &&
            !result.Data.Items.Any())
        {
            return NoContent();
        }

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Cadastra um novo time.
    /// </summary>
    [Authorize]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastra um time",
        Description = "Cria um novo time competitivo.",
        OperationId = "CreateTime")]
    [SwaggerResponse(201, "Time criado com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    public async Task<IActionResult> Create(TimeCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _timeService.CreateAsync(dto);

        if (result.Status == OperationStatus.Success &&
            result.Data is not null)
        {
            return CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.Id },
                result);
        }

        return StatusCode(
            StatusCodes.Status500InternalServerError,
            result);
    }

    /// <summary>
    /// Atualiza um time existente.
    /// </summary>
    [Authorize]
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualiza um time",
        Description = "Atualiza os dados de um time existente.",
        OperationId = "UpdateTime")]
    [SwaggerResponse(200, "Time atualizado com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(404, "Time não encontrado")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> Update(
        int id,
        TimeCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _timeService.UpdateAsync(id, dto);

        return result.Status switch
        {
            OperationStatus.Success => Ok(result),

            OperationStatus.NotFound => NotFound(result),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                result)
        };
    }

    /// <summary>
    /// Remove um time pelo ID.
    /// </summary>
    [Authorize]
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remove um time",
        Description = "Exclui um time pelo identificador.",
        OperationId = "DeleteTime")]
    [SwaggerResponse(204, "Time removido com sucesso")]
    [SwaggerResponse(404, "Time não encontrado")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _timeService.DeleteAsync(id);

        return result.Status switch
        {
            OperationStatus.Success => NoContent(),

            OperationStatus.NotFound => NotFound(result),

            _ => StatusCode(
                StatusCodes.Status500InternalServerError,
                result)
        };
    }
}