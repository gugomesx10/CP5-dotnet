using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CP5.Domain.Entities;

public class PerfilCompetitivo
{
    [Key]
    public int Id { get; set; }
    
    [Range(0, 50)]
    public double KDA { get; set; }
    
    [Range(0, 100)]
    public double WinRate { get; set; }
    
    [Range(0, 50000)]
    public int HorasJogadas { get; set; }
    
    [ForeignKey("Jogador")]
    public int JogadorId { get; set; }
    
    public Jogador? Jogador { get; set; }
}