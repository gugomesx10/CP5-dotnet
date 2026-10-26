using CP5.Application.DTOs;
using CP5.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CP5.Controllers;

[ApiController]
[Route("api/[controller]")]
public class JogadorController : ControllerBase
{
    private readonly IJogadorService _jogadorService;

    public JogadorController(IJogadorService jogadorService)
    {
        _jogadorService = jogadorService;
    }

    /// <summary>
    /// Retorna todos os jogadores cadastrados com seus times.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista os jogadores",
        Description = "Retorna os jogadores cadastrados de forma paginada.",
        OperationId = "GetJogadores")]
    [SwaggerResponse(200, "Jogadores encontrados com sucesso")]
    [SwaggerResponse(204, "Nenhum jogador encontrado")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        pageSize = Math.Min(pageSize, 100);

        var jogadores = await _jogadorService
            .GetAllAsync(pageNumber, pageSize);

        if (!jogadores.Items.Any())
            return NoContent();

        return Ok(jogadores);
    }

    /// <summary>
    /// Retorna um jogador pelo ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Busca um jogador por ID",
        Description = "Retorna o jogador e as informações resumidas do seu time.",
        OperationId = "GetJogadorById")]
    [SwaggerResponse(200, "Jogador encontrado com sucesso")]
    [SwaggerResponse(404, "Jogador não encontrado")]
    public async Task<IActionResult> GetById(int id)
    {
        var jogador = await _jogadorService.GetByIdAsync(id);

        if (jogador == null)
            return NotFound();

        return Ok(jogador);
    }

    /// <summary>
    /// Retorna jogadores filtrados pelo ID do time.
    /// </summary>
    [HttpGet("time/{timeId}")]
    [SwaggerOperation(
        Summary = "Busca jogadores por time",
        Description = "Retorna de forma paginada os jogadores vinculados ao time informado.",
        OperationId = "GetJogadoresByTime")]
    [SwaggerResponse(200, "Jogadores encontrados com sucesso")]
    [SwaggerResponse(204, "Nenhum jogador encontrado para este time")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    public async Task<IActionResult> GetByTime(
        int timeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        pageSize = Math.Min(pageSize, 100);

        var jogadores = await _jogadorService
            .GetByTimeAsync(timeId, pageNumber, pageSize);

        if (!jogadores.Items.Any())
            return NoContent();

        return Ok(jogadores);
    }

    /// <summary>
    /// Retorna jogadores filtrados pela função.
    /// </summary>
    [HttpGet("funcao/{funcao}")]
    [SwaggerOperation(
        Summary = "Busca jogadores por função",
        Description = "Filtra os jogadores pela função e retorna o resultado de forma paginada.",
        OperationId = "GetJogadoresByFuncao")]
    [SwaggerResponse(200, "Jogadores encontrados com sucesso")]
    [SwaggerResponse(204, "Nenhum jogador encontrado para esta função")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    public async Task<IActionResult> GetByFuncao(
        string funcao,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        pageSize = Math.Min(pageSize, 100);

        var jogadores = await _jogadorService
            .GetByFuncaoAsync(funcao, pageNumber, pageSize);

        if (!jogadores.Items.Any())
            return NoContent();

        return Ok(jogadores);
    }

    /// <summary>
    /// Cadastra um novo jogador vinculado a um time.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastra um jogador",
        Description = "Cria um jogador e o associa a um time existente.",
        OperationId = "CreateJogador")]
    [SwaggerResponse(201, "Jogador criado com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(404, "Time não encontrado")]
    public async Task<IActionResult> Create(JogadorCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var jogador = await _jogadorService.CreateAsync(dto);

        if (jogador == null)
            return NotFound("Time não encontrado.");

        return CreatedAtAction(
            nameof(GetById),
            new { id = jogador.Id },
            jogador);
    }

    /// <summary>
    /// Atualiza um jogador existente.
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualiza um jogador",
        Description = "Atualiza os dados do jogador e sua associação com o time.",
        OperationId = "UpdateJogador")]
    [SwaggerResponse(200, "Jogador atualizado com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(404, "Jogador ou time não encontrado")]
    public async Task<IActionResult> Update(
        int id,
        JogadorCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var jogador = await _jogadorService.UpdateAsync(id, dto);

        if (jogador == null)
            return NotFound("Jogador ou time não encontrado.");

        return Ok(jogador);
    }

    /// <summary>
    /// Remove um jogador pelo ID.
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remove um jogador",
        Description = "Exclui o jogador identificado pelo ID.",
        OperationId = "DeleteJogador")]
    [SwaggerResponse(204, "Jogador removido com sucesso")]
    [SwaggerResponse(404, "Jogador não encontrado")]
    public async Task<IActionResult> Delete(int id)
    {
        var removido = await _jogadorService.DeleteAsync(id);

        if (!removido)
            return NotFound();

        return NoContent();
    }
}