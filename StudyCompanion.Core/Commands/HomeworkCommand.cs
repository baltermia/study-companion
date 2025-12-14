using System.Globalization;
using Microsoft.EntityFrameworkCore;
using MinimalTelegramBot;
using MinimalTelegramBot.Builder;
using MinimalTelegramBot.Handling;
using MinimalTelegramBot.StateMachine.Abstractions;
using MinimalTelegramBot.StateMachine.Extensions;
using StudyCompanion.Core.Builders;
using StudyCompanion.Core.Contracts;
using StudyCompanion.Core.Data;
using StudyCompanion.Core.Extensions;
using StudyCompanion.Core.Helpers;
using StudyCompanion.Core.Jobs;
using StudyCompanion.Core.Shared;
using StudyCompanion.Core.Shared.Filters;
using StudyCompanion.Shared.Contracts;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Telegram.Bot.Types.Enums;
using Telegram.Bot.Types.ReplyMarkups;
using TickerQ.Utilities;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;
using IResult = MinimalTelegramBot.Results.IResult;
using Results = MinimalTelegramBot.Results.Results;

namespace StudyCompanion.Core.Commands;

public class HomeworkCommand : IBotCommand
{
    public static string GetTitle(Language lang) => lang.GetLocalized(
        en => "📌 Homework",
        de => "📌 Hausaufgaben");
    
    public static List<CommandDescription> Commands { get; } =
    [
        new("/homework", "📌 Homework", CommandChat.Private),
    ];

    private static InlineKeyboardMarkup GetButtons(Language lang, bool hasHomework)
    {
        List<InlineKeyboardButton> buttons =
        [
            InlineKeyboardButton.WithCallbackData(lang.GetLocalized(en => "➕ New", de => "➕ Neu"), "homework_new"),
        ];

        if (hasHomework)
            buttons.Add(InlineKeyboardButton.WithCallbackData(lang.GetLocalized(en => "❌ Delete", de => "❌ Löschen"),
                "homework_delete"));

        return new()
        {
            InlineKeyboard = [buttons]
        };
    }

    public static void ConfigureCommands(BotApplication bot)
    {
        bot.HandleCommand("/homework", OnHomework)
            .FilterChatType(ChatType.Private);

        bot.HandleMessageText(GetTitle(Language.English), OnHomework)
            .FilterChatType(ChatType.Private);

        bot.HandleMessageText(GetTitle(Language.German), OnHomework)
            .FilterChatType(ChatType.Private);
    }

    public static void ConfigureCallbacks(BotApplication bot)
    {
        bot.HandleCallbackData("homework_new", OnNew);
        bot.HandleCallbackData("homework_delete", OnDelete);

        bot.HandleUpdateType(UpdateType.Message, OnNote)
            .FilterState<NewHomeworkState.GetNote>();

        bot.HandleUpdateType(UpdateType.Message, OnDue)
            .FilterState<NewHomeworkState.GetDue>();
        
        bot.Handle(OnConfirmAdd)
            .FilterState<NewHomeworkState.Confirm>();
        
        bot.HandleUpdateType(UpdateType.Message, OnId)
            .FilterState<DeleteHomeworkState.GetIndex>();

        bot.Handle(OnConfirmDelete)
            .FilterState<DeleteHomeworkState.Confirm>();
    }

    private static async Task<IResult> OnHomework(BotRequestContext context, IHelper helper)
    {
        await context.DropPrevious();

        if (await helper.GetUserAsync(context.ChatId, true) is not User user)
            return Results.Empty;

        Language lang = user.Settings.Language;
        CultureInfo culture = lang.ToCultureInfo();

        string text = lang.GetLocalized(
            en => "📌 Your Homework",
            de => "📌 Deine Hausaufgaben"
        ).Bold().Newline().Newline();

        List<Homework> open = user.Homework.Where(h => !h.CompletedAt.HasValue).ToList();

        bool noHomework = open.Count == 0;

        if (noHomework)
        {
            text += lang.GetLocalized(
                en => "Yay! You dont have any homework at the moment 😄",
                de => "Yay! Du hast im Moment keine Hausaufgaben 😄"
            );
        }
        else
        {
            DateOnly now = DateOnly.FromDateTime(DateTime.Now);
            DateOnly week = now.AddDays(7);

            foreach ((Homework hw, int index) in open.OrderBy(h => h.Due).Select((h, i) => (h, i)))
            {
                if (hw.Due < now)
                    text += $"{index + 1}: ⚠️ [{hw.Due.ToString("d", culture)}] {hw.Note}".Newline();
                else if (hw.Due < week)
                    text += $"{index + 1}: ⌛ [{hw.Due.ToString("dddd MM.dd", culture)}] {hw.Note}".Newline();
                else
                    text += $"{index + 1}: ⏩ [{hw.Due.ToString("d", culture)}] {hw.Note}".Newline();
            }
        }
        
        List<Homework> today = user.Homework.Where(h => h.CompletedAt.HasValue && h.CompletedAt.Value.Date == DateTime.UtcNow.Date).ToList();
        
        if (today.Count > 0)
        {
            text = text.Newline().Newline() + lang.GetLocalized(
                en => "✅ Completed Today:".Newline(),
                de => "✅ Heute erledigt:".Newline()
            ).Bold();

            foreach (Homework hw in today.OrderBy(h => h.Due))
                text += $"- {hw.Note}".Newline();
        }

        return text
            .WithButtons(GetButtons(lang, !noHomework))
            .Delete()
            .AsMarkup();
    }

    [StateGroup(nameof(NewHomeworkState))]
    public static class NewHomeworkState
    {
        [State(1)]
        public class GetNote;

        [State(2)]
        public class GetDue
        {
            public required string Note { get; init; }
        };

        [State(3)]
        public class Confirm
        {
            public required string Note { get; init; }
            public required DateTime Due { get; init; }
        };
    }

    private static async Task<IResult> OnNew(BotRequestContext context, IHelper helper)
    {
        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        await context.SetState(new NewHomeworkState.GetNote());

        return user.Settings.Language.GetLocalized(
            en => "Response with the Note of your Homework.",
            de => "Antworte mit der Notiz zu deiner Hausaufgaben"
        ).Delete();
    }

    private static async Task<IResult> OnNote(BotRequestContext context, IHelper helper)
    {
        if (string.IsNullOrWhiteSpace(context.MessageText))
            return Results.Empty;

        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        Language lang = user.Settings.Language;

        await context.SetState(new NewHomeworkState.GetDue()
        {
            Note = context.MessageText,
        });

        return lang.GetLocalized(
            en =>
                "Please provide the due date of your Homework in a readable format (try natural language like 'next thursday' or simply DD.MM.YYYY).",
            de =>
                "Bitte gib das Fälligkeitsdatum deiner Hausaufgabe in einem lesbaren Format an (versuch es mit natürlicher sprache wie 'nächsten montag' oder einfach DD.MM.YYYY)."
        ).Delete();
    }

    private static readonly string _addPrefix = "homework_add_";
    private static readonly string _deletePrefix = "homework_delete_";

    private static async Task<IResult> OnDueRe(BotRequestContext context, IHelper helper)
    {
        if (string.IsNullOrWhiteSpace(context.MessageText))
            return Results.Empty;

        if (await context.GetState<NewHomeworkState.Confirm>() is not NewHomeworkState.Confirm confirm)
            return Results.Empty;

        return await HandleDue(context, helper, confirm.Note);
    }

    private static async Task<IResult> OnDue(BotRequestContext context, IHelper helper)
    {
        if (string.IsNullOrWhiteSpace(context.MessageText))
            return Results.Empty;

        if (await context.GetState<NewHomeworkState.GetDue>() is not NewHomeworkState.GetDue dueState)
            return Results.Empty;
        
        return await HandleDue(context, helper, dueState.Note);
    }

    private static async Task<IResult> HandleDue(BotRequestContext context, IHelper helper, string note)
    {
        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        Language lang = user.Settings.Language;
        CultureInfo culture = lang.ToCultureInfo();

        DateTime? due = RecognizerDateTimeHelper.ParseDateTime(context.MessageText, culture.ToString());

        if (due == null)
            return lang.GetLocalized(
                en => "Please try again, respond with the notes due date.",
                de => "Bitte versuche es erneut, antworte mit dem Fälligkeitsdatum der Hausaufgabe."
            ).Delete();

        await context.SetState(new NewHomeworkState.Confirm()
        {
            Note = note,
            Due = due.Value,
        });
            
        return user.Settings.Language.GetLocalized(
            en =>
                "Please confirm the addition of the homework:".Bold().Newline()
                + $"🗒️ Note: {note}".Newline()
                + $"📅 Due: {due.Value.ToString("D", culture)}".Newline().Newline()
                + "If you want to change the date, you can simply respond again",
            de =>
                "Bitte bestätige das hinzufügen der Hausaufgabe:".Bold().Newline()
                + $"🗒️ Notiz: {note}".Newline()
                + $"📅 Fällig: {due.Value.ToString("D", culture)}".Newline().Newline()
                + "Falls du das Datum ändern möchtest, kannst du einfach nochmals antworten"
        ).Delete().AsMarkup().WithButtons(Buttons.YesNoKeyboard(_addPrefix, lang));
    }

    private static async Task<IResult> OnConfirmAdd(BotRequestContext context, IHelper helper, PostgresDbContext db, ITimeTickerManager<TimeTickerEntity> ticker)
    {
        if (context.Update.Type == UpdateType.Message)
            return await OnDueRe(context, helper);
        
        bool? confirm = Buttons.ParseYesNoCallback(context.CallbackData, _addPrefix);

        if (confirm == null)
            return Results.Empty;

        if (await context.GetState<NewHomeworkState.Confirm>() is not NewHomeworkState.Confirm confirmation)
            return Results.Empty;

        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        await context.DropState();

        if (!confirm.Value)
            return user.Settings.Language.GetLocalized(
                en => "Homework not added.",
                de => "Hausaufgabe nicht hinzugefügt."
            ).Delete();
        
        DateOnly due = DateOnly.FromDateTime(confirmation.Due);

        Homework homework = new()
        {
            Due = due,
            Note = confirmation.Note,
        };
        
        user.Homework.Add(homework);

        db.Update(user);

        await db.SaveChangesAsync();
        
        DateTime utcAtMidday = TimeZoneInfo.FindSystemTimeZoneById(user.Settings.TimeZone.Id).ToMiddayUtc(due.AddDays(-1));

        await ticker.AddAsync(new TimeTickerEntity()
        {
            Function = nameof(HomeworkJob.RemindHomework),
            ExecutionTime = utcAtMidday,
            Description = $"User={user.Id};Homework={homework.Id};",
            Request = TickerHelper.CreateTickerRequest(new HomeworkJobData(homework.Id, homework.Note)),
        });

        return user.Settings.Language.GetLocalized(
            en => "✅ Homework added successfully!",
            de => "✅ Hausaufgabe erfolgreich hinzugefügt!"
        ).Delete();
    }

    [StateGroup(nameof(DeleteHomeworkState))]
    public static class DeleteHomeworkState
    {
        [State(1)]
        public class GetIndex;

        [State(2)]
        public class Confirm
        {
            public required int Id { get; init; }
        };
    }

    private static async Task<IResult> OnDelete(BotRequestContext context, IHelper helper)
    {
        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        await context.SetState(new DeleteHomeworkState.GetIndex());

        return user.Settings.Language.GetLocalized(
            en => "Respond with the Index you want to delete.",
            de => "Antworte mit dem Index den du löschen möchtest."
        ).Delete();
    }

    private static async Task<IResult> OnId(BotRequestContext context, IHelper helper)
    {
        if (string.IsNullOrWhiteSpace(context.MessageText))
            return Results.Empty;

        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        Language lang = user.Settings.Language;
        CultureInfo culture = lang.ToCultureInfo();
        
        if (!int.TryParse(context.MessageText, out int index))
            return lang.GetLocalized(
                en => "Parsing failed. Please try again.",
                de => "Parsing fehlgeschlagen. Bitte versuche es erneut."
            ).Delete();
        
        List<Homework> open = user.Homework.Where(h => !h.CompletedAt.HasValue).ToList();
        
        if (index < 1 || index > open.Count)
            return lang.GetLocalized(
                en => "The provided index is out of range. Please try again.",
                de => "Der angegebene Index ist außerhalb des gültigen Bereichs. Bitte versuche es erneut."
            ).Delete();

        if (open.OrderBy(h => h.Due).ElementAtOrDefault(index - 1) is not Homework homework)
            return user.Settings.Language.GetLocalized(
                en => "Homework Index not found.",
                de => "Hausaufgabe nicht gefunden."
            ).Delete();
        
        await context.SetState(new DeleteHomeworkState.Confirm()
        {
            Id = homework.Id,
        });

        return lang.GetLocalized(
            en => $"Please confirm you want to delete '{homework.Note}' (due on {homework.Due.ToString("d", culture)}).",
            de => $"Bitte bestätige, dass du '{homework.Note}' (fällig am {homework.Due.ToString("d", culture)}) löschen möchtest."
        ).Delete().WithButtons(Buttons.YesNoKeyboard(_deletePrefix, lang));
    }

    private static async Task<IResult> OnConfirmDelete(BotRequestContext context, IHelper helper, PostgresDbContext db, ITimeTickerManager<TimeTickerEntity> ticker)
    {
        bool? confirm = Buttons.ParseYesNoCallback(context.CallbackData, _deletePrefix);

        if (confirm == null)
            return Results.Empty;

        if (await context.GetState<DeleteHomeworkState.Confirm>() is not DeleteHomeworkState.Confirm confirmation)
            return Results.Empty;

        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;

        await context.DropState();

        if (!confirm.Value)
            return user.Settings.Language.GetLocalized(
                en => "Homework not deleted.",
                de => "Hausaufgabe nicht gelöscht."
            ).Delete();

        if (await db.Set<Homework>().FirstOrDefaultAsync(x => x.Id == confirmation.Id) is not Homework homework)
            return user.Settings.Language.GetLocalized(
                en => "Homework Index not found.",
                de => "Hausaufgabe nicht gefunden."
            ).Delete();
        
        db.Remove(homework);
        await db.SaveChangesAsync();
        
        if (await db.Set<TimeTickerEntity>().FirstOrDefaultAsync(t => t.Description.Contains($"Homework={confirmation.Id};")) is TimeTickerEntity entity)
            await ticker.DeleteAsync(entity.Id);
        
        return user.Settings.Language.GetLocalized(
            en => "✅ Homework deleted successfully!",
            de => "✅ Hausaufgabe erfolgreich gelöscht!"
        ).Delete();
    }
}