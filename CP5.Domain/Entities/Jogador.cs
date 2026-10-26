using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace CP5.Domain.Entities;

public class Jogador
{
    [Key]
    public int Id { get; set; }
    
    [Required]
    [MaxLength(80)]
    public string Nickname { get; set; } = string.Empty;
    
    [Required]
    [MaxLength(50)]
    public string Funcao { get; set; } = string.Empty;
    
    [Range(12, 60)]
    public int Idade  { get; set; }
    
    [ForeignKey("Time")]
    public int TimeId { get; set; }
    
    public Time? Time { get; set; }
    
    public PerfilCompetitivo?  PerfilCompetitivo { get; set; }
}