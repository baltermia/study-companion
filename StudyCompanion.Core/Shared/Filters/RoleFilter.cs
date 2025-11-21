using StudyCompanion.Shared.Models;
using Microsoft.EntityFrameworkCore;
using MinimalTelegramBot.Handling;
using MinimalTelegramBot.Handling.Filters;

namespace StudyCompanion.Core.Shared.Filters;

internal static class RoleFilter
{
    public static THandler FilterRole<THandler>(this THandler builder, params Role[] roles)
        where THandler : IHandlerConventionBuilder =>
        builder.Filter(async context =>
        {
            await using AsyncServiceScope scope = context.Services.CreateAsyncScope();
            DbContext db = scope.ServiceProvider.GetRequiredService<DbContext>();

            return await db.Set<User>().FirstOrDefaultAsync(p => p.TelegramUser.Id == context.BotRequestContext.ChatId) is User player && roles.Contains(player.Role);
        });
}

