using Microsoft.EntityFrameworkCore;
using MinimalTelegramBot;
using MinimalTelegramBot.Builder;
using MinimalTelegramBot.Handling;
using MinimalTelegramBot.StateMachine.Abstractions;
using MinimalTelegramBot.StateMachine.Extensions;
using NodaTime;
using StudyCompanion.Core.Builders;
using StudyCompanion.Core.Contracts;
using StudyCompanion.Core.Extensions;
using StudyCompanion.Core.Shared.Filters;
using StudyCompanion.Core.Data;
using StudyCompanion.Shared.Contracts;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using IResult = MinimalTelegramBot.Results.IResult;
using Results = MinimalTelegramBot.Results.Results;

namespace StudyCompanion.Core.Commands;

internal class SettingsCommand : IBotCommand
{
    public static List<CommandDescription> Commands { get; } =
    [
        new("/settings", "⚙️ Bot Settings", CommandChat.Private),
    ];

    private static InlineKeyboardMarkup GetButtons(Language lang) => new()
    {
        InlineKeyboard = 
        [[
            //InlineKeyboardButton.WithCallbackData(lang.GetLocalized(en => "📅 Import iCal", de => "📅 iCal importieren"), "settings_ical"),
            InlineKeyboardButton.WithCallbackData(lang.GetLocalized(en => "📚️ Set Language", de => "📚️ Sprache setzen"), "settings_language"),
            InlineKeyboardButton.WithCallbackData(lang.GetLocalized(en => "🌍 Set Timezone", de =>  "🌍 Zeitzone setzen"), "settings_timezone"),
        ],],
    };

    private static readonly InlineKeyboardMarkup _langButtons = new()
    {
        InlineKeyboard =
        [
            [
                InlineKeyboardButton.WithCallbackData(Language.English.ToLanguageString(), Language.English.ToString()),
                InlineKeyboardButton.WithCallbackData(Language.German.ToLanguageString(), Language.German.ToString()),
            ],
        ],
    };

    private static readonly List<string> _timezones =
    [
        "Etc/GMT+12",
        "Pacific/Honolulu",
        "America/Anchorage",
        "America/Los_Angeles",
        "America/Denver",
        "America/Chicago",
        "America/New_York",
        "Etc/GMT",
        "Europe/Berlin",
        "Europe/Athens",
        "Europe/Moscow",
        "Asia/Kolkata",
        "Asia/Shanghai",
        "Asia/Tokyo",
        "Australia/Sydney",
        "Pacific/Auckland",
    ];

    private static InlineKeyboardMarkup GetTimeZoneButtons()
    {
        Instant instant = SystemClock.Instance.GetCurrentInstant();

        IEnumerable<InlineKeyboardButton[]> buttons = _timezones
            .Select(tzString =>
            {
                DateTimeZone timezone = DateTimeZoneProviders.Tzdb[tzString];
                return InlineKeyboardButton.WithCallbackData(instant.InZone(timezone).ToDateTimeUnspecified().ToString("HH:mm"), tzString);
            })
            .Chunk(4);

        return new InlineKeyboardMarkup(buttons);
    }

    private static string GetTimeZoneString(DateTimeZone timezone)
    {
        Instant instant = SystemClock.Instance.GetCurrentInstant();

        return instant.InZone(timezone).ToDateTimeUnspecified().ToString("HH:mm") +
               $" (UTC{timezone.GetUtcOffset(instant)})";
    }

    public static void ConfigureCommands(BotApplication bot)
    {
        bot.HandleCommand("/settings", OnSettings)
            .FilterChatType(ChatType.Private);

        bot.HandleMessageText("⚙️ Settings", OnSettings)
            .FilterChatType(ChatType.Private);

        bot.HandleMessageText("⚙️ Einstellungen", OnSettings)
            .FilterChatType(ChatType.Private);
    }

    public static void ConfigureCallbacks(BotApplication bot)
    {
        bot.HandleCallbackData("settings_language", OnLanguage);

        bot.Handle(OnLanguageSelect)
            .FilterState<SetLanguageState.Setting>();

        bot.HandleCallbackData("settings_timezone", OnTimeZone);

        bot.Handle(OnTimeZoneSelect)
            .FilterState<SetTimezoneState.Setting>();
    }

    public static async Task<IResult> OnSettings(BotRequestContext context, IHelper helper)
    {
        await context.DropPrevious();
        
        if (context.Update.Message?.ConvertMessage() is not Message msg || msg.Chat is not TelegramUser telegramUser)
            return Results.Empty;

        // ensure user exists
        User user = await helper.GetUserAsync(telegramUser);

        Language lang = user.Settings.Language;
        string langStr = lang.ToLanguageString().Bold();

        string timeZone = GetTimeZoneString(user.Settings.TimeZone);

        string text = lang.GetLocalized(
            en =>
                "⚙️ Settings".Bold().Newline(2) +
                "📚️ Language: " + langStr.Newline() +
                "🌍 Timezone: " + timeZone.Bold(),
            de =>
                "⚙️ Einstellungen".Bold().Newline(2) +
                "📚️ Sprache: " + langStr.Newline() +
                "🌍 Zeitzone: " + timeZone.Bold()
        );

        return text.AsMarkup().Delete().WithButtons(GetButtons(lang));
    }

    #region Set Language
    [StateGroup(nameof(SetLanguageState))]
    public static class SetLanguageState
    {
        [State(1)]
        public class Setting;
    }

    public static async Task<IResult> OnLanguage(BotRequestContext context, IHelper helper, PostgresDbContext db)
    {
        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        await context.SetState(new SetLanguageState.Setting());

        string text = user.Settings.Language.GetLocalized(
            en => "📚 Please select your preferred language:".Bold(),
            de => "📚 Bitte wähle deine bevorzugte Sprache:".Bold()
        );

        return text.AsMarkup().WithButtons(_langButtons).Delete();
    }

    public static async Task<IResult> OnLanguageSelect(BotRequestContext context, IHelper helper, PostgresDbContext db)
    {
        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        if (await context.GetState<SetLanguageState.Setting>() is null)
            return Results.Empty;

        await context.DropState();

        if (context.Update.CallbackQuery?.Data is not string data || !Enum.TryParse(data, out Language language))
            return Results.Empty;

        user.Settings.Language = language;
        db.Update(user);
        await db.SaveChangesAsync();

        string text = user.Settings.Language.GetLocalized(
            en => $"✔ Language was set to {language.ToLanguageString().Bold()}.",
            de => $"✔ Sprache wurde auf {language.ToLanguageString().Bold()} gesetzt."
        );

        return text.AsMarkup().Delete().WithButtons(StartCommand.GetButtons(language, user.Role));
    }
    #endregion

    #region Set Language
    [StateGroup(nameof(SetTimezoneState))]
    public static class SetTimezoneState
    {
        [State(1)]
        public class Setting;
    }

    public static async Task<IResult> OnTimeZone(BotRequestContext context, IHelper helper, PostgresDbContext db)
    {
        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        await context.SetState(new SetTimezoneState.Setting());

        string text = user.Settings.Language.GetLocalized(
            en => "🌍 Please select the time that matches your timezone:".Bold(),
            de => "🌍 Bitte wähle die Zeit welche deiner Zeitzone entspricht:".Bold()
        );

        return text.AsMarkup().WithButtons(GetTimeZoneButtons()).Delete();
    }

    public static async Task<IResult> OnTimeZoneSelect(BotRequestContext context, IHelper helper, PostgresDbContext db)
    {
        if (await db.Users.Include(p => p.Settings).FirstOrDefaultAsync(p => p.TelegramUser.Id == context.ChatId) is not User user)
            return Results.Empty;

        if (await context.GetState<SetTimezoneState.Setting>() is null)
            return Results.Empty;

        await context.DropState();

        if (context.Update.CallbackQuery?.Data is not string data || DateTimeZoneProviders.Tzdb.GetZoneOrNull(data) is not DateTimeZone timezone)
            return Results.Empty;

        user.Settings.TimeZone = timezone;
        db.Update(user);
        await db.SaveChangesAsync();

        string timezoneString = GetTimeZoneString(timezone);

        string text = user.Settings.Language.GetLocalized(
            en => $"✔ Timezon was set to {timezoneString.Bold()}.",
            de => $"✔ Zeitzone wurde auf {timezoneString.Bold()} gesetzt."
        );

        return text.AsMarkup().Delete();
    }
    #endregion
}

