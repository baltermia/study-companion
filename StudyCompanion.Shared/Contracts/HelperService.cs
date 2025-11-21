using Microsoft.EntityFrameworkCore;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Microsoft.Extensions.Options;
using StudyCompanion.Shared.Options;
using NodaTime;

namespace StudyCompanion.Shared.Services;

public class NewPlayerEventArgs(Player player) : EventArgs
{
    public Player Player { get; } = player;
}

public interface IHelper
{
    public static event AsyncEventHandler<NewPlayerEventArgs>? NewPlayer;

    public Task<Player> GetPlayerAsync(User user, long? referrerId = null, Func<Player, Task>? referred = null);
}

public class HelperService<T>(T context, IOptions<UserOptions> options) : IHelper
    where T : DbContext
{
    
    public static event AsyncEventHandler<NewPlayerEventArgs>? NewPlayer;

    public async Task<Player> GetPlayerAsync(User user, long? referrerId = null, Func<Player, Task>? referred = null)
    {
        Player? player =
            await context
                .Set<Player>()
                .Include(p => p.ReferredBy)
                .Include(p => p.Settings)
                .FirstOrDefaultAsync(p => p.User.Id == user.Id);

        if (player == null)
        {
            Player? referrer = 
                await context
                    .Set<Player>()
                    .Include(p => p.Settings)
                    .FirstOrDefaultAsync(p => p.User.Id == referrerId);

            string? defaultTimeZone = options.Value.DefaultTimeZone;

            player = (await context.AddAsync(new Player()
            {
                User = user,
                ReferredBy = referrer,
                Settings = new()
                {
                    TimeZone = string.IsNullOrWhiteSpace(defaultTimeZone) ? DateTimeZone.Utc : DateTimeZoneProviders.Tzdb[defaultTimeZone],
                },
            })).Entity;

            await context.SaveChangesAsync();

            if (referrer != null && referred != null)
                await referred(referrer);
        }

        // update username
        if (player.User.Username != user.Username && user.Username != null)
        {
            player.User.Username = user.Username;

            context.Update(player);

            await context.SaveChangesAsync();
        }

        return player;
    }
}
