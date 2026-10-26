using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Interfaces.Services;
using CP5.Application.Mappings;
using CP5.Application.Common;
using CP5.Domain.Entities;
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

    public async Task<PagedResult<JogadorResumoDto>> GetAllAsync(
        int pageNumber,
        int pageSize)
    {
        var result = await _jogadorRepository
            .GetAllAsync(pageNumber, pageSize);

        return MapPagedResult(result);
    }

    public async Task<JogadorResumoDto?> GetByIdAsync(int id)
    {
        var jogador = await _jogadorRepository.GetByIdAsync(id);

        return jogador == null
            ? null
            : JogadorMapper.ToResumoDto(jogador);
    }

    public async Task<PagedResult<JogadorResumoDto>> GetByTimeAsync(
        int timeId,
        int pageNumber,
        int pageSize)
    {
        var result = await _jogadorRepository
            .GetByTimeAsync(timeId, pageNumber, pageSize);

        return MapPagedResult(result);
    }

    public async Task<PagedResult<JogadorResumoDto>> GetByFuncaoAsync(
        string funcao,
        int pageNumber,
        int pageSize)
    {
        var result = await _jogadorRepository
            .GetByFuncaoAsync(funcao, pageNumber, pageSize);

        return MapPagedResult(result);
    }

    private static PagedResult<JogadorResumoDto> MapPagedResult(
        PagedResult<Jogador> result)
    {
        return new PagedResult<JogadorResumoDto>
        {
            Items = result.Items.Select(JogadorMapper.ToResumoDto),
            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };
    }

    public async Task<JogadorResumoDto?> CreateAsync(JogadorCreateDto dto)
    {
        var time = await _timeRepository.GetByIdAsync(dto.TimeId);

        if (time == null)
        {
            _logger.LogWarning(
                "Tentativa de criar jogador para o time inexistente {TimeId}",
                dto.TimeId);

            return null;
        }

        var jogador = JogadorMapper.ToEntity(dto);

        await _jogadorRepository.AddAsync(jogador);

        _logger.LogInformation(
            "Jogador {JogadorId} criado para o time {TimeId}",
            jogador.Id,
            dto.TimeId);

        return JogadorMapper.ToResumoDto(jogador, time);
    }

    public async Task<JogadorResumoDto?> UpdateAsync(
        int id,
        JogadorCreateDto dto)
    {
        var jogador = await _jogadorRepository.GetByIdForUpdateAsync(id);

        if (jogador == null)
        {
            _logger.LogWarning(
                "Tentativa de atualizar jogador inexistente {JogadorId}",
                id);

            return null;
        }

        var time = await _timeRepository.GetByIdAsync(dto.TimeId);

        if (time == null)
        {
            _logger.LogWarning(
                "Tentativa de associar o jogador {JogadorId} ao time inexistente {TimeId}",
                id,
                dto.TimeId);

            return null;
        }

        JogadorMapper.UpdateEntity(jogador, dto);

        await _jogadorRepository.UpdateAsync(jogador);

        _logger.LogInformation(
            "Jogador {JogadorId} atualizado para o time {TimeId}",
            jogador.Id,
            dto.TimeId);

        return JogadorMapper.ToResumoDto(jogador, time);
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var jogador = await _jogadorRepository.GetByIdForUpdateAsync(id);

        if (jogador == null)
        {
            _logger.LogWarning(
                "Tentativa de excluir jogador inexistente {JogadorId}",
                id);

            return false;
        }

        await _jogadorRepository.DeleteAsync(jogador);

        _logger.LogInformation(
            "Jogador {JogadorId} excluído",
            id);

        return true;
    }
}