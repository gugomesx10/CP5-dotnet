using CP5.Application.Common;
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

    public async Task<PagedResult<PerfilCompetitivo>> GetAllAsync(
        int pageNumber,
        int pageSize)
    {
        var query = _context.PerfisCompetitivos
            .AsNoTracking()
            .Include(p => p.Jogador)
            .ThenInclude(j => j!.Time);

        var totalItems = await query.CountAsync();

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
            TotalItems = totalItems,
            TotalPages = (int)Math.Ceiling(
                totalItems / (double)pageSize)
        };
    }

    public async Task<PerfilCompetitivo?> GetByIdAsync(int id)
    {
        return await _context.PerfisCompetitivos
            .AsNoTracking()
            .Include(p => p.Jogador)
            .ThenInclude(j => j!.Time)
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PerfilCompetitivo?> GetByIdForUpdateAsync(int id)
    {
        return await _context.PerfisCompetitivos
            .FirstOrDefaultAsync(p => p.Id == id);
    }

    public async Task<PerfilCompetitivo?> GetByJogadorAsync(int jogadorId)
    {
        return await _context.PerfisCompetitivos
            .AsNoTracking()
            .Include(p => p.Jogador)
            .ThenInclude(j => j!.Time)
            .FirstOrDefaultAsync(p => p.JogadorId == jogadorId);
    }

    public async Task<bool> ExistsByJogadorAsync(
        int jogadorId,
        int? perfilIdIgnorado = null)
    {
        if (perfilIdIgnorado.HasValue)
        {
            var quantidade = await _context.PerfisCompetitivos
                .CountAsync(p =>
                    p.JogadorId == jogadorId &&
                    p.Id != perfilIdIgnorado.Value);

            return quantidade > 0;
        }

        var total = await _context.PerfisCompetitivos
            .CountAsync(p => p.JogadorId == jogadorId);

        return total > 0;
    }

    public async Task AddAsync(PerfilCompetitivo perfil)
    {
        await _context.PerfisCompetitivos.AddAsync(perfil);
        await _context.SaveChangesAsync();
    }

    public async Task UpdateAsync(PerfilCompetitivo perfil)
    {
        await _context.SaveChangesAsync();
    }

    public async Task DeleteAsync(PerfilCompetitivo perfil)
    {
        _context.PerfisCompetitivos.Remove(perfil);
        await _context.SaveChangesAsync();
    }
}