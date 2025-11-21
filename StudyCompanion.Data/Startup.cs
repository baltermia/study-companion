using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using StudyCompanion.Shared.Services;

namespace StudyCompanion.Data;

public static class Startup
{
    public static IServiceCollection AddData(this IServiceCollection services, string connectionString) => services
        //.AddDbContextPool<PostgresDbContext>(options => options.UseNpgsql(connectionString))
        //.AddPooledDbContextFactory<PostgresDbContext>(options => options.UseNpgsql(connectionString))
        .AddDbContextPool<PostgresDbContext>(options => options.UseInMemoryDatabase("StudyCompanion"))
        .AddPooledDbContextFactory<PostgresDbContext>(options => options.UseInMemoryDatabase("StudyCompanion"))
        .AddScoped<IHelper, HelperService<PostgresDbContext>>();
}
