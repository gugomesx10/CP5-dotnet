using CP5.Domain.Entities;
using Microsoft.EntityFrameworkCore;

namespace CP5.Infrastructure.Data;

public class ApplicationContext : DbContext
{
    public ApplicationContext(DbContextOptions<ApplicationContext> options)
        : base(options)
    {
    }

    public DbSet<Time> Times { get; set; }
    public DbSet<Jogador> Jogadores { get; set; }
    public DbSet<PerfilCompetitivo> PerfisCompetitivos { get; set; }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Time>()
            .HasMany(t => t.Jogadores)
            .WithOne(j => j.Time)
            .HasForeignKey(j => j.TimeId);

        modelBuilder.Entity<Jogador>()
            .HasOne(j => j.PerfilCompetitivo)
            .WithOne(p => p.Jogador)
            .HasForeignKey<PerfilCompetitivo>(p => p.JogadorId);

        modelBuilder.Entity<Time>()
            .HasIndex(t => t.Jogo);

        modelBuilder.Entity<Jogador>()
            .HasIndex(j => j.Funcao);
    }
}