namespace PB.Modules.AttractionDefinition.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using PB.Modules.AttractionDefinition.Application.Services;
using PB.Modules.AttractionDefinition.Domain.Repositories;
using PB.Modules.AttractionDefinition.Infrastructure.Repositories;

public static class Extensions
{
    public static IServiceCollection AddAttractionDefinitionModule(this IServiceCollection services)
    {
        services.AddSingleton<IAttractionComponentRepository, InMemoryAttractionComponentRepository>();
        services.AddScoped<IAttractionDefinitionService, AttractionDefinitionService>();
        return services;
    }
}
