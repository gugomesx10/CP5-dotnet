using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Results;

namespace CP5.Application.Interfaces.Services;

public interface IPerfilCompetitivoService
{
    Task<PagedResult<PerfilCompetitivoResponseDto>> GetAllAsync(
        int pageNumber,
        int pageSize);

    Task<PerfilCompetitivoResponseDto?> GetByIdAsync(int id);

    Task<PerfilCompetitivoResponseDto?> GetByJogadorAsync(
        int jogadorId);

    Task<PerfilCompetitivoResult> CreateAsync(
        PerfilCompetitivoCreateDto dto);

    Task<PerfilCompetitivoResult> UpdateAsync(
        int id,
        PerfilCompetitivoCreateDto dto);

    Task<bool> DeleteAsync(int id);
}