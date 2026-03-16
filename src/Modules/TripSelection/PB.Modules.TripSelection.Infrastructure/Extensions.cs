namespace PB.Modules.TripSelection.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using PB.Modules.TripSelection.Application.Services;
using PB.Modules.TripSelection.Domain.Repositories;
using PB.Modules.TripSelection.Domain.Services;
using PB.Modules.TripSelection.Infrastructure.Repositories;

public static class Extensions
{
    public static IServiceCollection AddTripSelectionModule(this IServiceCollection services)
    {
        services.AddSingleton<ITripSelectionResultRepository, InMemoryTripSelectionResultRepository>();
        services.AddSingleton<ISelectionStrategy, DefaultSelectionStrategy>();
        services.AddScoped<ITripSelectionService, TripSelectionService>();
        return services;
    }
}
