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

public class TimeService : ITimeService
{
    private readonly ITimeRepository _timeRepository;
    private readonly ILogger<TimeService> _logger;

    public TimeService(
        ITimeRepository timeRepository,
        ILogger<TimeService> logger)
    {
        _timeRepository = timeRepository;
        _logger = logger;
    }

    public Task<OperationResult<PagedResult<TimeResumoDto>>> GetAllAsync(
        int pageNumber,
        int pageSize)
    {
        return QueryAsync(() =>
            _timeRepository.GetAllAsync(pageNumber, pageSize));
    }

    public async Task<OperationResult<TimeResumoDto>> GetByIdAsync(int id)
    {
        try
        {
            var time = await _timeRepository.GetRequiredByIdAsync(id);

            return OperationResult<TimeResumoDto>.Success(
                TimeMapper.ToResumoDto(time),
                "Time encontrado com sucesso.");
        }
        catch (EntityNotFoundException)
        {
            _logger.LogWarning(
                "Time não encontrado: {TimeId}", id);

            return OperationResult<TimeResumoDto>.Failure(
                OperationStatus.NotFound,
                "TIME_NOT_FOUND",
                "O time informado não foi encontrado.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex,
                "Erro ao consultar o time {TimeId}", id);

            return OperationResult<TimeResumoDto>.Failure(
                OperationStatus.UnexpectedError,
                "DATABASE_ERROR",
                "Não foi possível consultar o time.");
        }
    }

    public Task<OperationResult<PagedResult<TimeResumoDto>>> GetByJogoAsync(
        string jogo,
        int pageNumber,
        int pageSize)
    {
        return QueryAsync(() =>
            _timeRepository.GetByJogoAsync(
                jogo,
                pageNumber,
                pageSize));
    }

    public async Task<OperationResult<TimeResumoDto>> CreateAsync(
        TimeCreateDto dto)
    {
        try
        {
            var time = TimeMapper.ToEntity(dto);

            await _timeRepository.AddAsync(time);

            _logger.LogInformation(
                "Time {TimeId} criado para o jogo {Jogo}",
                time.Id,
                time.Jogo);

            return OperationResult<TimeResumoDto>.Success(
                TimeMapper.ToResumoDto(time),
                "Time cadastrado com sucesso.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex,
                "Erro ao cadastrar time {Nome}",
                dto.Nome);

            return OperationResult<TimeResumoDto>.Failure(
                OperationStatus.UnexpectedError,
                "DATABASE_ERROR",
                "Não foi possível cadastrar o time.");
        }
    }

    public async Task<OperationResult<TimeResumoDto>> UpdateAsync(
        int id,
        TimeCreateDto dto)
    {
        try
        {
            var time = await _timeRepository.GetRequiredByIdAsync(id);

            TimeMapper.UpdateEntity(time, dto);

            await _timeRepository.UpdateAsync(time);

            _logger.LogInformation(
                "Time {TimeId} atualizado com sucesso",
                id);

            return OperationResult<TimeResumoDto>.Success(
                TimeMapper.ToResumoDto(time),
                "Time atualizado com sucesso.");
        }
        catch (EntityNotFoundException)
        {
            _logger.LogWarning(
                "Tentativa de atualizar time inexistente {TimeId}",
                id);

            return OperationResult<TimeResumoDto>.Failure(
                OperationStatus.NotFound,
                "TIME_NOT_FOUND",
                "O time informado não foi encontrado.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex,
                "Erro ao atualizar time {TimeId}",
                id);

            return OperationResult<TimeResumoDto>.Failure(
                OperationStatus.UnexpectedError,
                "DATABASE_ERROR",
                "Não foi possível atualizar o time.");
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var time = await _timeRepository.GetRequiredByIdAsync(id);

            await _timeRepository.DeleteAsync(time);

            _logger.LogInformation(
                "Time {TimeId} excluído com sucesso",
                id);

            return OperationResult<bool>.Success(
                true,
                "Time excluído com sucesso.");
        }
        catch (EntityNotFoundException)
        {
            _logger.LogWarning(
                "Tentativa de excluir time inexistente {TimeId}",
                id);

            return OperationResult<bool>.Failure(
                OperationStatus.NotFound,
                "TIME_NOT_FOUND",
                "O time informado não foi encontrado.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex,
                "Erro ao excluir time {TimeId}",
                id);

            return OperationResult<bool>.Failure(
                OperationStatus.UnexpectedError,
                "DATABASE_ERROR",
                "Não foi possível excluir o time.");
        }
    }

    private async Task<OperationResult<PagedResult<TimeResumoDto>>> QueryAsync(
        Func<Task<PagedResult<Time>>> query)
    {
        try
        {
            var result = await query();

            return OperationResult<PagedResult<TimeResumoDto>>.Success(
                MapPagedResult(result),
                "Consulta de times realizada com sucesso.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex,
                "Erro ao consultar a lista de times");

            return OperationResult<PagedResult<TimeResumoDto>>.Failure(
                OperationStatus.UnexpectedError,
                "DATABASE_ERROR",
                "Não foi possível consultar os times.");
        }
    }

    private static PagedResult<TimeResumoDto> MapPagedResult(
        PagedResult<Time> result)
    {
        return new PagedResult<TimeResumoDto>
        {
            Items = result.Items.Select(TimeMapper.ToResumoDto),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };
    }
}