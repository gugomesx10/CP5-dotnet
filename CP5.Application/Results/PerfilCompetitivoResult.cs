using CP5.Application.DTOs.Responses;

namespace CP5.Application.Results;

public enum PerfilCompetitivoResultStatus
{
    Sucesso,
    PerfilNaoEncontrado,
    JogadorNaoEncontrado,
    PerfilJaExiste
}

public record PerfilCompetitivoResult(
    PerfilCompetitivoResultStatus Status,
    PerfilCompetitivoResponseDto? Perfil = null);