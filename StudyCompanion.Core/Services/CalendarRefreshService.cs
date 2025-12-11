using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using StudyCompanion.Core.Data;
using StudyCompanion.Core.Jobs;
using StudyCompanion.Shared.Models;
using StudyCompanion.Shared.Options;
using TickerQ.Utilities;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace StudyCompanion.Core.Services;

public class CalendarRefreshService(
    IDbContextFactory<PostgresDbContext> dbFactory, 
    IOptions<UserOptions> options,
    ILogger<CalendarRefreshService> logger,
    ITimeTickerManager<TimeTickerEntity> ticker
) : BackgroundService
{
    protected override async Task ExecuteAsync(CancellationToken token)
    {
        using PeriodicTimer timer = new(TimeSpan.FromMinutes(options.Value.CalendarCheckMinutes));
        
        TimeSpan eventOffset = TimeSpan.FromMinutes(options.Value.CalendarEventOffsetMinutes);

        bool first = true;

        while (first || (!token.IsCancellationRequested && await timer.WaitForNextTickAsync(token)))
        {
            first = false;
            
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
                        
                        if (Calendar.Load(calender.Data) is not Calendar ical)
                            continue;

                        DateTime calStart = DateTime.UtcNow;
                        DateTime calEnd = calStart.AddDays(3);
                        
                        List<CalendarEvent> events = ical.Events
                            .Where(e => e.Start?.AsUtc >= calStart && e.End?.AsUtc <= calEnd)
                            .ToList();
                        
                        List<TimeTickerEntity> tickers = await db.Set<TimeTickerEntity>()
                            .Where(t => t.Description.Contains($"Calender={calender.Id};"))
                            .ToListAsync(token);
                        
                        foreach (CalendarEvent calEvent in events)
                        {
                            TimeTickerEntity? entity = tickers.FirstOrDefault(t => t.Description.Contains($"Event={calEvent.Uid};"));

                            if (entity == null)
                            {
                                await ticker.AddAsync(new TimeTickerEntity()
                                {
                                    Function = nameof(EventJob.RemindEvent),
                                    Description = $"Calender={calender.Id};Event={calEvent.Uid};",
                                    ExecutionTime = calEvent.Start!.AsUtc - eventOffset,
                                    Request = TickerHelper.CreateTickerRequest(new EventJobData(calender.Id, calEvent.Uid)),
                                }, token);
                            }
                            else
                            {
                                entity.ExecutionTime =  calEvent.Start!.AsUtc - eventOffset;
                            
                                await ticker.UpdateAsync(entity, token);
                            }
                        }
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