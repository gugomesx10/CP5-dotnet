using System.Reflection;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading.RateLimiting;
using CP5.Application.Interfaces.Repositories;
using CP5.Application.Interfaces.Services;
using CP5.Application.Services;
using CP5.Infrastructure.Data;
using CP5.Infrastructure.Repositories;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.AspNetCore.RateLimiting;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Diagnostics.HealthChecks;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

var applicationInsightsConnectionString =
    Environment.GetEnvironmentVariable(
        "APPLICATIONINSIGHTS_CONNECTION_STRING")
    ?? builder.Configuration[
        "ApplicationInsights:ConnectionString"];

if (!builder.Environment.IsEnvironment("Testing") &&
    !string.IsNullOrWhiteSpace(
        applicationInsightsConnectionString))
{
    builder.Services.AddApplicationInsightsTelemetry(options =>
    {
        options.ConnectionString =
            applicationInsightsConnectionString;
    });
}

builder.Services.AddControllers()
    .AddJsonOptions(options =>
    {
        options.JsonSerializerOptions.ReferenceHandler =
            ReferenceHandler.IgnoreCycles;
    });

builder.Services.AddAuthorization();

builder.Services.AddEndpointsApiExplorer();

builder.Services.AddSwaggerGen(options =>
{
    options.EnableAnnotations();

    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "ArenaSync API",
        Version = "v1",
        Description =
            "API REST para gerenciamento de times, jogadores e perfis competitivos de e-sports."
    });

    var xmlFile =
        $"{Assembly.GetExecutingAssembly().GetName().Name}.xml";

    var xmlPath =
        Path.Combine(
            AppContext.BaseDirectory,
            xmlFile);

    options.IncludeXmlComments(xmlPath);
});

builder.Services.AddDbContext<ApplicationContext>(options =>
    options.UseOracle(
        builder.Configuration.GetConnectionString(
            "OracleConnection")
    )
);

builder.Services.AddHealthChecks()
    .AddCheck(
        "api",
        () =>
            HealthCheckResult.Healthy(
                "API disponível"))
    .AddDbContextCheck<ApplicationContext>(
        name: "oracle-database",
        failureStatus: HealthStatus.Unhealthy,
        tags: new[] { "database" });

builder.Services.AddScoped<
    ITimeRepository,
    TimeRepository>();

builder.Services.AddScoped<
    ITimeService,
    TimeService>();

builder.Services.AddScoped<
    IJogadorRepository,
    JogadorRepository>();

builder.Services.AddScoped<
    IJogadorService,
    JogadorService>();

builder.Services.AddScoped<
    IPerfilCompetitivoRepository,
    PerfilCompetitivoRepository>();

builder.Services.AddScoped<
    IPerfilCompetitivoService,
    PerfilCompetitivoService>();

builder.Services.AddResponseCompression(options =>
{
    options.EnableForHttps = true;
});

builder.Services.AddRateLimiter(options =>
{
    options.RejectionStatusCode =
        StatusCodes.Status429TooManyRequests;

    options.GlobalLimiter =
        PartitionedRateLimiter.Create<HttpContext, string>(
            httpContext =>
            {
                var ip =
                    httpContext.Connection
                        .RemoteIpAddress?
                        .ToString()
                    ?? "unknown";

                return RateLimitPartition
                    .GetFixedWindowLimiter(
                        partitionKey: ip,
                        factory: _ =>
                            new FixedWindowRateLimiterOptions
                            {
                                PermitLimit = 20,
                                Window =
                                    TimeSpan.FromMinutes(1),
                                QueueLimit = 0,
                                AutoReplenishment = true
                            });
            });
});

var app = builder.Build();

if (app.Environment.IsDevelopment())
{
    app.UseSwagger();
    app.UseSwaggerUI();
}

app.UseResponseCompression();

app.UseHttpsRedirection();

app.UseRateLimiter();

app.UseAuthorization();

app.MapControllers();

app.MapHealthChecks(
    "/health",
    new HealthCheckOptions
    {
        ResponseWriter = async (context, report) =>
        {
            context.Response.ContentType =
                "application/json";

            var response = new
            {
                status = report.Status.ToString(),

                checks = report.Entries.Select(
                    entry => new
                    {
                        name = entry.Key,
                        status =
                            entry.Value.Status.ToString(),
                        description =
                            entry.Value.Description,
                        durationMs =
                            entry.Value.Duration
                                .TotalMilliseconds
                    }),

                totalDurationMs =
                    report.TotalDuration
                        .TotalMilliseconds
            };

            await context.Response.WriteAsync(
                JsonSerializer.Serialize(response));
        }
    });

app.MapGet("/", () => Results.Redirect("/swagger")).ExcludeFromDescription();

app.Run();

public partial class Program
{
}