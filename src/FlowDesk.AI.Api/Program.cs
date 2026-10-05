using DotNetEnv;
using FlowDesk.AI.Api.Endpoints;
using FlowDesk.AI.Infrastructure;

Env.TraversePath().Load();

var builder = WebApplication.CreateBuilder(args);

builder.Services.AddFlowDeskInfrastructure(builder.Configuration);

var app = builder.Build();

app.MapGet("/health", () => Results.Ok(new
{
    service = "FlowDesk AI",
    status = "healthy"
}));

app.MapAiEndpoints();

app.Run();
