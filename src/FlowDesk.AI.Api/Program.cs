using DotNetEnv;
using FlowDesk.AI.Api.Endpoints;
using FlowDesk.AI.Infrastructure;
using FlowDesk.AI.Infrastructure.Persistence;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFlowDeskInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    service = "FlowDesk AI",
    status = "healthy"
}));

app.MapGet("/health/database", async (
    FlowDeskDbContext dbContext,
    CancellationToken cancellationToken) =>
{
    var canConnect = await dbContext.Database.CanConnectAsync(cancellationToken);

    return canConnect
        ? Results.Ok(new
        {
            service = "FlowDesk AI",
            database = "healthy"
        })
        : Results.StatusCode(StatusCodes.Status503ServiceUnavailable);
});

app.MapAiEndpoints();

app.Run();
