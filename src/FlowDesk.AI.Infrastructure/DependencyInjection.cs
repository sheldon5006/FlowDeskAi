using FlowDesk.AI.Application.Abstractions.AI;
using FlowDesk.AI.Application.Abstractions.Knowledge;
using FlowDesk.AI.Infrastructure.AI;
using FlowDesk.AI.Infrastructure.Knowledge;
using FlowDesk.AI.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Npgsql;

namespace FlowDesk.AI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFlowDeskInfrastructure(
        this IServiceCollection services,
        IConfiguration configuration)
    {
        services.AddScoped<IFlowDeskAiService, FlowDeskAiService>();
        services.AddScoped<IKnowledgeService, KnowledgeService>();

        services.AddDbContext<FlowDeskDbContext>((_, options) =>
        {
            var connectionString = BuildPostgresConnectionString(configuration);

            options.UseNpgsql(connectionString, npgsql =>
            {
                npgsql.UseVector();
            });
        });

        return services;
    }

    private static string BuildPostgresConnectionString(IConfiguration configuration)
    {
        var portValue = configuration["POSTGRES_PORT"] ?? "5432";

        if (!int.TryParse(portValue, out var port))
        {
            throw new InvalidOperationException("POSTGRES_PORT must be a valid integer.");
        }

        var password = configuration["POSTGRES_PASSWORD"];

        if (string.IsNullOrWhiteSpace(password))
        {
            throw new InvalidOperationException(
                "POSTGRES_PASSWORD is missing. Add it to the local .env file.");
        }

        var builder = new NpgsqlConnectionStringBuilder
        {
            Host = configuration["POSTGRES_HOST"] ?? "localhost",
            Port = port,
            Database = configuration["POSTGRES_DB"] ?? "flowdesk",
            Username = configuration["POSTGRES_USER"] ?? "flowdesk",
            Password = password
        };

        return builder.ConnectionString;
    }
}
