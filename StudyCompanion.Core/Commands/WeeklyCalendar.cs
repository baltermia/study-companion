using System.Globalization;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using MinimalTelegramBot;
using MinimalTelegramBot.Builder;
using MinimalTelegramBot.Handling;
using MinimalTelegramBot.StateMachine.Abstractions;
using MinimalTelegramBot.StateMachine.Extensions;
using StudyCompanion.Core.Builders;
using StudyCompanion.Core.Contracts;
using StudyCompanion.Core.Extensions;
using StudyCompanion.Core.Shared.Filters;
using StudyCompanion.Shared.Contracts;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Calendar = Ical.Net.Calendar;
using IResult = MinimalTelegramBot.Results.IResult;
using Results = MinimalTelegramBot.Results.Results;

namespace StudyCompanion.Core.Commands;

public class WeeklyCalendar : IBotCommand
{
    public static List<CommandDescription> Commands { get; } =
    [
        new("/calendar", "📅 Weekly Calendar", CommandChat.Private),
    ];

    private static InlineKeyboardMarkup GetButtons(Language lang) => new()
    {
        InlineKeyboard = 
        [[
            InlineKeyboardButton.WithCallbackData(lang.GetLocalized(en => "⬅️ Previous", de => "⬅️ Vorherig"), "calendar_previous"),
            InlineKeyboardButton.WithCallbackData(lang.GetLocalized(en => "📌 This Week", de => "📌 Diese Woche"), "calendar_this"),
            InlineKeyboardButton.WithCallbackData(lang.GetLocalized(en => "Next ➡️", de =>  "Nächste ➡️"), "calendar_next"),
        ]]
    };
    
    public static void ConfigureCommands(BotApplication bot)
    {
        bot.HandleCommand("/calendar", OnCalendar)
            .FilterChatType(ChatType.Private);

        bot.HandleMessageText("📅 Wöchentlicher Kalender", OnCalendar)
            .FilterChatType(ChatType.Private);

        bot.HandleMessageText("📅 Weekly Calendar", OnCalendar)
            .FilterChatType(ChatType.Private);
    }
    
    public static void ConfigureCallbacks(BotApplication bot)
    {
        bot.HandleCallbackData("calendar_previous", OnPrevious);
        bot.HandleCallbackData("calendar_this", OnCalendar);
        bot.HandleCallbackData("calendar_next", OnNext);
    }
    
    [StateGroup(nameof(CurrentCalendarState))]
    public static class CurrentCalendarState
    {
        [State(1)]
        public class WeekOffset
        {
            public required int Offset { get; set; }
        };
    }

    public static async Task<IResult> OnCalendar(BotRequestContext context, IHelper helper)
    {
        await context.DropPrevious();
        
        if (await helper.GetUserAsync(context.ChatId, true) is not User user)
            return Results.Empty;

        await context.SetState(new CurrentCalendarState.WeekOffset()
        {
            Offset = 0,
        });

        return GetCalendarString(user, 0);
    }

    public static async Task<IResult> OnPrevious(BotRequestContext context, IHelper helper)
    {
        if (await helper.GetUserAsync(context.ChatId, true) is not User user)
            return Results.Empty;

        if (await context.GetState<CurrentCalendarState.WeekOffset>() is not CurrentCalendarState.WeekOffset state)
            return await OnCalendar(context, helper);
        
        await context.DropPrevious();

        state.Offset -= 1;
        
        await context.SetState(state);

        return GetCalendarString(user, state.Offset);
    }
    
    public static async Task<IResult> OnNext(BotRequestContext context, IHelper helper)
    {
        if (await helper.GetUserAsync(context.ChatId, true) is not User user)
            return Results.Empty;

        if (await context.GetState<CurrentCalendarState.WeekOffset>() is not CurrentCalendarState.WeekOffset state)
            return await OnCalendar(context, helper);
        
        await context.DropPrevious();

        state.Offset += 1;
        
        await context.SetState(state);

        return GetCalendarString(user, state.Offset);
    }
    
    private static IResult GetCalendarString(User user, int offset)
    {
        if (user.Settings.Calender == null)
            return "You do not have a calendar configured...".AsMarkup();
        
        if (Calendar.Load(user.Settings.Calender.Data) is not Calendar ical)
            return "Your Calendar seems to be invalid".AsMarkup();
        
        (DateTime start, DateTime end) = GetWeekRange(offset);

        CalDateTime calStart = new(start);
        CalDateTime calEnd = new(end);
            
        List<CalendarEvent> events = ical.Events
            .Where(e => e.GetOccurrences(calStart).TakeWhileBefore(calEnd).Any())
            .ToList();

        IEnumerable<IGrouping<DateOnly, CalendarEvent>> groups = events.GroupBy(ev => ev.Start.Date);

        Language lang = user.Settings.Language;
        CultureInfo culture = lang.ToCultureInfo();

        string range = $"{start.ToString("d", culture)} - {end.ToString("d", culture)}";
        
        string text = lang.GetLocalized(
            en => $"📅 Calender [{range}]",
            de => $"📅 Kalender [{range}]"
        ).Bold().Newline();

        if (events.Count == 0)
            text += lang.GetLocalized(
                en => "You don't have any lectures this week 😄",
                de => "Du hast diese Woche keine Lektionen 😄");

        foreach (IGrouping<DateOnly, CalendarEvent> group in groups)
        {
            text = text.Newline() + $"[{group.Key.ToString("dddd", culture)}]".Bold().Newline();

            foreach (CalendarEvent ev in group)
            {
                TimeSpan? duration = ev.End?.SubtractExact(ev.Start ?? ev.End);
                    
                text += $"{ev.Start?.Time?.ToString(culture)}: {ev.Summary} ({duration?.ToCompactString()}) {ev.Description?.Trim()}".Newline();
            }
        }

        return text
            .WithButtons(GetButtons(lang))
            .Delete()
            .AsMarkup();
    }

    private static (DateTime Start, DateTime End) GetWeekRange(int weekOffset, DateTime? referenceDate = null)
    {
        // Normalize to date portion while preserving Kind if possible
        
        referenceDate ??= DateTime.UtcNow;
        DateTimeKind kind = referenceDate.Value.Kind;
        DateTime dateOnly = referenceDate.Value.Date;
        
        if (dateOnly.Kind != kind)
            dateOnly = DateTime.SpecifyKind(dateOnly, kind);

        // Calculate how many days have passed since Monday
        int daysSinceMonday = ((int)dateOnly.DayOfWeek - (int)DayOfWeek.Monday + 7) % 7;
        DateTime mondayThisWeek = dateOnly.AddDays(-daysSinceMonday);

        // Ensure mondayThisWeek has the same Kind
        if (mondayThisWeek.Kind != kind)
            mondayThisWeek = DateTime.SpecifyKind(mondayThisWeek, kind);

        // Start at Monday 00:00:00 with the preserved kind
        DateTime start = new DateTime(mondayThisWeek.Year, mondayThisWeek.Month, mondayThisWeek.Day, 0, 0, 0, kind)
            .AddDays(weekOffset * 7);

        // End is the last tick of Sunday: next Monday 00:00:00 minus one tick
        DateTime nextMonday = start.AddDays(7);
        DateTime end = nextMonday.AddTicks(-1);

        return (start, end);
    }
}