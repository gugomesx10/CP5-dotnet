using CP5.Application.DTOs;
using CP5.Application.DTOs.Responses;
using CP5.Domain.Entities;

namespace CP5.Application.Mappings;

public static class PerfilCompetitivoMapper
{
    public static PerfilCompetitivoResponseDto ToResponseDto(
        PerfilCompetitivo perfil)
    {
        return new PerfilCompetitivoResponseDto
        {
            Id = perfil.Id,
            KDA = perfil.KDA,
            WinRate = perfil.WinRate,
            HorasJogadas = perfil.HorasJogadas,
            JogadorId = perfil.JogadorId,
            Jogador = perfil.Jogador == null
                ? null
                : JogadorMapper.ToResumoDto(perfil.Jogador)
        };
    }

    public static PerfilCompetitivoResponseDto ToResponseDto(
        PerfilCompetitivo perfil,
        Jogador jogador)
    {
        return new PerfilCompetitivoResponseDto
        {
            Id = perfil.Id,
            KDA = perfil.KDA,
            WinRate = perfil.WinRate,
            HorasJogadas = perfil.HorasJogadas,
            JogadorId = perfil.JogadorId,
            Jogador = JogadorMapper.ToResumoDto(jogador)
        };
    }

    public static PerfilCompetitivo ToEntity(
        PerfilCompetitivoCreateDto dto)
    {
        return new PerfilCompetitivo
        {
            KDA = dto.KDA,
            WinRate = dto.WinRate,
            HorasJogadas = dto.HorasJogadas,
            JogadorId = dto.JogadorId
        };
    }

    public static void UpdateEntity(
        PerfilCompetitivo perfil,
        PerfilCompetitivoCreateDto dto)
    {
        perfil.KDA = dto.KDA;
        perfil.WinRate = dto.WinRate;
        perfil.HorasJogadas = dto.HorasJogadas;
        perfil.JogadorId = dto.JogadorId;
    }
}