using Microsoft.EntityFrameworkCore;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using Microsoft.Extensions.Options;
using StudyCompanion.Shared.Options;
using NodaTime;

namespace StudyCompanion.Shared.Services;

public class NewUserEventArgs(User user) : EventArgs
{
    public User User { get; } = user;
}

public interface IHelper
{
    public static event AsyncEventHandler<NewUserEventArgs>? NewUser;

    public Task<User> GetUserAsync(TelegramUser telegramUser);
}

public class HelperService<T>(IDbContextFactory<T> contextFactory, IOptions<UserOptions> options) : IHelper
    where T : DbContext
{
    public async Task<User> GetUserAsync(TelegramUser telegramUser)
    {
        await using T context = await contextFactory.CreateDbContextAsync();
        User? user =
            await context
                .Set<User>()
                .Include(p => p.Settings)
                .FirstOrDefaultAsync(p => p.TelegramUser.Id == telegramUser.Id);

        if (user == null)
        {
            string? defaultTimeZone = options.Value.DefaultTimeZone;

            user = (await context.AddAsync(new User()
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
        if (user.TelegramUser.Username != telegramUser.Username && telegramUser.Username != null)
        {
            user.TelegramUser.Username = telegramUser.Username;

            context.Update(user);

            await context.SaveChangesAsync();
        }

        return user;
    }
}
