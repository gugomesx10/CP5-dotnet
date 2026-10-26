using System.Net;
using System.Text;
using CP5.Application.Results;
using CP5.Infrastructure.ExternalServices;
using Microsoft.Extensions.Logging.Abstractions;

namespace CP5.UnitTests.ExternalServices;

public class OpenDotaServiceTests
{
    private static OpenDotaService CreateService(
        Func<HttpResponseMessage> responseFactory)
    {
        var client = new HttpClient(
            new FakeHttpHandler(responseFactory))
        {
            BaseAddress = new Uri(
                "https://api.opendota.com/api/")
        };

        return new OpenDotaService(
            client,
            NullLogger<OpenDotaService>.Instance);
    }

    [Fact]
    public async Task GetTeam_DeveRetornarSucesso()
    {
        var service = CreateService(() => new HttpResponseMessage(
            HttpStatusCode.OK)
        {
            Content = new StringContent(
                """
                {
                  "team_id": 123,
                  "name": "Team Test",
                  "tag": "TEST",
                  "rating": 1500,
                  "wins": 10,
                  "losses": 3
                }
                """,
                Encoding.UTF8,
                "application/json")
        });

        var result = await service.GetTeamAsync(123);

        Assert.True(result.IsSuccess);
        Assert.Equal("Team Test", result.Data!.Name);
        Assert.Equal(123, result.Data.TeamId);
        Assert.Equal(10, result.Data.Wins);
    }

    [Fact]
    public async Task GetTeam_DeveRetornarNotFound()
    {
        var service = CreateService(() =>
            new HttpResponseMessage(HttpStatusCode.NotFound));

        var result = await service.GetTeamAsync(999);

        Assert.False(result.IsSuccess);
        Assert.Equal(OperationStatus.NotFound, result.Status);
        Assert.Equal(
            "EXTERNAL_TEAM_NOT_FOUND",
            result.ErrorCode);
    }

    [Fact]
    public async Task GetTeam_DeveTratarIndisponibilidade()
    {
        var service = CreateService(() =>
            new HttpResponseMessage(
                HttpStatusCode.ServiceUnavailable));

        var result = await service.GetTeamAsync(123);

        Assert.False(result.IsSuccess);
        Assert.Equal(
            OperationStatus.ExternalServiceFailure,
            result.Status);
        Assert.Equal(
            "EXTERNAL_SERVICE_ERROR",
            result.ErrorCode);
    }

    [Fact]
    public async Task GetTeam_DeveValidarId()
    {
        var service = CreateService(() =>
            throw new InvalidOperationException(
                "Não deveria fazer requisição HTTP."));

        var result = await service.GetTeamAsync(0);

        Assert.Equal(
            OperationStatus.ValidationError,
            result.Status);
        Assert.Equal("INVALID_TEAM_ID", result.ErrorCode);
    }

    private sealed class FakeHttpHandler : HttpMessageHandler
    {
        private readonly Func<HttpResponseMessage> _responseFactory;

        public FakeHttpHandler(
            Func<HttpResponseMessage> responseFactory)
        {
            _responseFactory = responseFactory;
        }

        protected override Task<HttpResponseMessage> SendAsync(
            HttpRequestMessage request,
            CancellationToken cancellationToken)
        {
            return Task.FromResult(_responseFactory());
        }
    }
}