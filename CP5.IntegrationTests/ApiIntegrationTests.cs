using System.Net;
using System.Text.Json;

namespace CP5.IntegrationTests;

public class ApiIntegrationTests
    : IClassFixture<CustomWebApplicationFactory>
{
    private readonly HttpClient _client;

    public ApiIntegrationTests(
        CustomWebApplicationFactory factory)
    {
        _client = factory.CreateClient();
    }

    [Fact]
    public async Task GetTimes_DeveRetornarPaginaComTimes()
    {
        var response = await _client.GetAsync(
            "/api/Time?pageNumber=1&pageSize=10");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json =
            await response.Content.ReadAsStringAsync();

        using var document =
            JsonDocument.Parse(json);

        var root = document.RootElement;

        Assert.Equal(
            1,
            root.GetProperty("pageNumber").GetInt32());

        Assert.Equal(
            10,
            root.GetProperty("pageSize").GetInt32());

        Assert.Equal(
            1,
            root.GetProperty("totalItems").GetInt32());

        var items =
            root.GetProperty("items");

        Assert.Single(items.EnumerateArray());

        var time =
            items[0];

        Assert.Equal(
            "FURIA",
            time.GetProperty("nome").GetString());

        Assert.Equal(
            "CS2",
            time.GetProperty("jogo").GetString());
    }

    [Fact]
    public async Task GetTimePorId_DeveRetornar404_QuandoNaoExistir()
    {
        var response =
            await _client.GetAsync(
                "/api/Time/999999");

        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);
    }

    [Fact]
    public async Task HealthCheck_DeveRetornarHealthy()
    {
        var response =
            await _client.GetAsync("/health");

        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var conteudo =
            await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Healthy",
            conteudo);
    }
}