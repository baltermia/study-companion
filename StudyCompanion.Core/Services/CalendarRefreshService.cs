using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyCompanion.Data;
using StudyCompanion.Shared.Models;
using StudyCompanion.Shared.Options;

namespace StudyCompanion.Core.Services;

public class CalendarRefreshService(
    IDbContextFactory<PostgresDbContext> dbFactory, 
    IOptions<UserOptions> options,
    ILogger<CalendarRefreshService> logger
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(options.Value.CalendarCheckMinutes));

        while (!token.IsCancellationRequested && await timer.WaitForNextTickAsync(token))
        {
            try
            {
                await using PostgresDbContext db = await dbFactory.CreateDbContextAsync(token);

                DateTime threshold = DateTime.UtcNow.AddHours(-options.Value.CalendarRefreshHours);

                List<Calender> overdue = await db.Set<Calender>()
                    .Where(c => c.LastRefresh <= threshold)
                    .ToListAsync(token);

                foreach (Calender calender in overdue)
                {
                    using HttpClient client = new() ;
                    
                    try
                    {
                        calender.Data = await client.GetStringAsync(calender.Link, token);
                        calender.LastRefresh = DateTime.UtcNow;

                        db.Update(calender);
                    }
                    catch (HttpRequestException ex)
                    {
                        logger.LogError(ex, "Error fetching calendar data");
                    }
                }

                await db.SaveChangesAsync();
            }
            catch (Exception ex)
            {
                logger.LogError(ex, "Error in Calendar Refresh Service");
            }
        }
    }
}