using CP5.Application.DTOs;
using CP5.Application.Interfaces.Services;
using CP5.Application.Results;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CP5.Controllers;

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
    /// Retorna os perfis competitivos cadastrados.
    /// </summary>
    [HttpGet]
    [SwaggerOperation(
        Summary = "Lista os perfis competitivos",
        Description = "Retorna os perfis competitivos de forma paginada.",
        OperationId = "GetPerfisCompetitivos")]
    [SwaggerResponse(200, "Perfis competitivos encontrados com sucesso")]
    [SwaggerResponse(204, "Nenhum perfil competitivo encontrado")]
    [SwaggerResponse(400, "Parâmetros de paginação inválidos")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int pageNumber = 1,
        [FromQuery] int pageSize = 10)
    {
        if (pageNumber < 1 || pageSize < 1)
            return BadRequest(
                "PageNumber e PageSize devem ser maiores que zero.");

        pageSize = Math.Min(pageSize, 100);

        var perfis = await _perfilService
            .GetAllAsync(pageNumber, pageSize);

        if (!perfis.Items.Any())
            return NoContent();

        return Ok(perfis);
    }

    /// <summary>
    /// Retorna um perfil competitivo pelo ID.
    /// </summary>
    [HttpGet("{id}")]
    [SwaggerOperation(
        Summary = "Busca um perfil competitivo por ID",
        Description = "Retorna o perfil competitivo correspondente ao identificador informado.",
        OperationId = "GetPerfilCompetitivoById")]
    [SwaggerResponse(200, "Perfil competitivo encontrado com sucesso")]
    [SwaggerResponse(404, "Perfil competitivo não encontrado")]
    public async Task<IActionResult> GetById(int id)
    {
        var perfil = await _perfilService.GetByIdAsync(id);

        if (perfil == null)
            return NotFound();

        return Ok(perfil);
    }

    /// <summary>
    /// Retorna o perfil competitivo de um jogador.
    /// </summary>
    [HttpGet("jogador/{jogadorId}")]
    [SwaggerOperation(
        Summary = "Busca o perfil competitivo de um jogador",
        Description = "Retorna o perfil competitivo associado ao jogador informado.",
        OperationId = "GetPerfilByJogador")]
    [SwaggerResponse(200, "Perfil competitivo encontrado com sucesso")]
    [SwaggerResponse(404, "Perfil competitivo não encontrado para este jogador")]
    public async Task<IActionResult> GetByJogador(int jogadorId)
    {
        var perfil = await _perfilService.GetByJogadorAsync(jogadorId);

        if (perfil == null)
            return NotFound();

        return Ok(perfil);
    }

    /// <summary>
    /// Cadastra um perfil competitivo para um jogador.
    /// </summary>
    [HttpPost]
    [SwaggerOperation(
        Summary = "Cadastra um perfil competitivo",
        Description = "Cria um perfil para um jogador existente. Cada jogador pode possuir apenas um perfil competitivo.",
        OperationId = "CreatePerfilCompetitivo")]
    [SwaggerResponse(201, "Perfil competitivo criado com sucesso")]
    [SwaggerResponse(400, "Jogador já possui perfil competitivo")]
    [SwaggerResponse(404, "Jogador não encontrado")]
    public async Task<IActionResult> Create(
        PerfilCompetitivoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _perfilService.CreateAsync(dto);

        if (result.Status ==
            PerfilCompetitivoResultStatus.JogadorNaoEncontrado)
        {
            return NotFound("Jogador não encontrado.");
        }

        if (result.Status ==
            PerfilCompetitivoResultStatus.PerfilJaExiste)
        {
            return BadRequest(
                "Este jogador já possui perfil competitivo.");
        }

        return CreatedAtAction(
            nameof(GetById),
            new { id = result.Perfil!.Id },
            result.Perfil);
    }

    /// <summary>
    /// Atualiza um perfil competitivo existente.
    /// </summary>
    [HttpPut("{id}")]
    [SwaggerOperation(
        Summary = "Atualiza um perfil competitivo",
        Description = "Atualiza as estatísticas e o jogador associado ao perfil, respeitando a relação 1:1.",
        OperationId = "UpdatePerfilCompetitivo")]
    [SwaggerResponse(200, "Perfil competitivo atualizado com sucesso")]
    [SwaggerResponse(400, "Jogador já possui outro perfil competitivo")]
    [SwaggerResponse(404, "Perfil competitivo ou jogador não encontrado")]
    public async Task<IActionResult> Update(
        int id,
        PerfilCompetitivoCreateDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var result = await _perfilService.UpdateAsync(id, dto);

        if (result.Status ==
            PerfilCompetitivoResultStatus.PerfilNaoEncontrado)
        {
            return NotFound("Perfil competitivo não encontrado.");
        }

        if (result.Status ==
            PerfilCompetitivoResultStatus.JogadorNaoEncontrado)
        {
            return NotFound("Jogador não encontrado.");
        }

        if (result.Status ==
            PerfilCompetitivoResultStatus.PerfilJaExiste)
        {
            return BadRequest(
                "Este jogador já possui outro perfil competitivo.");
        }

        return Ok(result.Perfil);
    }

    /// <summary>
    /// Remove um perfil competitivo pelo ID.
    /// </summary>
    [HttpDelete("{id}")]
    [SwaggerOperation(
        Summary = "Remove um perfil competitivo",
        Description = "Exclui um perfil competitivo pelo identificador.",
        OperationId = "DeletePerfilCompetitivo")]
    [SwaggerResponse(204, "Perfil competitivo removido com sucesso")]
    [SwaggerResponse(404, "Perfil competitivo não encontrado")]
    public async Task<IActionResult> Delete(int id)
    {
        var removido = await _perfilService.DeleteAsync(id);

        if (!removido)
            return NotFound();

        return NoContent();
    }
}