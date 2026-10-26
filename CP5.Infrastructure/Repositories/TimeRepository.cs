using CP5.Application.Interfaces.Repositories;
using CP5.Domain.Entities;
using CP5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CP5.Application.Common;
using System.Data.Common;
using CP5.Application.Exceptions;
using CP5.Domain.Exceptions;

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
        try
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
        catch (DbException ex)
        {
            throw new RepositoryException(
                "Falha ao consultar os times no banco de dados.",
                ex);
        }
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
        try
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
        catch (DbException ex)
        {
            throw new RepositoryException(
                "Falha ao consultar times por jogo.",
                ex);
        }
    }
    
    public async Task<Time> GetRequiredByIdAsync(int id)
    {
        try
        {
            var time = await _context.Times
                .AsNoTracking()
                .FirstOrDefaultAsync(t => t.Id == id);

            if (time is null)
                throw new EntityNotFoundException("Time", id);

            return time;
        }
        catch (DbException ex)
        {
            throw new RepositoryException(
                "Falha ao consultar o banco de dados.",
                ex);
        }
    }

    public async Task AddAsync(Time time)
    {
        try
        {
            await _context.Times.AddAsync(time);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new RepositoryException(
                "Falha ao cadastrar o time no banco de dados.",
                ex);
        }
        catch (DbException ex)
        {
            throw new RepositoryException(
                "Falha de comunicação com o banco de dados.",
                ex);
        }
    }

    public async Task UpdateAsync(Time time)
    {
        try
        {
            _context.Times.Update(time);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new RepositoryException(
                "Falha ao atualizar o time no banco de dados.",
                ex);
        }
    }

    public async Task DeleteAsync(Time time)
    {
        try
        {
            _context.Times.Remove(time);
            await _context.SaveChangesAsync();
        }
        catch (DbUpdateException ex)
        {
            throw new RepositoryException(
                "Falha ao excluir o time no banco de dados.",
                ex);
        }
    }
}