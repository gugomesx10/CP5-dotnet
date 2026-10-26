using CP5.Application.Common;
using CP5.Domain.Entities;

namespace CP5.Application.Interfaces.Repositories;

public interface IJogadorRepository
{
    Task<PagedResult<Jogador>> GetAllAsync(
        int pageNumber,
        int pageSize);

    Task<Jogador?> GetByIdAsync(int id);
    Task<Jogador?> GetByIdForUpdateAsync(int id);

    Task<PagedResult<Jogador>> GetByTimeAsync(
        int timeId,
        int pageNumber,
        int pageSize);

    Task<PagedResult<Jogador>> GetByFuncaoAsync(
        string funcao,
        int pageNumber,
        int pageSize);

    Task AddAsync(Jogador jogador);
    Task UpdateAsync(Jogador jogador);
    Task DeleteAsync(Jogador jogador);
}