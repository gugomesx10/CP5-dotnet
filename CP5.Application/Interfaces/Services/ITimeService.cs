using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Results;

namespace CP5.Application.Interfaces.Services;

public interface ITimeService
{
    Task<OperationResult<PagedResult<TimeResumoDto>>> GetAllAsync(
        int pageNumber,
        int pageSize);
    Task<OperationResult<TimeResumoDto>> GetByIdAsync(int id);
    Task<OperationResult<PagedResult<TimeResumoDto>>> GetByJogoAsync(
        string jogo,
        int pageNumber,
        int pageSize);
    Task<OperationResult<TimeResumoDto>> CreateAsync(TimeCreateDto dto);
    Task<OperationResult<TimeResumoDto>> UpdateAsync(int id, TimeCreateDto dto);
    Task<OperationResult<bool>> DeleteAsync(int id);
}