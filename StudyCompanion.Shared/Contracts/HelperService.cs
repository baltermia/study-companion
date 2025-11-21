using Microsoft.EntityFrameworkCore;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Microsoft.Extensions.Options;
using StudyCompanion.Shared.Options;
using NodaTime;

namespace StudyCompanion.Shared.Services;

public class NewPlayerEventArgs(User user) : EventArgs
{
    public User User { get; } = user;
}

public interface IHelper
{
    public static event AsyncEventHandler<NewPlayerEventArgs>? NewPlayer;

    public Task<User> GetPlayerAsync(TelegramUser telegramUser);
}

public class HelperService<T>(IDbContextFactory<T> contextFactory, IOptions<UserOptions> options) : IHelper
    where T : DbContext
{
    
    public static event AsyncEventHandler<NewPlayerEventArgs>? NewPlayer;

    public async Task<User> GetPlayerAsync(TelegramUser telegramUser)
    {
        await using T context = await contextFactory.CreateDbContextAsync();
        User? player =
            await context
                .Set<User>()
                .Include(p => p.Settings)
                .FirstOrDefaultAsync(p => p.TelegramUser.Id == telegramUser.Id);

        if (player == null)
        {
            string? defaultTimeZone = options.Value.DefaultTimeZone;

            player = (await context.AddAsync(new User()
            {
                TelegramUser = telegramUser,
                Settings = new()
                {
                    TimeZone = string.IsNullOrWhiteSpace(defaultTimeZone) ? DateTimeZone.Utc : DateTimeZoneProviders.Tzdb[defaultTimeZone],
                },
            })).Entity;

            await context.SaveChangesAsync();
        }

        // update username
        if (player.TelegramUser.Username != telegramUser.Username && telegramUser.Username != null)
        {
            player.TelegramUser.Username = telegramUser.Username;

            context.Update(player);

            await context.SaveChangesAsync();
        }

        return player;
    }
}
