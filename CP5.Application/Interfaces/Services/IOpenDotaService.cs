using CP5.Application.DTOs.Responses;
using CP5.Application.Results;

namespace CP5.Application.Interfaces.Services;

public interface IOpenDotaService
{
    Task<OperationResult<OpenDotaTeamDto>> GetTeamAsync(
        int teamId,
        CancellationToken cancellationToken = default);
}