using System.Globalization;
using Ical.Net.CalendarComponents;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StudyCompanion.Core.Builders;
using StudyCompanion.Core.Data;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Telegram.Bot;
using TickerQ.Utilities.Base;
using Calendar = Ical.Net.Calendar;

namespace StudyCompanion.Core.Jobs;

public record EventJobData(int CalendarId, string EventId);

public class EventJob(PostgresDbContext db, ITelegramBotClient bot, IDistributedCache cache)
{
    [TickerFunction(nameof(RemindEvent))]
    public async Task RemindEvent(TickerFunctionContext<EventJobData> context, CancellationToken token)
    {
        EventJobData stored = context.Request;
            
        User? user = await db.Set<User>()
            .Include(u => u.Settings)
                .ThenInclude(s => s.Calender)
            .FirstOrDefaultAsync(u => u.Settings.CalenderId == stored.CalendarId, token);
        
        if (user == null)
            return;

        if (Calendar.Load(user.Settings.Calender!.Data) is not Calendar ical)
            return;

        if (ical.Events.FirstOrDefault(e => e.Uid == stored.EventId) is not CalendarEvent ev)
            return;
        
        Language lang = user.Settings.Language;
        CultureInfo culture = lang.ToCultureInfo();
        TimeZoneInfo tz = TimeZoneInfo.FindSystemTimeZoneById(user.Settings.TimeZone.Id);
        
        string text = lang.GetLocalized(
            en => $"⏰ Upcoming Lecture starting {ev.Start?.ToTimeZone(tz.Id).Time?.ToString(culture)}".Newline().Newline() + $"{ev.Summary.Bold()} starting at {ev.Start.ToTimeZone(user.Settings.TimeZone.Id).ToString("f", user.Settings.Language.ToCultureInfo())}.",
            de => $"⏰ Erinnerung: Du hast eine bevorstehendes Lektion: {ev.Summary.Bold()} das um {ev.Start.ToTimeZone(user.Settings.TimeZone.Id).ToString("f", user.Settings.Language.ToCultureInfo())} beginnt."
        );
        
        await text
            .AsMarkup()
            .ExecuteAsync(user.TelegramUser.Id, bot, cache);
    }
}