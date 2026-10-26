using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Results;

namespace CP5.Application.Interfaces.Services;

public interface IPerfilCompetitivoService
{
    Task<OperationResult<PagedResult<PerfilCompetitivoResponseDto>>> GetAllAsync(int pageNumber, int pageSize);
    Task<OperationResult<PerfilCompetitivoResponseDto>> GetByIdAsync(int id);
    Task<OperationResult<PerfilCompetitivoResponseDto>> GetByJogadorAsync(int jogadorId);
    Task<OperationResult<PerfilCompetitivoResponseDto>> CreateAsync(PerfilCompetitivoCreateDto dto);
    Task<OperationResult<PerfilCompetitivoResponseDto>> UpdateAsync(int id, PerfilCompetitivoCreateDto dto);
    Task<OperationResult<bool>> DeleteAsync(int id);
}