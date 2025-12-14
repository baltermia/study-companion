using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Caching.Distributed;
using StudyCompanion.Core.Builders;
using StudyCompanion.Core.Data;
using StudyCompanion.Shared.Contracts;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Telegram.Bot;
using TickerQ.Utilities.Base;

namespace StudyCompanion.Core.Jobs;

public record MorningJobData(int UserId);

public class MorningJob(PostgresDbContext db, ITelegramBotClient bot, IDistributedCache cache, IAiService ai)
{
    [TickerFunction(nameof(RemindMorning))]
    public async Task RemindMorning(TickerFunctionContext<MorningJobData> context, CancellationToken token)
    {
        MorningJobData data = context.Request;
        
        User? user = await db.Set<User>()
            .Include(u => u.Settings)
            .FirstOrDefaultAsync(u => u.Id == data.UserId, token);
        
        if (user == null)
            return;

        Language lang = user.Settings.Language;

        string text = lang.GetLocalized(
            en => "🌅 Good morning!",
            de => "🌅 Guten morgen!"
        ).Bold().Newline();

        string summary = await ai.GetUserSummary(user);
        
        await text
            .AsMarkup()
            .ExecuteAsync(user.TelegramUser.Id, bot, cache);
    }
}