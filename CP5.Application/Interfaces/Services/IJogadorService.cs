using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Common;

namespace CP5.Application.Interfaces.Services;

public interface IJogadorService
{
    Task<PagedResult<JogadorResumoDto>> GetAllAsync(
        int pageNumber,
        int pageSize);
    Task<JogadorResumoDto?> GetByIdAsync(int id);
    Task<PagedResult<JogadorResumoDto>> GetByTimeAsync(
        int timeId,
        int pageNumber,
        int pageSize);
    Task<PagedResult<JogadorResumoDto>> GetByFuncaoAsync(
        string funcao,
        int pageNumber,
        int pageSize);
    Task<JogadorResumoDto?> CreateAsync(JogadorCreateDto dto);
    Task<JogadorResumoDto?> UpdateAsync(int id, JogadorCreateDto dto);
    Task<bool> DeleteAsync(int id);
}