using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using CP5.Application.DTOs.Responses;
using CP5.Application.Interfaces.Services;
using CP5.Application.Results;
using Microsoft.Extensions.Logging;

namespace CP5.Infrastructure.ExternalServices;

public sealed class OpenDotaService : IOpenDotaService
{
    private readonly HttpClient _client;
    private readonly ILogger<OpenDotaService> _logger;

    public OpenDotaService(
        HttpClient client,
        ILogger<OpenDotaService> logger)
    {
        _client = client;
        _logger = logger;
    }

    public async Task<OperationResult<OpenDotaTeamDto>> GetTeamAsync(
        int teamId,
        CancellationToken cancellationToken = default)
    {
        if (teamId <= 0)
        {
            return OperationResult<OpenDotaTeamDto>.Failure(
                OperationStatus.ValidationError,
                "INVALID_TEAM_ID",
                "O identificador deve ser maior que zero.");
        }

        try
        {
            using var response = await _client.GetAsync(
                $"teams/{teamId}",
                cancellationToken);

            if (response.StatusCode == HttpStatusCode.NotFound)
            {
                return OperationResult<OpenDotaTeamDto>.Failure(
                    OperationStatus.NotFound,
                    "EXTERNAL_TEAM_NOT_FOUND",
                    "Equipe não encontrada na OpenDota.");
            }

            if (!response.IsSuccessStatusCode)
            {
                _logger.LogWarning(
                    "OpenDota retornou HTTP {StatusCode}",
                    (int)response.StatusCode);

                return ExternalFailure();
            }

            var team = await response.Content
                .ReadFromJsonAsync<OpenDotaTeamDto>(
                    cancellationToken: cancellationToken);

            if (team is null || team.TeamId <= 0)
            {
                return ExternalFailure();
            }

            return OperationResult<OpenDotaTeamDto>.Success(
                team,
                "Equipe consultada na OpenDota com sucesso.");
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Falha HTTP na OpenDota");
            return ExternalFailure();
        }
        catch (TaskCanceledException ex)
            when (!cancellationToken.IsCancellationRequested)
        {
            _logger.LogWarning(ex, "Timeout na OpenDota");
            return ExternalFailure();
        }
        catch (JsonException ex)
        {
            _logger.LogError(ex, "JSON inválido da OpenDota");
            return ExternalFailure();
        }
    }

    private static OperationResult<OpenDotaTeamDto> ExternalFailure() =>
        OperationResult<OpenDotaTeamDto>.Failure(
            OperationStatus.ExternalServiceFailure,
            "EXTERNAL_SERVICE_ERROR",
            "A OpenDota está temporariamente indisponível.");
}