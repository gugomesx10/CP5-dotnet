using System.Data.Common;
using CP5.Application.Common;
using CP5.Application.Exceptions;
using CP5.Application.Interfaces.Repositories;
using CP5.Domain.Entities;
using CP5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;

namespace CP5.Infrastructure.Repositories;

public class JogadorRepository : IJogadorRepository
{
    private readonly ApplicationContext _context;

    public JogadorRepository(ApplicationContext context)
    {
        _context = context;
    }

    public Task<PagedResult<Jogador>> GetAllAsync(
        int pageNumber, int pageSize) =>
        QueryAsync(async () =>
        {
            var query = _context.Jogadores
                .AsNoTracking()
                .Include(j => j.Time);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(j => j.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Page(items, pageNumber, pageSize, total);
        });

    public Task<Jogador?> GetByIdAsync(int id) =>
        QueryAsync(() =>
            _context.Jogadores
                .AsNoTracking()
                .Include(j => j.Time)
                .FirstOrDefaultAsync(j => j.Id == id));

    public Task<Jogador?> GetByIdForUpdateAsync(int id) =>
        QueryAsync(() =>
            _context.Jogadores
                .FirstOrDefaultAsync(j => j.Id == id));

    public Task<PagedResult<Jogador>> GetByTimeAsync(
        int timeId, int pageNumber, int pageSize) =>
        QueryAsync(async () =>
        {
            var query = _context.Jogadores
                .AsNoTracking()
                .Include(j => j.Time)
                .Where(j => j.TimeId == timeId);

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(j => j.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Page(items, pageNumber, pageSize, total);
        });

    public Task<PagedResult<Jogador>> GetByFuncaoAsync(
        string funcao, int pageNumber, int pageSize) =>
        QueryAsync(async () =>
        {
            var query = _context.Jogadores
                .AsNoTracking()
                .Include(j => j.Time)
                .Where(j => j.Funcao.ToLower() == funcao.ToLower());

            var total = await query.CountAsync();

            var items = await query
                .OrderBy(j => j.Id)
                .Skip((pageNumber - 1) * pageSize)
                .Take(pageSize)
                .ToListAsync();

            return Page(items, pageNumber, pageSize, total);
        });

    public Task AddAsync(Jogador jogador) =>
        SaveAsync(async () =>
        {
            await _context.Jogadores.AddAsync(jogador);
            await _context.SaveChangesAsync();
        });

    public Task UpdateAsync(Jogador jogador) =>
        SaveAsync(() => _context.SaveChangesAsync());

    public Task DeleteAsync(Jogador jogador) =>
        SaveAsync(async () =>
        {
            _context.Jogadores.Remove(jogador);
            await _context.SaveChangesAsync();
        });

    private static PagedResult<Jogador> Page(
        List<Jogador> items, int page, int size, int total) => new()
    {
        Items = items,
        PageNumber = page,
        PageSize = size,
        TotalItems = total,
        TotalPages = (int)Math.Ceiling(total / (double)size)
    };

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
                "Falha de banco ao consultar jogadores.", ex);
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
                "Falha ao persistir dados do jogador.", ex);
        }
        catch (DbException ex)
        {
            throw new RepositoryException(
                "Falha de comunicação com o banco de dados.", ex);
        }
    }
}