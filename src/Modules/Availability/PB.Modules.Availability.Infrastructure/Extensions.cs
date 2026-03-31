using Microsoft.Extensions.DependencyInjection;
using PB.Modules.Availability.Application.Services;
using PB.Modules.Availability.Domain.Ports;
using PB.Modules.Availability.Infrastructure.Repositories;

namespace PB.Modules.Availability.Infrastructure;

public static class Extensions
{
    public static IServiceCollection AddAvailabilityModule(this IServiceCollection services)
    {
        services.AddSingleton<ITicketPoolRepository, InMemoryTicketPoolRepository>();
        services.AddScoped<IAvailabilityService, AvailabilityService>();
        return services;
    }
}
