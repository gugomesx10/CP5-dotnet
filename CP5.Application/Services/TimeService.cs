using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Interfaces.Services;
using CP5.Application.Mappings;
using CP5.Domain.Entities;
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

    public async Task<PagedResult<TimeResumoDto>> GetAllAsync(
        int pageNumber,
        int pageSize)
    {
        var result = await _timeRepository
            .GetAllAsync(pageNumber, pageSize);

        return MapPagedResult(result);
    }

    public async Task<TimeResumoDto?> GetByIdAsync(int id)
    {
        var time = await _timeRepository.GetByIdAsync(id);

        return time == null
            ? null
            : TimeMapper.ToResumoDto(time);
    }

    public async Task<PagedResult<TimeResumoDto>> GetByJogoAsync(
        string jogo,
        int pageNumber,
        int pageSize)
    {
        var result = await _timeRepository
            .GetByJogoAsync(jogo, pageNumber, pageSize);

        return MapPagedResult(result);
    }

    public async Task<TimeResumoDto> CreateAsync(TimeCreateDto dto)
    {
        var time = TimeMapper.ToEntity(dto);

        await _timeRepository.AddAsync(time);

        _logger.LogInformation(
            "Time {TimeId} criado para o jogo {Jogo}",
            time.Id,
            time.Jogo);

        return TimeMapper.ToResumoDto(time);
    }

    public async Task<TimeResumoDto?> UpdateAsync(
        int id,
        TimeCreateDto dto)
    {
        var time = await _timeRepository.GetByIdAsync(id);

        if (time == null)
        {
            _logger.LogWarning(
                "Tentativa de atualizar time inexistente {TimeId}",
                id);

            return null;
        }

        TimeMapper.UpdateEntity(time, dto);

        await _timeRepository.UpdateAsync(time);

        _logger.LogInformation(
            "Time {TimeId} atualizado",
            time.Id);

        return TimeMapper.ToResumoDto(time);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var time = await _timeRepository.GetByIdAsync(id);

        if (time == null)
        {
            _logger.LogWarning(
                "Tentativa de excluir time inexistente {TimeId}",
                id);

            return false;
        }

        await _timeRepository.DeleteAsync(time);

        _logger.LogInformation(
            "Time {TimeId} excluído",
            id);

        return true;
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