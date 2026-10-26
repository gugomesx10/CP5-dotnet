using CP5.Application.DTOs;
using CP5.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

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

        var times = await _timeService.GetAllAsync(
            pageNumber,
            pageSize);

        if (!times.Items.Any())
            return NoContent();

        return Ok(times);
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
        var time = await _timeService.GetByIdAsync(id);

        if (time == null)
            return NotFound();

        return Ok(time);
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

        var times = await _timeService.GetByJogoAsync(
            jogo,
            pageNumber,
            pageSize);

        if (!times.Items.Any())
            return NoContent();

        return Ok(times);
    }

    /// <summary>
    /// Cadastra um novo time.
    /// </summary>
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

        var time = await _timeService.CreateAsync(dto);

        return CreatedAtAction(
            nameof(GetById),
            new { id = time.Id },
            time);
    }

    /// <summary>
    /// Atualiza um time existente.
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualiza um time",
        Description = "Atualiza os dados de um time existente.",
        OperationId = "UpdateTime")]
    [SwaggerResponse(200, "Time atualizado com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(404, "Time não encontrado")]
    public async Task<IActionResult> Update(
        int id,
        TimeCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var time = await _timeService.UpdateAsync(id, dto);

        if (time == null)
            return NotFound();

        return Ok(time);
    }

    /// <summary>
    /// Remove um time pelo ID.
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remove um time",
        Description = "Exclui um time pelo identificador.",
        OperationId = "DeleteTime")]
    [SwaggerResponse(204, "Time removido com sucesso")]
    [SwaggerResponse(404, "Time não encontrado")]
    public async Task<IActionResult> Delete(int id)
    {
        var removido = await _timeService.DeleteAsync(id);

        if (!removido)
            return NotFound();

        return NoContent();
    }
}