using Microsoft.Extensions.DependencyInjection;
using PB.Modules.Catalog.Application.Services;
using PB.Modules.Catalog.Domain.Ports;
using PB.Modules.Catalog.Infrastructure.Repositories;

namespace PB.Modules.Catalog.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddCatalogModule(this IServiceCollection services)
    {
        services.AddSingleton<ICatalogEntryRepository, InMemoryCatalogEntryRepository>();
        services.AddScoped<ICatalogService, CatalogService>();
        return services;
    }
}
