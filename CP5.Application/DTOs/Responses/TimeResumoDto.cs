using System.ComponentModel.DataAnnotations;
using CP5.Application.Common;

namespace CP5.Application.DTOs.Responses;

public class TimeResumoDto
{
    [Required]
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
    
    public List<HateoasLink> Links { get; set; } = new();
}