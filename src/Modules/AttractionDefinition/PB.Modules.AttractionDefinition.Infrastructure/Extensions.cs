using Microsoft.Extensions.DependencyInjection;
using PB.Modules.AttractionDefinition.Application.Services;
using PB.Modules.AttractionDefinition.Domain.Ports;
using PB.Modules.AttractionDefinition.Infrastructure.Repositories;

namespace PB.Modules.AttractionDefinition.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddAttractionDefinitionModule(this IServiceCollection services)
    {
        services.AddSingleton<IAttractionComponentRepository, InMemoryAttractionComponentRepository>();
        services.AddScoped<IAttractionComponentService, AttractionComponentService>();
        return services;
    }
}
