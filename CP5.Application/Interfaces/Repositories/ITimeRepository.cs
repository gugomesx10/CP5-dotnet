using CP5.Application.Common;
using CP5.Domain.Entities;

namespace CP5.Application.Interfaces.Repositories;

public interface ITimeRepository
{
    Task<PagedResult<Time>> GetAllAsync(int pageNumber, int pageSize);
    Task<Time?> GetByIdAsync(int id);
    Task<PagedResult<Time>> GetByJogoAsync(
        string jogo,
        int pageNumber,
        int pageSize);
    Task AddAsync(Time time);
    Task UpdateAsync(Time time);
    Task DeleteAsync(Time time);
}