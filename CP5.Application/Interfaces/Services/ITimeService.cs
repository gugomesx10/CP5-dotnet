using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;

namespace CP5.Application.Interfaces.Services;

public interface ITimeService
{
    Task<PagedResult<TimeResumoDto>> GetAllAsync(
        int pageNumber,
        int pageSize);

    Task<TimeResumoDto?> GetByIdAsync(int id);
    Task<PagedResult<TimeResumoDto>> GetByJogoAsync(
        string jogo,
        int pageNumber,
        int pageSize);
    Task<TimeResumoDto> CreateAsync(TimeCreateDto dto);
    Task<TimeResumoDto?> UpdateAsync(int id, TimeCreateDto dto);
    Task<bool> DeleteAsync(int id);
}