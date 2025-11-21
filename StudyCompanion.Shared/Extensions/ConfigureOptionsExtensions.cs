using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;

namespace StudyCompanion.Shared.Extensions;


public static class ConfigureOptionsExtensions
{
    public static IServiceCollection ConfigureOptions<TOptions>(this IServiceCollection services, IConfiguration configuration)
        where TOptions : class =>
        services.Configure<TOptions>(configuration.GetRequiredSection(typeof(TOptions).Name));
}

