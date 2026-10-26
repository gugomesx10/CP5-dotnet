using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Domain.Entities;

namespace CP5.Application.Mappings;

public static class JogadorMapper
{
    public static JogadorResumoDto ToResumoDto(Jogador jogador)
    {
        return new JogadorResumoDto
        {
            Id = jogador.Id,
            Nickname = jogador.Nickname,
            Funcao = jogador.Funcao,
            Idade = jogador.Idade,
            Time = jogador.Time == null
                ? null
                : TimeMapper.ToResumoDto(jogador.Time)
        };
    }

    public static JogadorResumoDto ToResumoDto(Jogador jogador, Time time)
    {
        return new JogadorResumoDto
        {
            Id = jogador.Id,
            Nickname = jogador.Nickname,
            Funcao = jogador.Funcao,
            Idade = jogador.Idade,
            Time = TimeMapper.ToResumoDto(time)
        };
    }

    public static Jogador ToEntity(JogadorCreateDto dto)
    {
        return new Jogador
        {
            Nickname = dto.Nickname,
            Funcao = dto.Funcao,
            Idade = dto.Idade,
            TimeId = dto.TimeId
        };
    }

    public static void UpdateEntity(Jogador jogador, JogadorCreateDto dto)
    {
        jogador.Nickname = dto.Nickname;
        jogador.Funcao = dto.Funcao;
        jogador.Idade = dto.Idade;
        jogador.TimeId = dto.TimeId;
    }
}