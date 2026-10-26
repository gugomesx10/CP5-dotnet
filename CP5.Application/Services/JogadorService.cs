using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Exceptions;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Interfaces.Services;
using CP5.Application.Mappings;
using CP5.Application.Results;
using CP5.Domain.Entities;
using CP5.Domain.Exceptions;
using Microsoft.Extensions.Logging;

namespace CP5.Application.Services;

public class JogadorService : IJogadorService
{
    private readonly IJogadorRepository _jogadorRepository;
    private readonly ITimeRepository _timeRepository;
    private readonly ILogger<JogadorService> _logger;

    public JogadorService(
        IJogadorRepository jogadorRepository,
        ITimeRepository timeRepository,
        ILogger<JogadorService> logger)
    {
        _jogadorRepository = jogadorRepository;
        _timeRepository = timeRepository;
        _logger = logger;
    }

    public Task<OperationResult<PagedResult<JogadorResumoDto>>> GetAllAsync(
        int pageNumber, int pageSize) =>
        QueryAsync(async () =>
            MapPagedResult(
                await _jogadorRepository.GetAllAsync(pageNumber, pageSize)));

    public Task<OperationResult<PagedResult<JogadorResumoDto>>> GetByTimeAsync(
        int timeId, int pageNumber, int pageSize) =>
        QueryAsync(async () =>
            MapPagedResult(
                await _jogadorRepository.GetByTimeAsync(
                    timeId, pageNumber, pageSize)));

    public Task<OperationResult<PagedResult<JogadorResumoDto>>> GetByFuncaoAsync(
        string funcao, int pageNumber, int pageSize) =>
        QueryAsync(async () =>
            MapPagedResult(
                await _jogadorRepository.GetByFuncaoAsync(
                    funcao, pageNumber, pageSize)));

    public async Task<OperationResult<JogadorResumoDto>> GetByIdAsync(int id)
    {
        try
        {
            var jogador = await _jogadorRepository.GetByIdAsync(id);

            return jogador is null
                ? OperationResult<JogadorResumoDto>.Failure(
                    OperationStatus.NotFound,
                    "JOGADOR_NOT_FOUND",
                    "Jogador não encontrado.")
                : OperationResult<JogadorResumoDto>.Success(
                    JogadorMapper.ToResumoDto(jogador));
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex, "Falha ao consultar jogador {JogadorId}", id);

            return DbError<JogadorResumoDto>();
        }
    }

    public async Task<OperationResult<JogadorResumoDto>> CreateAsync(
        JogadorCreateDto dto)
    {
        try
        {
            var time = await _timeRepository.GetRequiredByIdAsync(dto.TimeId);

            var jogador = JogadorMapper.ToEntity(dto);

            await _jogadorRepository.AddAsync(jogador);

            _logger.LogInformation(
                "Jogador {JogadorId} criado", jogador.Id);

            return OperationResult<JogadorResumoDto>.Success(
                JogadorMapper.ToResumoDto(jogador, time),
                "Jogador cadastrado com sucesso.");
        }
        catch (EntityNotFoundException)
        {
            return OperationResult<JogadorResumoDto>.Failure(
                OperationStatus.NotFound,
                "TIME_NOT_FOUND",
                "O time informado não foi encontrado.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(ex, "Falha ao criar jogador");

            return DbError<JogadorResumoDto>();
        }
    }

    public async Task<OperationResult<JogadorResumoDto>> UpdateAsync(
        int id, JogadorCreateDto dto)
    {
        try
        {
            var jogador = await _jogadorRepository.GetByIdForUpdateAsync(id);

            if (jogador is null)
            {
                return OperationResult<JogadorResumoDto>.Failure(
                    OperationStatus.NotFound,
                    "JOGADOR_NOT_FOUND",
                    "Jogador não encontrado.");
            }

            var time = await _timeRepository.GetRequiredByIdAsync(dto.TimeId);

            JogadorMapper.UpdateEntity(jogador, dto);

            await _jogadorRepository.UpdateAsync(jogador);

            return OperationResult<JogadorResumoDto>.Success(
                JogadorMapper.ToResumoDto(jogador, time),
                "Jogador atualizado com sucesso.");
        }
        catch (EntityNotFoundException)
        {
            return OperationResult<JogadorResumoDto>.Failure(
                OperationStatus.NotFound,
                "TIME_NOT_FOUND",
                "O time informado não foi encontrado.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex, "Falha ao atualizar jogador {JogadorId}", id);

            return DbError<JogadorResumoDto>();
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var jogador = await _jogadorRepository.GetByIdForUpdateAsync(id);

            if (jogador is null)
            {
                return OperationResult<bool>.Failure(
                    OperationStatus.NotFound,
                    "JOGADOR_NOT_FOUND",
                    "Jogador não encontrado.");
            }

            await _jogadorRepository.DeleteAsync(jogador);

            return OperationResult<bool>.Success(
                true, "Jogador excluído com sucesso.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex, "Falha ao excluir jogador {JogadorId}", id);

            return DbError<bool>();
        }
    }

    private async Task<OperationResult<T>> QueryAsync<T>(
        Func<Task<T>> operation)
    {
        try
        {
            return OperationResult<T>.Success(await operation());
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(ex, "Falha na consulta de jogadores");

            return DbError<T>();
        }
    }

    private static OperationResult<T> DbError<T>() =>
        OperationResult<T>.Failure(
            OperationStatus.UnexpectedError,
            "DATABASE_ERROR",
            "Não foi possível acessar os dados dos jogadores.");

    private static PagedResult<JogadorResumoDto> MapPagedResult(
        PagedResult<Jogador> result) => new()
    {
        Items = result.Items.Select(JogadorMapper.ToResumoDto),
        PageNumber = result.PageNumber,
        PageSize = result.PageSize,
        TotalItems = result.TotalItems,
        TotalPages = result.TotalPages
    };
}