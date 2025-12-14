using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Options;
using NodaTime;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Shared.Models;
using StudyCompanion.Shared.Options;

namespace StudyCompanion.Shared.Contracts;

public class NewUserEventArgs(User user, IServiceProvider services) : EventArgs
{
    public User User { get; } = user;
    public IServiceProvider Services { get; } = services;
}

public interface IHelper
{
    public static virtual event AsyncEventHandler<NewUserEventArgs>? NewUser;

    public Task<User?> GetUserAsync(long userId, bool withCalendar = false);
    public Task<User> GetUserAsync(TelegramUser telegramUser, bool withCalendar = false);
}

public class HelperService<T>(IDbContextFactory<T> contextFactory, IOptions<AppOptions> options, IServiceScopeFactory factory) : IHelper
    where T : DbContext
{
    public static event AsyncEventHandler<NewUserEventArgs>? NewUser;
    
    public async Task<User?> GetUserAsync(long userId, bool withCalendar = false)
    {
        await using T db = await contextFactory.CreateDbContextAsync();
        
        if (withCalendar)
            return await db.Set<User>()
                .Include(p => p.Homework)
                .Include(p => p.Settings)
                .ThenInclude(s => s.Calender)
                .FirstOrDefaultAsync(p => p.TelegramUser.Id == userId);
        
        return await db.Set<User>()
            .Include(p => p.Settings)
            .Include(p => p.Homework)
            .FirstOrDefaultAsync(p => p.TelegramUser.Id == userId);
    }
    
    public async Task<User> GetUserAsync(TelegramUser telegramUser, bool withCalendar = false)
    {
        await using T context = await contextFactory.CreateDbContextAsync();
        User? user;
        
        if (withCalendar)
            user =
                await context
                    .Set<User>()
                    .Include(p => p.Homework)
                    .Include(p => p.Settings)
                    .ThenInclude(s => s.Calender)
                    .FirstOrDefaultAsync(p => p.TelegramUser.Id == telegramUser.Id);
        else
            user =
                await context
                    .Set<User>()
                    .Include(p => p.Homework)
                    .Include(p => p.Settings)
                    .FirstOrDefaultAsync(p => p.TelegramUser.Id == telegramUser.Id);

        if (user == null)
        {
            string? defaultTimeZone = options.Value.DefaultTimeZone;

            user = context.Add(new User
            {
                TelegramUser = telegramUser,
                Settings = new()
                {
                    TimeZone = string.IsNullOrWhiteSpace(defaultTimeZone) ? DateTimeZone.Utc : DateTimeZoneProviders.Tzdb[defaultTimeZone],
                },
            }).Entity;

            await context.SaveChangesAsync();

            if (NewUser != null)
            {
                await using AsyncServiceScope scope = factory.CreateAsyncScope();
                await NewUser.Invoke(this, new NewUserEventArgs(user, scope.ServiceProvider));
            }
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