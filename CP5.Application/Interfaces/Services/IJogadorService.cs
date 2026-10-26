using CP5.Application.Common;
using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Application.Results;

namespace CP5.Application.Interfaces.Services;

public interface IJogadorService
{
    Task<OperationResult<PagedResult<JogadorResumoDto>>> GetAllAsync(int pageNumber, int pageSize);
    Task<OperationResult<JogadorResumoDto>> GetByIdAsync(int id);
    Task<OperationResult<PagedResult<JogadorResumoDto>>> GetByTimeAsync(int timeId, int pageNumber, int pageSize);
    Task<OperationResult<PagedResult<JogadorResumoDto>>> GetByFuncaoAsync(string funcao, int pageNumber, int pageSize);
    Task<OperationResult<JogadorResumoDto>> CreateAsync(JogadorCreateDto dto);
    Task<OperationResult<JogadorResumoDto>> UpdateAsync(int id, JogadorCreateDto dto);
    Task<OperationResult<bool>> DeleteAsync(int id);
}