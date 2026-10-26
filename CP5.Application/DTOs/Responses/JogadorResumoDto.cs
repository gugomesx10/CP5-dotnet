using System.ComponentModel.DataAnnotations;
using CP5.Application.Common;

namespace CP5.Application.DTOs.Responses;

public class JogadorResumoDto
{
    [Required]
    public int Id { get; set; }

    [Required]
    [MaxLength(80)]
    public string Nickname { get; set; } = string.Empty;

    [Required]
    [MaxLength(50)]
    public string Funcao { get; set; } = string.Empty;

    [Range(12, 60)]
    public int Idade { get; set; }

    public TimeResumoDto? Time { get; set; }
    
    public List<HateoasLink> Links { get; set; } = new();
}