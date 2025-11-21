using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;

namespace StudyCompanion.Data;

public static class Startup
{
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString) => services
        .AddDbContext<PostgresDbContext>(options => options.UseNpgsql(connectionString))
        .AddDbContextFactory<PostgresDbContext>();
}
