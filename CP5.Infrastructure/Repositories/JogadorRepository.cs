using CP5.Application.Interfaces.Repositories;
using CP5.Domain.Entities;
using CP5.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using CP5.Application.Common;

namespace CP5.Infrastructure.Repositories;

public class JogadorRepository : IJogadorRepository
{
    private readonly ApplicationContext _context;

    public JogadorRepository(ApplicationContext context)
    {
        _context = context;
    }

    public async Task<PagedResult<Jogador>> GetAllAsync(
        int pageNumber,
        int pageSize)
    {
        var query = _context.Jogadores
            .AsNoTracking()
            .Include(j => j.Time);

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(j => j.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return CriarResultado(
            items,
            pageNumber,
            pageSize,
            totalItems);
    }

    public async Task<Jogador?> GetByIdAsync(int id)
    {
        return await _context.Jogadores
            .AsNoTracking()
            .Include(j => j.Time)
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<Jogador?> GetByIdForUpdateAsync(int id)
    {
        return await _context.Jogadores
            .FirstOrDefaultAsync(j => j.Id == id);
    }

    public async Task<PagedResult<Jogador>> GetByTimeAsync(
        int timeId,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Jogadores
            .AsNoTracking()
            .Include(j => j.Time)
            .Where(j => j.TimeId == timeId);

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(j => j.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return CriarResultado(
            items,
            pageNumber,
            pageSize,
            totalItems);
    }

    public async Task<PagedResult<Jogador>> GetByFuncaoAsync(
        string funcao,
        int pageNumber,
        int pageSize)
    {
        var query = _context.Jogadores
            .AsNoTracking()
            .Include(j => j.Time)
            .Where(j => j.Funcao.ToLower() == funcao.ToLower());

        var totalItems = await query.CountAsync();

        var items = await query
            .OrderBy(j => j.Id)
            .Skip((pageNumber - 1) * pageSize)
            .Take(pageSize)
            .ToListAsync();

        return CriarResultado(
            items,
            pageNumber,
            pageSize,
            totalItems);
    }
    
    private static PagedResult<Jogador> CriarResultado(
        List<Jogador> items,
        int pageNumber,
        int pageSize,
        int totalItems)
    {
        return new PagedResult<Jogador>
        {
            Items = items,
            PageNumber = pageNumber,
            PageSize = pageSize,
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize)
        };
    }

    public async Task AddAsync(Jogador jogador)
    {
        await _context.Jogadores.AddAsync(jogador);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(Jogador jogador)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(Jogador jogador)
    {
        _context.Jogadores.Remove(jogador);
        await _context.SaveChangesAsync();
    }
}