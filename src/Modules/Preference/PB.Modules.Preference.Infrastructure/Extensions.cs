namespace PB.Modules.Preference.Infrastructure;

using Microsoft.Extensions.DependencyInjection;
using PB.Modules.Preference.Application.Services;
using PB.Modules.Preference.Domain.Repositories;
using PB.Modules.Preference.Infrastructure.Repositories;

public static class Extensions
{
    public static IServiceCollection AddPreferenceModule(this IServiceCollection services)
    {
        services.AddSingleton<IUserPreferenceRepository, InMemoryUserPreferenceRepository>();
        services.AddScoped<IPreferenceService, PreferenceService>();
        return services;
    }
}
