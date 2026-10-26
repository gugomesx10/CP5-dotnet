using CP5.Application.DTOs;
using CP5.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace CP5.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento de jogadores.
/// </summary>
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
    /// Retorna todos os jogadores cadastrados com seus respectivos times.
    /// </summary>
    /// <param name="pageNumber">Número da página.</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <returns>Lista paginada de jogadores.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista os jogadores",
        Description = "Retorna os jogadores cadastrados de forma paginada.",
        OperationId = "GetJogadores")]
    [SwaggerResponse(200, "Jogadores encontrados com sucesso")]
    [SwaggerResponse(204, "Nenhum jogador encontrado")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        var result = await _jogadorService.GetAllAsync(
            pageNumber,
            Math.Min(pageSize, 100));

        if (result.IsSuccess &&
            result.Data is not null &&
            !result.Data.Items.Any())
        {
            return NoContent();
        }

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Retorna um jogador pelo seu identificador.
    /// </summary>
    /// <param name="id">Identificador do jogador.</param>
    /// <returns>Dados do jogador e informações resumidas do time.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Busca um jogador por ID",
        Description = "Retorna os dados de um jogador específico e seu time.",
        OperationId = "GetJogadorById")]
    [SwaggerResponse(200, "Jogador encontrado com sucesso")]
    [SwaggerResponse(404, "Jogador não encontrado")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _jogadorService.GetByIdAsync(id);

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Retorna os jogadores vinculados a um time específico.
    /// </summary>
    /// <param name="timeId">Identificador do time.</param>
    /// <param name="pageNumber">Número da página.</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <returns>Lista paginada de jogadores do time.</returns>
    [HttpGet("time/{timeId}")]
    [SwaggerOperation(
        Summary = "Busca jogadores por time",
        Description = "Retorna os jogadores vinculados ao time informado.",
        OperationId = "GetJogadoresByTime")]
    [SwaggerResponse(200, "Jogadores encontrados com sucesso")]
    [SwaggerResponse(204, "Nenhum jogador encontrado para o time")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> GetByTime(
        int timeId,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        var result = await _jogadorService.GetByTimeAsync(
            timeId,
            pageNumber,
            Math.Min(pageSize, 100));

        if (result.IsSuccess &&
            result.Data is not null &&
            !result.Data.Items.Any())
        {
            return NoContent();
        }

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Retorna os jogadores filtrados pela função desempenhada.
    /// </summary>
    /// <param name="funcao">Função do jogador.</param>
    /// <param name="pageNumber">Número da página.</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <returns>Lista paginada de jogadores filtrados pela função.</returns>
    [HttpGet("funcao/{funcao}")]
    [SwaggerOperation(
        Summary = "Busca jogadores por função",
        Description = "Filtra os jogadores pela função e retorna os resultados paginados.",
        OperationId = "GetJogadoresByFuncao")]
    [SwaggerResponse(200, "Jogadores encontrados com sucesso")]
    [SwaggerResponse(204, "Nenhum jogador encontrado para a função")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> GetByFuncao(
        string funcao,
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        var result = await _jogadorService.GetByFuncaoAsync(
            funcao,
            pageNumber,
            Math.Min(pageSize, 100));

        if (result.IsSuccess &&
            result.Data is not null &&
            !result.Data.Items.Any())
        {
            return NoContent();
        }

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Cadastra um novo jogador e o associa a um time existente.
    /// </summary>
    /// <param name="dto">Dados necessários para cadastrar o jogador.</param>
    /// <returns>Jogador cadastrado e sua localização na API.</returns>
    [Authorize]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastra um jogador",
        Description = "Cria um jogador e o associa a um time existente.",
        OperationId = "CreateJogador")]
    [SwaggerResponse(201, "Jogador criado com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(404, "Time não encontrado")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> Create(JogadorCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _jogadorService.CreateAsync(dto);

        return result.IsSuccess && result.Data is not null
            ? CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.Id },
                result)
            : this.ToHttpResult(result);
    }

    /// <summary>
    /// Atualiza os dados de um jogador existente.
    /// </summary>
    /// <param name="id">Identificador do jogador.</param>
    /// <param name="dto">Novos dados do jogador.</param>
    /// <returns>Resultado da atualização do jogador.</returns>
    [Authorize]
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualiza um jogador",
        Description = "Atualiza os dados do jogador e sua associação com o time.",
        OperationId = "UpdateJogador")]
    [SwaggerResponse(200, "Jogador atualizado com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(404, "Jogador ou time não encontrado")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> Update(
        int id,
        JogadorCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _jogadorService.UpdateAsync(id, dto);

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Remove um jogador pelo seu identificador.
    /// </summary>
    /// <param name="id">Identificador do jogador a ser excluído.</param>
    /// <returns>Resultado da operação de exclusão.</returns>
    [Authorize]
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remove um jogador",
        Description = "Exclui o jogador identificado pelo ID.",
        OperationId = "DeleteJogador")]
    [SwaggerResponse(204, "Jogador removido com sucesso")]
    [SwaggerResponse(404, "Jogador não encontrado")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _jogadorService.DeleteAsync(id);

        return result.IsSuccess
            ? NoContent()
            : this.ToHttpResult(result);
    }
}