using CP5.Application.Interfaces.Repositories;
using CP5.Domain.Entities;
using CP5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CP5.Application.Common;

namespace CP5.Infrastructure.Repositories;

public class TimeRepository : ITimeRepository
{
    private readonly ApplicationContext _context;

    public TimeRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Time>> GetAllAsync(
        int pageNumber,
        int pageSize)
    {
        var query = _context.Times.AsNoTracking();

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Time>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize)
        };
    }

    public async Task<Time?> GetByIdAsync(int id)
    {
        return await _context.Times
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == id);
    }

    public async Task<PagedResult<Time>> GetByJogoAsync(
        string jogo,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Times
            .AsNoTracking()
            .Where(t => t.Jogo.ToLower() == jogo.ToLower());

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return new PagedResult<Time>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize)
        };
    }

    public async Task AddAsync(Time time)
    {
        await _context.Times.AddAsync(time);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Time time)
    {
        _context.Times.Update(time);
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Time time)
    {
        _context.Times.Remove(time);
        await _context.SaveChangesAsync();
    }
}