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
    IWebHostEnvironment environment,
    CancellationToken cancellationToken) =>
{
    try
    {
        await dbContext.Database.OpenConnectionAsync(cancellationToken);
        await dbContext.Database.CloseConnectionAsync();

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

app.MapAiEndpoints();

app.Run();
