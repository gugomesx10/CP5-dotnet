using System.Text.Json.Serialization;

namespace CP5.Application.DTOs.Responses;

public sealed class OpenDotaTeamDto
{
    [JsonPropertyName("team_id")]
    public int TeamId { get; set; }

    [JsonPropertyName("name")]
    public string? Name { get; set; }

    [JsonPropertyName("tag")]
    public string? Tag { get; set; }

    [JsonPropertyName("rating")]
    public double? Rating { get; set; }

    [JsonPropertyName("wins")]
    public int? Wins { get; set; }

    [JsonPropertyName("losses")]
    public int? Losses { get; set; }
}