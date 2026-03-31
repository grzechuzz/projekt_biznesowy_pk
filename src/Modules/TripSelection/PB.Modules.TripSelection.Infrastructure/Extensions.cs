using Microsoft.Extensions.DependencyInjection;
using PB.Modules.TripSelection.Application.Ports;
using PB.Modules.TripSelection.Application.Services;
using PB.Modules.TripSelection.Domain.Ports;
using PB.Modules.TripSelection.Domain.Services;
using PB.Modules.TripSelection.Infrastructure.CrossModule;
using PB.Modules.TripSelection.Infrastructure.Repositories;

namespace PB.Modules.TripSelection.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddTripSelectionModule(this IServiceCollection services)
    {
        services.AddSingleton<IAttractionRelationRepository, InMemoryAttractionRelationRepository>();
        services.AddSingleton<ISelectionSessionRepository, InMemorySelectionSessionRepository>();
        services.AddScoped<IRelationValidationService, RelationValidationService>();
        services.AddScoped<ISelectionSessionService, SelectionSessionService>();
        services.AddScoped<IAttractionRelationService, AttractionRelationService>();
        services.AddScoped<ICatalogEntryQuery, CatalogEntryQueryAdapter>();
        services.AddScoped<IAvailabilityQuery, AvailabilityQueryAdapter>();
        return services;
    }
}
