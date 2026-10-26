using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Exceptions;
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

    public Task<OperationResult<PagedResult<PerfilCompetitivoResponseDto>>> GetAllAsync(
        int pageNumber, int pageSize) =>
        QueryAsync(async () =>
        {
            var result = await _perfilRepository.GetAllAsync(
                pageNumber, pageSize);

            return new PagedResult<PerfilCompetitivoResponseDto>
            {
                Items = result.Items.Select(
                    PerfilCompetitivoMapper.ToResponseDto),
                PageNumber = result.PageNumber,
                PageSize = result.PageSize,
                TotalItems = result.TotalItems,
                TotalPages = result.TotalPages
            };
        });

    public async Task<OperationResult<PerfilCompetitivoResponseDto>> GetByIdAsync(
        int id)
    {
        try
        {
            var perfil = await _perfilRepository.GetByIdAsync(id);

            return perfil is null
                ? NotFound(
                    "PERFIL_NOT_FOUND",
                    "Perfil competitivo não encontrado.")
                : OperationResult<PerfilCompetitivoResponseDto>.Success(
                    PerfilCompetitivoMapper.ToResponseDto(perfil));
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex, "Falha ao consultar perfil {PerfilId}", id);

            return DbError<PerfilCompetitivoResponseDto>();
        }
    }

    public async Task<OperationResult<PerfilCompetitivoResponseDto>> GetByJogadorAsync(
        int jogadorId)
    {
        try
        {
            var perfil = await _perfilRepository.GetByJogadorAsync(jogadorId);

            return perfil is null
                ? NotFound(
                    "PERFIL_NOT_FOUND",
                    "Perfil competitivo não encontrado para o jogador.")
                : OperationResult<PerfilCompetitivoResponseDto>.Success(
                    PerfilCompetitivoMapper.ToResponseDto(perfil));
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao consultar perfil do jogador {JogadorId}",
                jogadorId);

            return DbError<PerfilCompetitivoResponseDto>();
        }
    }

    public async Task<OperationResult<PerfilCompetitivoResponseDto>> CreateAsync(
        PerfilCompetitivoCreateDto dto)
    {
        try
        {
            var jogador = await _jogadorRepository.GetByIdAsync(dto.JogadorId);

            if (jogador is null)
            {
                return NotFound(
                    "JOGADOR_NOT_FOUND",
                    "Jogador não encontrado.");
            }

            if (await _perfilRepository.ExistsByJogadorAsync(dto.JogadorId))
            {
                return OperationResult<PerfilCompetitivoResponseDto>.Failure(
                    OperationStatus.Conflict,
                    "PERFIL_ALREADY_EXISTS",
                    "Este jogador já possui perfil competitivo.");
            }

            var perfil = PerfilCompetitivoMapper.ToEntity(dto);

            await _perfilRepository.AddAsync(perfil);

            return OperationResult<PerfilCompetitivoResponseDto>.Success(
                PerfilCompetitivoMapper.ToResponseDto(perfil, jogador),
                "Perfil competitivo cadastrado com sucesso.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex,
                "Falha ao cadastrar perfil para jogador {JogadorId}",
                dto.JogadorId);

            return DbError<PerfilCompetitivoResponseDto>();
        }
    }

    public async Task<OperationResult<PerfilCompetitivoResponseDto>> UpdateAsync(
        int id, PerfilCompetitivoCreateDto dto)
    {
        try
        {
            var perfil = await _perfilRepository.GetByIdForUpdateAsync(id);

            if (perfil is null)
            {
                return NotFound(
                    "PERFIL_NOT_FOUND",
                    "Perfil competitivo não encontrado.");
            }

            var jogador = await _jogadorRepository.GetByIdAsync(dto.JogadorId);

            if (jogador is null)
            {
                return NotFound(
                    "JOGADOR_NOT_FOUND",
                    "Jogador não encontrado.");
            }

            if (await _perfilRepository.ExistsByJogadorAsync(
                dto.JogadorId, perfil.Id))
            {
                return OperationResult<PerfilCompetitivoResponseDto>.Failure(
                    OperationStatus.Conflict,
                    "PERFIL_ALREADY_EXISTS",
                    "O jogador já possui outro perfil competitivo.");
            }

            PerfilCompetitivoMapper.UpdateEntity(perfil, dto);

            await _perfilRepository.UpdateAsync(perfil);

            return OperationResult<PerfilCompetitivoResponseDto>.Success(
                PerfilCompetitivoMapper.ToResponseDto(perfil, jogador),
                "Perfil competitivo atualizado com sucesso.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex, "Falha ao atualizar perfil {PerfilId}", id);

            return DbError<PerfilCompetitivoResponseDto>();
        }
    }

    public async Task<OperationResult<bool>> DeleteAsync(int id)
    {
        try
        {
            var perfil = await _perfilRepository.GetByIdForUpdateAsync(id);

            if (perfil is null)
            {
                return OperationResult<bool>.Failure(
                    OperationStatus.NotFound,
                    "PERFIL_NOT_FOUND",
                    "Perfil competitivo não encontrado.");
            }

            await _perfilRepository.DeleteAsync(perfil);

            return OperationResult<bool>.Success(
                true, "Perfil competitivo excluído com sucesso.");
        }
        catch (RepositoryException ex)
        {
            _logger.LogError(
                ex, "Falha ao excluir perfil {PerfilId}", id);

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
            _logger.LogError(
                ex, "Falha ao consultar perfis competitivos");

            return DbError<T>();
        }
    }

    private static OperationResult<PerfilCompetitivoResponseDto> NotFound(
        string code, string message) =>
        OperationResult<PerfilCompetitivoResponseDto>.Failure(
            OperationStatus.NotFound, code, message);

    private static OperationResult<T> DbError<T>() =>
        OperationResult<T>.Failure(
            OperationStatus.UnexpectedError,
            "DATABASE_ERROR",
            "Não foi possível acessar os dados dos perfis competitivos.");
}