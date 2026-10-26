using CP5.Application.DTOs;
using CP5.Application.Interfaces.Services;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;
using Microsoft.AspNetCore.Authorization;

namespace CP5.Controllers;

/// <summary>
/// Controller responsável pelo gerenciamento dos perfis competitivos.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public class PerfilCompetitivoController : ControllerBase
{
    private readonly IPerfilCompetitivoService _perfilService;

    public PerfilCompetitivoController(
        IPerfilCompetitivoService perfilService)
    {
        _perfilService = perfilService;
    }

    /// <summary>
    /// Retorna todos os perfis competitivos cadastrados.
    /// </summary>
    /// <param name="pageNumber">Número da página.</param>
    /// <param name="pageSize">Quantidade de registros por página.</param>
    /// <returns>Lista paginada dos perfis competitivos.</returns>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista os perfis competitivos",
        Description = "Retorna os perfis competitivos de forma paginada.",
        OperationId = "GetPerfisCompetitivos")]
    [SwaggerResponse(200, "Perfis competitivos encontrados")]
    [SwaggerResponse(204, "Nenhum perfil competitivo encontrado")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        var result = await _perfilService.GetAllAsync(
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
    /// Retorna um perfil competitivo pelo seu identificador.
    /// </summary>
    /// <param name="id">Identificador do perfil competitivo.</param>
    /// <returns>Dados do perfil competitivo encontrado.</returns>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Busca um perfil competitivo por ID",
        Description = "Retorna os dados do perfil competitivo correspondente ao ID informado.",
        OperationId = "GetPerfilCompetitivoById")]
    [SwaggerResponse(200, "Perfil competitivo encontrado")]
    [SwaggerResponse(404, "Perfil competitivo não encontrado")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> GetById(int id)
    {
        var result = await _perfilService.GetByIdAsync(id);

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Retorna o perfil competitivo associado a um jogador.
    /// </summary>
    /// <param name="jogadorId">Identificador do jogador.</param>
    /// <returns>Perfil competitivo do jogador informado.</returns>
    [HttpGet("jogador/{jogadorId}")]
    [SwaggerOperation(
        Summary = "Busca o perfil competitivo de um jogador",
        Description = "Retorna o perfil competitivo associado ao jogador informado.",
        OperationId = "GetPerfilByJogador")]
    [SwaggerResponse(200, "Perfil competitivo encontrado")]
    [SwaggerResponse(404, "Perfil competitivo não encontrado")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> GetByJogador(int jogadorId)
    {
        var result = await _perfilService.GetByJogadorAsync(jogadorId);

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Cadastra um perfil competitivo para um jogador existente.
    /// </summary>
    /// <remarks>
    /// Cada jogador pode possuir apenas um perfil competitivo.
    /// </remarks>
    /// <param name="dto">Dados estatísticos do perfil competitivo.</param>
    /// <returns>Perfil competitivo criado.</returns>
    [Authorize]
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastra um perfil competitivo",
        Description = "Cria um perfil para um jogador existente, respeitando a relação 1:1.",
        OperationId = "CreatePerfilCompetitivo")]
    [SwaggerResponse(201, "Perfil competitivo criado com sucesso")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(404, "Jogador não encontrado")]
    [SwaggerResponse(409, "Jogador já possui perfil competitivo")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> Create(
        PerfilCompetitivoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _perfilService.CreateAsync(dto);

        return result.IsSuccess && result.Data is not null
            ? CreatedAtAction(
                nameof(GetById),
                new { id = result.Data.Id },
                result)
            : this.ToHttpResult(result);
    }

    /// <summary>
    /// Atualiza as estatísticas e a associação de um perfil competitivo.
    /// </summary>
    /// <remarks>
    /// A atualização deve preservar a relação de um perfil por jogador.
    /// </remarks>
    /// <param name="id">Identificador do perfil competitivo.</param>
    /// <param name="dto">Novos dados do perfil competitivo.</param>
    /// <returns>Perfil competitivo atualizado.</returns>
    [Authorize]
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualiza um perfil competitivo",
        Description = "Atualiza as estatísticas e o jogador associado ao perfil, respeitando a relação 1:1.",
        OperationId = "UpdatePerfilCompetitivo")]
    [SwaggerResponse(200, "Perfil competitivo atualizado")]
    [SwaggerResponse(400, "Dados inválidos")]
    [SwaggerResponse(404, "Perfil competitivo ou jogador não encontrado")]
    [SwaggerResponse(409, "Jogador já possui outro perfil competitivo")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> Update(
        int id,
        PerfilCompetitivoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _perfilService.UpdateAsync(id, dto);

        return this.ToHttpResult(result);
    }

    /// <summary>
    /// Remove um perfil competitivo pelo seu identificador.
    /// </summary>
    /// <param name="id">Identificador do perfil competitivo.</param>
    /// <returns>Resultado da exclusão do perfil competitivo.</returns>
    [Authorize]
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remove um perfil competitivo",
        Description = "Exclui um perfil competitivo pelo identificador.",
        OperationId = "DeletePerfilCompetitivo")]
    [SwaggerResponse(204, "Perfil competitivo removido")]
    [SwaggerResponse(404, "Perfil competitivo não encontrado")]
    [SwaggerResponse(500, "Erro interno")]
    public async Task<IActionResult> Delete(int id)
    {
        var result = await _perfilService.DeleteAsync(id);

        return result.IsSuccess
            ? NoContent()
            : this.ToHttpResult(result);
    }
}