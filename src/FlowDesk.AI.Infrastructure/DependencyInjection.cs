using FlowDesk.AI.Application.Abstractions.AI;
using FlowDesk.AI.Infrastructure.AI;
using Microsoft.Extensions.DependencyInjection;

namespace FlowDesk.AI.Infrastructure;

public static class DependencyInjection
{
    public static IServiceCollection AddFlowDeskInfrastructure(this IServiceCollection services)
    {
        services.AddScoped<IFlowDeskAiService, FlowDeskAiService>();

        return services;
    }
}
