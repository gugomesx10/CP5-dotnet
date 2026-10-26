using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Domain.Entities;

namespace CP5.Application.Mappings;

public static class TimeMapper
{
    public static TimeResumoDto ToResumoDto(Time time)
    {
        return new TimeResumoDto
        {
            Id = time.Id,
            Nome = time.Nome,
            Jogo = time.Jogo,
            Pais = time.Pais,
            Ranking = time.Ranking
        };
    }

    public static Time ToEntity(TimeCreateDto dto)
    {
        return new Time
        {
            Nome = dto.Nome,
            Jogo = dto.Jogo,
            Pais = dto.Pais,
            Ranking = dto.Ranking
        };
    }

    public static void UpdateEntity(Time time, TimeCreateDto dto)
    {
        time.Nome = dto.Nome;
        time.Jogo = dto.Jogo;
        time.Pais = dto.Pais;
        time.Ranking = dto.Ranking;
    }
}