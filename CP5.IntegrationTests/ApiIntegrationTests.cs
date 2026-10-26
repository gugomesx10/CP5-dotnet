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
        // Arrange & Act
        var response = await _client.GetAsync(
            "/api/Time?pageNumber=1&pageSize=10");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;

        // Validação do OperationResult
        Assert.True(
            root.GetProperty("isSuccess").GetBoolean());

        Assert.Equal(
            0,
            root.GetProperty("status").GetInt32());

        Assert.Equal(
            JsonValueKind.Null,
            root.GetProperty("errorCode").ValueKind);

        // Os dados paginados estão dentro de "data"
        var data = root.GetProperty("data");

        Assert.Equal(
            1,
            data.GetProperty("pageNumber").GetInt32());

        Assert.Equal(
            10,
            data.GetProperty("pageSize").GetInt32());

        Assert.Equal(
            1,
            data.GetProperty("totalItems").GetInt32());

        var items = data.GetProperty("items");

        Assert.Single(items.EnumerateArray());

        var time = items[0];

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
        // Arrange & Act
        var response = await _client.GetAsync(
            "/api/Time/999999");

        // Assert
        Assert.Equal(
            HttpStatusCode.NotFound,
            response.StatusCode);

        var json = await response.Content.ReadAsStringAsync();

        using var document = JsonDocument.Parse(json);

        var root = document.RootElement;

        Assert.False(
            root.GetProperty("isSuccess").GetBoolean());

        Assert.Equal(
            "TIME_NOT_FOUND",
            root.GetProperty("errorCode").GetString());
    }

    [Fact]
    public async Task HealthCheck_DeveRetornarHealthy()
    {
        // Arrange & Act
        var response = await _client.GetAsync("/health");

        // Assert
        Assert.Equal(
            HttpStatusCode.OK,
            response.StatusCode);

        var conteudo = await response.Content.ReadAsStringAsync();

        Assert.Contains(
            "Healthy",
            conteudo);
    }
}