using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using MinimalTelegramBot.Builder;
using MinimalTelegramBot.StateMachine.Extensions;
using MinimalTelegramBot.StateMachine.Persistence.EntityFrameworkCore;
using StudyCompanion.Core.Contracts;
using Serilog;
using StudyCompanion.Core.Services;
using Telegram.Bot;
using Telegram.Bot.Types;
using StudyCompanion.Core.Data;
using StudyCompanion.Shared.Contracts;
using StudyCompanion.Shared.Options;
using StudyCompanion.Shared.Extensions;
using StudyCompanion.Core.Commands;
using OpenAI.Chat;
using StudyCompanion.Core.Jobs;
using TickerQ.DependencyInjection;
using TickerQ.EntityFrameworkCore.Customizer;
using TickerQ.EntityFrameworkCore.DependencyInjection;
using TickerQ.Utilities;
using TickerQ.Utilities.Entities;
using TickerQ.Utilities.Interfaces.Managers;

namespace StudyCompanion.Core;

public static class Program
{
    private static readonly List<Action<BotApplication>> _configureCommands = [];
    private static readonly List<Action<BotApplication>> _configureCallbacks = [];
    private static readonly List<CommandDescription> _commands = [];

    public static async Task Main(string[] args)
    {
        string? contentRoot = Environment.GetEnvironmentVariable("STUDY_COMPANION_BOT_CONTENT_ROOT");
        
        BotApplicationBuilder builder = BotApplication.CreateBuilder(new BotApplicationOptions()
        {
            WebApplicationOptions = new()
            {
                Args = args,
                ContentRootPath = contentRoot,
            }
        });
        
        // these are required for the bot to run, therefore the !
        string connectionString = builder.Configuration.GetConnectionString("DefaultConnection")!;
        string redisConnection = builder.Configuration.GetConnectionString("RedisConnection")!;

        builder.ConfigureAppsettings();
        
        builder.Services.AddSingleton(new ChatClient(model: "gpt-4o", apiKey: builder.Configuration.GetRequiredSection("OpenAI:Key").Value));
        builder.Services.AddTransient<IAiService, OpenAiService>();
        
        builder.Services
            .AddStateMachine()
            .PersistStatesToDbContext<PostgresDbContext>()
            .WithHybridCache();
        
        builder.WebHost.UseKestrelHttpsConfiguration();

        builder.Services
            .AddStackExchangeRedisCache(options =>
            {
                options.Configuration = redisConnection; // your redis connection
            })
            //.AddDistributedMemoryCache()
                
            .AddHostedService<CalendarRefreshService>()
            .AddPostgres(connectionString)
            //.AddSingleton<IConnectionMultiplexer>(await ConnectionMultiplexer.ConnectAsync(redisConnection));
            // libs
            //.ConfigureDataServices(builder.Configuration)

            // options
            .ConfigureOptions<AppOptions>(builder.Configuration);
            
        // hosted services
        //.AddHostedService<PayoutCheckerService>();

        builder.Logging.ConfigureLogging(builder.Configuration);

        builder.Services.AddTickerQ(options =>
        {
            options.AddOperationalStore(efOptions =>
            {
                efOptions.UseApplicationDbContext<PostgresDbContext>(ConfigurationType.UseModelCustomizer);
                efOptions.SetDbContextPoolSize(128);
            });
        });

        BotApplication bot = builder.Build();

        bot.UseStateMachine();

        // commands
        bot
            .ConfigureCommand<SummaryCommand>()
            .ConfigureCommand<SettingsCommand>()
            .ConfigureCommand<CalendarCommand>()
            .ConfigureCommand<HomeworkCommand>()
            .ConfigureCommand<StartCommand>()
            .ConfigureCallback<HomeworkCallback>();
            
        // configure commands and callbacks
        bot.ConfigureCommands();

        bot.WebApplicationAccessor.UseTickerQ();
        
        // set commands
        await SetTelegramCommandsAsync(bot.Services);
        
        await using (AsyncServiceScope scope = bot.Services.CreateAsyncScope())
        {
            PostgresDbContext db = scope.ServiceProvider.GetRequiredService<PostgresDbContext>();
            await db.Database.EnsureCreatedAsync();
            
            if ((await db.Database.GetPendingMigrationsAsync()).Any())
                await db.Database.MigrateAsync();
        }
        
        HelperService<PostgresDbContext>.NewUser += async (s, e) =>
        {
            IOptions<AppOptions> options = e.Services.GetRequiredService<IOptions<AppOptions>>();
            ICronTickerManager<CronTickerEntity> cronTicker = e.Services.GetRequiredService<ICronTickerManager<CronTickerEntity>>();
            
            TimeSpan time = options.Value.MorningReminderTime;
            TimeSpan offset = TimeZoneInfo.FindSystemTimeZoneById(e.User.Settings.TimeZone.Id).BaseUtcOffset;

            TimeSpan execution = time - offset;
            string expression = $"{execution.Seconds} {execution.Minutes} {execution.Hours} * * *";
            
            await cronTicker.AddAsync(new CronTickerEntity
            {
                Function = nameof(MorningJob.RemindMorning),
                Description = $"User={e.User.Id};",
                Request = TickerHelper.CreateTickerRequest(new MorningJobData(e.User.Id)),
                Expression = expression,
            });
        };

        await bot.RunAsync();
    }

    private static BotApplication ConfigureCommand<T>(this BotApplication bot) where T : IBotCommand
    {
        _configureCommands.Add(T.ConfigureCommands);
        _configureCallbacks.Add(T.ConfigureCallbacks);
        _commands.AddRange(T.Commands);

        return bot;
    }
    
    private static BotApplication ConfigureCallback<T>(this BotApplication bot) where T : IBotCallback
    {
        _configureCallbacks.Add(T.ConfigureCallbacks);

        return bot;
    }
    
    public static IServiceCollection AddPostgres(this IServiceCollection services, string connectionString) => services
        .AddDbContextPool<PostgresDbContext>(options => options.UseNpgsql(connectionString))
        .AddPooledDbContextFactory<PostgresDbContext>(options => options.UseNpgsql(connectionString))
        .AddScoped<IHelper, HelperService<PostgresDbContext>>();
    
    public static IServiceCollection AddMemoryDb(this IServiceCollection services) => services
        .AddDbContextPool<PostgresDbContext>(options => options.UseInMemoryDatabase("StudyCompanion"))
        .AddPooledDbContextFactory<PostgresDbContext>(options => options.UseInMemoryDatabase("StudyCompanion"))
        .AddScoped<IHelper, HelperService<PostgresDbContext>>();

    private static void ConfigureCommands(this BotApplication bot)
    {
        foreach (Action<BotApplication> configure in _configureCommands)
            configure.Invoke(bot);
        foreach (Action<BotApplication> configure in _configureCallbacks)
            configure.Invoke(bot);
    }
    
    private static async Task SetTelegramCommandsAsync(IServiceProvider provider)
    {
        using IServiceScope scope = provider.CreateScope();
        ITelegramBotClient client = scope.ServiceProvider.GetRequiredService<ITelegramBotClient>();

        // user commands
        await client.SetMyCommands(_commands.Where(cmd => cmd.Chat.HasFlag(CommandChat.Private)).Select(cmd => new BotCommand()
        {
            Command = cmd.Command,
            Description = cmd.Description
        }), BotCommandScope.AllPrivateChats());

        // group commands
        await client.SetMyCommands(_commands.Where(cmd => cmd.Chat.HasFlag(CommandChat.Group)).Select(cmd => new BotCommand()
        {
            Command = cmd.Command,
            Description = cmd.Description
        }), BotCommandScope.AllGroupChats());
    }

    private static void ConfigureLogging(this ILoggingBuilder builder, IConfiguration configuration)
    {
        Log.Logger = new LoggerConfiguration()
            .ReadFrom.Configuration(configuration)
            .CreateLogger();

        builder.ClearProviders();
        builder.AddSerilog();
    }
    
    private static void ConfigureAppsettings(this BotApplicationBuilder builder) => builder.Configuration
        .AddJsonFile("appsettings.json",
            optional: false,
            reloadOnChange: true)
        .AddJsonFile($"appsettings.{builder.Environment.EnvironmentName}.json",
            optional: true,
            reloadOnChange: true);

}