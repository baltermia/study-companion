using System.Globalization;
using Ical.Net;
using Ical.Net.CalendarComponents;
using Ical.Net.DataTypes;
using Microsoft.Extensions.Options;
using MinimalTelegramBot;
using MinimalTelegramBot.Builder;
using MinimalTelegramBot.Handling;
using MinimalTelegramBot.StateMachine.Abstractions;
using MinimalTelegramBot.StateMachine.Extensions;
using StudyCompanion.Core.Builders;
using StudyCompanion.Core.Contracts;
using StudyCompanion.Core.Extensions;
using StudyCompanion.Core.Shared.Filters;
using StudyCompanion.Core.Data;
using StudyCompanion.Shared.Contracts;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using StudyCompanion.Shared.Options;
using IResult = MinimalTelegramBot.Results.IResult;
using Results = MinimalTelegramBot.Results.Results;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using Calendar = Ical.Net.Calendar;

namespace StudyCompanion.Core.Commands;

internal class Start : IBotCommand
{
    public static List<CommandDescription> Commands { get; } =
    [
        new("/start", "🏠 Bot Start Dialog", CommandChat.Private),
    ];

    public static ReplyKeyboardMarkup GetButtons(Language lang, Role? role)
    {
        List<KeyboardButton> row1 =
        [
            new(lang.GetLocalized(en =>"🏠 Home", de => "🏠 Home" )),
            new(lang.GetLocalized(en =>"⚙️ Settings", de => "⚙️ Einstellungen" )),
        ];

        List<KeyboardButton> row2 = 
        [
            new(lang.GetLocalized(en => "📅 Weekly Calendar", de => "📅 Wöchentlicher Kalender")),
        ];

        if (role is Role.Mod or Role.Admin)
            row2.Add(new KeyboardButton("🛡️ Admin"));

        return new()
        {
            Keyboard = [row1, row2],
            ResizeKeyboard = true,
            IsPersistent = true,
        };
    }

    public static void ConfigureCommands(BotApplication bot)
    {
        bot.HandleCommand("/start", OnStart)
            .FilterChatType(ChatType.Private);

        bot.HandleMessageText("🏠 Home", OnStart)
            .FilterChatType(ChatType.Private);
        
        bot.HandleUpdateType(UpdateType.Message, OnCalender)
            .FilterState<SetCalendarState.Setting>();
    }
    
    [StateGroup(nameof(SetCalendarState))]
    public static class SetCalendarState
    {
        [State(1)]
        public class Setting;
    }

    private static async Task<IResult> OnStart(BotRequestContext context, IHelper helper, IOptions<UserOptions> options)
    {
        await context.DropPrevious();

        User? user = null;

        // ensures that the account gets created
        if (context.Update.Message?.ConvertMessage() is Message msg && msg.Chat is TelegramUser telegramUser)
        {
            user = await helper.GetUserAsync(telegramUser, withCalendar: true);
        }

        Language lang = user?.Settings.Language ?? Language.English;
        CultureInfo culture = lang.ToCultureInfo();

        string text = lang.GetLocalized(
            en => "Welcome to your Study Companion!".Bold().Newline() ,
            de => "Willkommen, ich bin dein Study Companion!".Bold().Newline()
        );

        if (user?.Settings.Calender is not Calender cal)
        {
            // ical is needed
            
            await context.SetState(new SetCalendarState.Setting());
            
            text += lang.GetLocalized(
                en => """
                      First off I need your iCal Calender. Simply respond with the link.

                      Change Language / Sprache ändern: /settings
                      """,
                de => """
                      Erstmals brauche ich deinen iCal Kalender. Antworte dazu einfach mit dem Link.

                      Sprache ändern / Change language: /settings
                      """
            );
        }
        
        return 
            text.AsMarkup()
                .WithButtons(GetButtons(lang, user?.Role))
                .Delete();
    }

    private static async Task<IResult> OnCalender(BotRequestContext context, PostgresDbContext db, IHelper helper)
    {
        if (context.Update.Message?.ConvertMessage() is not Message msg || msg.Chat is not TelegramUser telegramUser)
            return Results.Empty;

        if (context.MessageText is not string link)
            return Results.Empty;

        if (!IsValidHttpUrl(link))
            return "Please provide a valid url.".Delete();

        User user = await helper.GetUserAsync(telegramUser);

        using HttpClient client = new() ;
        try
        {
            string data = await client.GetStringAsync(link);
            
            if (Calendar.Load(data) is not Calendar ical)
                return "The provided link did not lead to a valid iCalendar.".Delete();
            
            user.Settings.Calender = new Calender()
            {
                Data = data,
                Link = link,
                LastRefresh = DateTime.UtcNow,
            };

            db.Update(user);
                
            await db.SaveChangesAsync();
                
            await context.DropPrevious();

            return "Calendar saved. Fetch with /start".Delete();
        }
        catch
        {
            return "Error fetching Calendar. Please try again.".Delete();
        }
    }
    private static bool IsValidHttpUrl(string text)
    {
        if (string.IsNullOrWhiteSpace(text)) return false;
        if (!Uri.TryCreate(text, UriKind.Absolute, out var uri)) return false;
        return uri.Scheme == Uri.UriSchemeHttp || uri.Scheme == Uri.UriSchemeHttps;
    }
    private static DateOnly GetEndOfWeek(DateOnly date)
    {
        int daysUntilSunday = ((int)DayOfWeek.Sunday - (int)date.DayOfWeek + 7) % 7;
        return date.AddDays(daysUntilSunday);
    }
}