using CP5.Application.Interfaces.Services;
using CP5.Application.Results;
using Microsoft.AspNetCore.Mvc;
using Swashbuckle.AspNetCore.Annotations;

namespace CP5.Controllers;

/// <summary>
/// Consulta informações públicas de equipes de Dota 2.
/// </summary>
[ApiController]
[Route("api/[controller]")]
public sealed class OpenDotaController : ControllerBase
{
    private readonly IOpenDotaService _service;

    public OpenDotaController(IOpenDotaService service)
    {
        _service = service;
    }

    /// <summary>
    /// Busca informações de uma equipe na API externa OpenDota.
    /// </summary>
    /// <param name="id">Identificador da equipe na OpenDota.</param>
    /// <param name="cancellationToken">Token de cancelamento.</param>
    /// <returns>Dados públicos da equipe consultada.</returns>
    [HttpGet("teams/{id:int}")]
    [SwaggerOperation(
        Summary = "Consulta uma equipe de Dota 2",
        Description = "Busca informações na API pública OpenDota.",
        OperationId = "GetOpenDotaTeam")]
    [SwaggerResponse(200, "Equipe encontrada")]
    [SwaggerResponse(400, "Identificador inválido")]
    [SwaggerResponse(404, "Equipe não encontrada")]
    [SwaggerResponse(503, "Serviço externo indisponível")]
    public async Task<IActionResult> GetTeam(
        int id,
        CancellationToken cancellationToken)
    {
        var result = await _service.GetTeamAsync(
            id,
            cancellationToken);

        return this.ToHttpResult(result);
    }
}