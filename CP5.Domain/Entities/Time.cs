using System.ComponentModel.DataAnnotations;

namespace CP5.Domain.Entities;

public class Time
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(100)]
    public string Nome { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(80)]
    public string Jogo { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(80)]
    public string Pais { get; set; } = string.Empty;
    
    [Range(1, 9999)]
    public int Ranking { get; set; }
    
    public ICollection<Jogador> Jogadores { get; set; } = new List<Jogador>();
    
}