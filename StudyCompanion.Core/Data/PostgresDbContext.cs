using Microsoft.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore.Metadata;
using MinimalTelegramBot.StateMachine.Persistence.EntityFrameworkCore;
using NodaTime;
using StudyCompanion.Shared.Models;

namespace StudyCompanion.Core.Data;

public class PostgresDbContext : DbContext, IStateMachineDbContext
{
    public DbSet<User> Users { get; set; }

    public DbSet<MinimalTelegramBotState> MinimalTelegramBotStates { get; set; }

    public PostgresDbContext(DbContextOptions options)
        : base(options)
    { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        // Sets the deletion behavior to restricted (data can't be deleted if a forgein key still points to it)
        foreach (IMutableForeignKey relationship in modelBuilder.Model.GetEntityTypes().SelectMany(e => e.GetForeignKeys()))
            relationship.DeleteBehavior = DeleteBehavior.Restrict;

        modelBuilder.Entity<User>(user =>
        {
            user
                .OwnsOne(p => p.TelegramUser,
                    u => u.Property(w => w.Id).HasColumnName("TelegramUserId"));
        });

        modelBuilder.Entity<Settings>(settings =>
        {
            settings
                .Property(s => s.TimeZone)
                .HasConversion(
                    tz => tz.Id,
                    str => DateTimeZoneProviders.Tzdb[str]
                );
        });

        base.OnModelCreating(modelBuilder);
    }
}