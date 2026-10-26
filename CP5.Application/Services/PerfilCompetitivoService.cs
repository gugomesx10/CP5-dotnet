using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Interfaces.Services;
using CP5.Application.Mappings;
using CP5.Application.Results;
using Microsoft.Extensions.Logging;

namespace CP5.Application.Services;

public class PerfilCompetitivoService : IPerfilCompetitivoService
{
    private readonly IPerfilCompetitivoRepository _perfilRepository;
    private readonly IJogadorRepository _jogadorRepository;
    private readonly ILogger<PerfilCompetitivoService> _logger;

    public PerfilCompetitivoService(
        IPerfilCompetitivoRepository perfilRepository,
        IJogadorRepository jogadorRepository,
        ILogger<PerfilCompetitivoService> logger)
    {
        _perfilRepository = perfilRepository;
        _jogadorRepository = jogadorRepository;
        _logger = logger;
    }

    public async Task<PagedResult<PerfilCompetitivoResponseDto>> GetAllAsync(
        int pageNumber,
        int pageSize)
    {
        var result = await _perfilRepository
            .GetAllAsync(pageNumber, pageSize);

        return new PagedResult<PerfilCompetitivoResponseDto>
        {
            Items = result.Items.Select(
                PerfilCompetitivoMapper.ToResponseDto),

            PageNumber = result.PageNumber,
            PageSize = result.PageSize,
            TotalItems = result.TotalItems,
            TotalPages = result.TotalPages
        };
    }

    public async Task<PerfilCompetitivoResponseDto?> GetByIdAsync(int id)
    {
        var perfil = await _perfilRepository.GetByIdAsync(id);

        return perfil == null
            ? null
            : PerfilCompetitivoMapper.ToResponseDto(perfil);
    }

    public async Task<PerfilCompetitivoResponseDto?> GetByJogadorAsync(
        int jogadorId)
    {
        var perfil = await _perfilRepository.GetByJogadorAsync(jogadorId);

        return perfil == null
            ? null
            : PerfilCompetitivoMapper.ToResponseDto(perfil);
    }

    public async Task<PerfilCompetitivoResult> CreateAsync(
        PerfilCompetitivoCreateDto dto)
    {
        var jogador = await _jogadorRepository.GetByIdAsync(dto.JogadorId);

        if (jogador == null)
        {
            _logger.LogWarning(
                "Tentativa de criar perfil competitivo para o jogador inexistente {JogadorId}",
                dto.JogadorId);

            return new PerfilCompetitivoResult(
                PerfilCompetitivoResultStatus.JogadorNaoEncontrado);
        }

        var perfilJaExiste =
            await _perfilRepository.ExistsByJogadorAsync(dto.JogadorId);

        if (perfilJaExiste)
        {
            _logger.LogWarning(
                "Jogador {JogadorId} já possui perfil competitivo",
                dto.JogadorId);

            return new PerfilCompetitivoResult(
                PerfilCompetitivoResultStatus.PerfilJaExiste);
        }

        var perfil = PerfilCompetitivoMapper.ToEntity(dto);

        await _perfilRepository.AddAsync(perfil);

        _logger.LogInformation(
            "Perfil competitivo {PerfilId} criado para o jogador {JogadorId}",
            perfil.Id,
            dto.JogadorId);

        return new PerfilCompetitivoResult(
            PerfilCompetitivoResultStatus.Sucesso,
            PerfilCompetitivoMapper.ToResponseDto(perfil, jogador));
    }

    public async Task<PerfilCompetitivoResult> UpdateAsync(
        int id,
        PerfilCompetitivoCreateDto dto)
    {
        var perfil = await _perfilRepository.GetByIdForUpdateAsync(id);

        if (perfil == null)
        {
            _logger.LogWarning(
                "Tentativa de atualizar perfil competitivo inexistente {PerfilId}",
                id);

            return new PerfilCompetitivoResult(
                PerfilCompetitivoResultStatus.PerfilNaoEncontrado);
        }

        var jogador = await _jogadorRepository.GetByIdAsync(dto.JogadorId);

        if (jogador == null)
        {
            _logger.LogWarning(
                "Tentativa de associar o perfil {PerfilId} ao jogador inexistente {JogadorId}",
                id,
                dto.JogadorId);

            return new PerfilCompetitivoResult(
                PerfilCompetitivoResultStatus.JogadorNaoEncontrado);
        }

        var perfilJaExiste =
            await _perfilRepository.ExistsByJogadorAsync(
                dto.JogadorId,
                perfil.Id);

        if (perfilJaExiste)
        {
            _logger.LogWarning(
                "Jogador {JogadorId} já possui outro perfil competitivo",
                dto.JogadorId);

            return new PerfilCompetitivoResult(
                PerfilCompetitivoResultStatus.PerfilJaExiste);
        }

        PerfilCompetitivoMapper.UpdateEntity(perfil, dto);

        await _perfilRepository.UpdateAsync(perfil);

        _logger.LogInformation(
            "Perfil competitivo {PerfilId} atualizado para o jogador {JogadorId}",
            perfil.Id,
            dto.JogadorId);

        return new PerfilCompetitivoResult(
            PerfilCompetitivoResultStatus.Sucesso,
            PerfilCompetitivoMapper.ToResponseDto(perfil, jogador));
    }

    public async Task<bool> DeleteAsync(int id)
    {
        var perfil = await _perfilRepository.GetByIdForUpdateAsync(id);

        if (perfil == null)
        {
            _logger.LogWarning(
                "Tentativa de excluir perfil competitivo inexistente {PerfilId}",
                id);

            return false;
        }

        await _perfilRepository.DeleteAsync(perfil);

        _logger.LogInformation(
            "Perfil competitivo {PerfilId} excluído",
            id);

        return true;
    }
}