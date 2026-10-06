using DotNetEnv;
using FlowDesk.AI.Api.Endpoints;
using FlowDesk.AI.Infrastructure;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

Env.NoClobber().TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFlowDeskInfrastructure(builder.Configuration);

var app = builder.Build();

using (var scope = app.Services.CreateScope())
{
    var initializer = scope.ServiceProvider.GetRequiredService<DatabaseInitializer>();
    await initializer.InitializeAsync();
}

app.MapGet("/health", () => Results.Ok(new
{
    service = "FlowDesk AI",
    status = "healthy"
}));

app.MapGet("/health/config", (IConfiguration configuration) => Results.Ok(new
{
    postgresHost = configuration["POSTGRES_HOST"] ?? "(missing)",
    postgresPort = configuration["POSTGRES_PORT"] ?? "(missing)",
    postgresDatabase = configuration["POSTGRES_DB"] ?? "(missing)",
    postgresUser = configuration["POSTGRES_USER"] ?? "(missing)",
    passwordSet = !string.IsNullOrWhiteSpace(configuration["POSTGRES_PASSWORD"]),
    passwordLength = configuration["POSTGRES_PASSWORD"]?.Length ?? 0
}));

app.MapGet("/health/database", async (
    FlowDeskDbContext dbContext,
    IWebHostEnvironment environment,
    CancellationToken cancellationToken) =>
{
    try
    {
        var connection = dbContext.Database.GetDbConnection();

        await connection.OpenAsync(cancellationToken);
        await connection.CloseAsync();

        return Results.Ok(new
        {
            service = "FlowDesk AI",
            database = "healthy"
        });
    }
    catch (Exception exception)
    {
        if (environment.IsDevelopment())
        {
            return Results.Problem(
                statusCode: StatusCodes.Status503ServiceUnavailable,
                title: "Database connection failed",
                detail: $"{exception.GetType().Name}: {exception.Message}");
        }

        return Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
    }
});

app.MapBusinessEndpoints();
app.MapAiEndpoints();
app.MapKnowledgeEndpoints();

app.Run();
