using System.Data.Common;
using CP5.Application.Common;
using CP5.Application.Exceptions;
using CP5.Application.Interfaces.Repositories;
using CP5.Domain.Entities;
using CP5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CP5.Infrastructure.Repositories;

public class PerfilCompetitivoRepository : IPerfilCompetitivoRepository
{
    private readonly ApplicationContext _context;

    public PerfilCompetitivoRepository(ApplicationContext context)
    {
        _context = context;
    }

    public Task<PagedResult<PerfilCompetitivo>> GetAllAsync(
        int pageNumber, int pageSize) =>
        QueryAsync(async () =>
        {
            var query = _context.PerfisCompetitivos
                .AsNoTracking()
                .Include(p => p.Jogador)
                .ThenInclude(j => j!.Time);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(p => p.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return new PagedResult<PerfilCompetitivo>
            {
                Items = items,
                PageNumber = pageNumber,
                PageSize = pageSize,
                TotalItems = total,
                TotalPages = (int)Math.Ceiling(
                    total / (double)pageSize)
            };
        });

    public Task<PerfilCompetitivo?> GetByIdAsync(int id) =>
        QueryAsync(() =>
            _context.PerfisCompetitivos
                .AsNoTracking()
                .Include(p => p.Jogador)
                .ThenInclude(j => j!.Time)
                .FirstOrDefaultAsync(p => p.Id == id));

    public Task<PerfilCompetitivo?> GetByJogadorAsync(int jogadorId) =>
        QueryAsync(() =>
            _context.PerfisCompetitivos
                .AsNoTracking()
                .Include(p => p.Jogador)
                .ThenInclude(j => j!.Time)
                .FirstOrDefaultAsync(p => p.JogadorId == jogadorId));

    public Task<PerfilCompetitivo?> GetByIdForUpdateAsync(int id) =>
        QueryAsync(() =>
            _context.PerfisCompetitivos
                .FirstOrDefaultAsync(p => p.Id == id));

    public Task<bool> ExistsByJogadorAsync(
        int jogadorId, int? perfilIdIgnorado = null) =>
        QueryAsync(() =>
            _context.PerfisCompetitivos.AnyAsync(p =>
                p.JogadorId == jogadorId &&
                (!perfilIdIgnorado.HasValue ||
                 p.Id != perfilIdIgnorado.Value)));

    public Task AddAsync(PerfilCompetitivo perfil) =>
        SaveAsync(async () =>
        {
            await _context.PerfisCompetitivos.AddAsync(perfil);
            await _context.SaveChangesAsync();
        });

    public Task UpdateAsync(PerfilCompetitivo perfil) =>
        SaveAsync(() => _context.SaveChangesAsync());

    public Task DeleteAsync(PerfilCompetitivo perfil) =>
        SaveAsync(async () =>
        {
            _context.PerfisCompetitivos.Remove(perfil);
            await _context.SaveChangesAsync();
        });

    private static async Task<T> QueryAsync<T>(
        Func<Task<T>> operation)
    {
        try
        {
            return await operation();
        }
        catch (DbException ex)
        {
            throw new RepositoryException(
                "Falha de banco ao consultar perfis competitivos.", ex);
        }
    }

    private static async Task SaveAsync(Func<Task> operation)
    {
        try
        {
            await operation();
        }
        catch (DbUpdateException ex)
        {
            throw new RepositoryException(
                "Falha ao persistir perfil competitivo.", ex);
        }
        catch (DbException ex)
        {
            throw new RepositoryException(
                "Falha de comunicação com o banco de dados.", ex);
        }
    }
}