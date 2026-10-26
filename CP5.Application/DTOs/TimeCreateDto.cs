using System.ComponentModel.DataAnnotations;

namespace CP5.Application.DTOs;

public class TimeCreateDto
{
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
}