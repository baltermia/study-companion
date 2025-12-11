using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using MinimalTelegramBot;
using MinimalTelegramBot.Builder;
using MinimalTelegramBot.Handling;
using StudyCompanion.Core.Builders;
using StudyCompanion.Core.Contracts;
using StudyCompanion.Core.Data;
using StudyCompanion.Shared.Contracts;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Telegram.Bot;
using Telegram.Bot.Types.ReplyMarkups;
using TickerQ.Utilities.Base;
using IResult = MinimalTelegramBot.Results.IResult;
using Results = MinimalTelegramBot.Results.Results;

namespace StudyCompanion.Core.Jobs;
    
public record HomeworkJobData(int HomeworkId, string Note);

public class HomeworkCallbback : IBotCallback
{
    public static readonly string CALLBACK_PREFIX = "homework_remind_";
    
    public static void ConfigureCallbacks(BotApplication bot)
    {
        bot.HandleCallbackDataPrefix(CALLBACK_PREFIX, OnDone);
    }

    private static async Task<IResult> OnDone(BotRequestContext context, IHelper helper, PostgresDbContext db)
    {
        if (string.IsNullOrWhiteSpace(context.CallbackData))
            return Results.Empty;

        if (await helper.GetUserAsync(context.ChatId) is not User user)
            return Results.Empty;
        
        string data = context.CallbackData.Replace(CALLBACK_PREFIX, string.Empty);
        
        if (!int.TryParse(data, out int homeworkId))
            return Results.Empty;

        if (await db.Set<Homework>().FirstOrDefaultAsync(x => x.Id == homeworkId) is not Homework homework)
            return Results.Empty;

        db.Remove(homework);
        
        await db.SaveChangesAsync();
        
        if (context.Update.Message?.MessageId is int msgId)
            await context.Client.DeleteMessage(context.ChatId, msgId);
        
        return user.Settings.Language.GetLocalized(
            en => $"🎉 Homework {homework.Note.Code()} marked as done!",
            de => $"🎉 Hausaufgabe {homework.Note.Code()} als erledigt markiert!"
        ).AsMarkup();
    }
}

public class HomeworkJob(PostgresDbContext db, ITelegramBotClient bot, IDistributedCache cache)
{
    private static InlineKeyboardMarkup GetButtons(Language lang, int homeworkId) => new()
    {
        InlineKeyboard = [[
            InlineKeyboardButton.WithCallbackData(
                lang.GetLocalized(en => "✅ Done", de => "✅ Fertig"), 
                HomeworkCallbback.CALLBACK_PREFIX + homeworkId)
        ]]
    };
    
    [TickerFunction(nameof(RemindHomework))]
    public async Task RemindHomework(
        TickerFunctionContext<HomeworkJobData> context,
        CancellationToken token)
    {
        HomeworkJobData data = context.Request;
        if (await db.Set<User>().FirstOrDefaultAsync(u => u.Homework.Any(h => h.Id == data.HomeworkId)) is not User user)
            return;

        Language lang = user.Settings.Language;

        string text = lang.GetLocalized(
            en => "🔔 Homework Reminder",
            de => "🔔 Hausaufgaben Erinnerung"
        ).Bold().Newline();
        
        text += lang.GetLocalized(
            en => $"Due today: {data.Note}",
            de => $"Heute fällig: {data.Note}"
        );

        await text
            .AsMarkup()
            .WithButtons(GetButtons(lang, data.HomeworkId))
            .ExecuteAsync(user.TelegramUser.Id, bot, cache);
    }
}