using System.ComponentModel.DataAnnotations;

namespace CP5.Application.DTOs;

public class PerfilCompetitivoCreateDto
{
    [Range(0, 50)]
    public double KDA { get; set; }

    [Range(0, 100)]
    public double WinRate { get; set; }

    [Range(0, 50000)]
    public int HorasJogadas { get; set; }

    [Required]
    public int JogadorId { get; set; }
}