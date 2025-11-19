using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using Microsoft.Extensions.Options;
using Telegram.Bot;
using Telegram.Bot.Polling;
using Telegram.Bot.Types;
using Telegram.Bot.Types.Enums;

namespace StudyCompanion.Bot;

public class BotService : BackgroundService
{
    private readonly ILogger<BotService> _logger;
    private readonly BotConfiguration _configuration;
    private ITelegramBotClient? _botClient;

    public BotService(
        ILogger<BotService> logger,
        IOptions<BotConfiguration> configuration)
    {
        _logger = logger;
        _configuration = configuration.Value;
    }

    protected override async Task ExecuteAsync(CancellationToken stoppingToken)
    {
        if (string.IsNullOrWhiteSpace(_configuration.BotToken))
        {
            _logger.LogError("Bot token is not configured. Please set the BotToken in appsettings.json or environment variable.");
            return;
        }

        _botClient = new TelegramBotClient(_configuration.BotToken);

        var me = await _botClient.GetMe(stoppingToken);
        _logger.LogInformation("Bot started successfully: @{BotUsername}", me.Username);

        var receiverOptions = new ReceiverOptions
        {
            AllowedUpdates = Array.Empty<UpdateType>() // receive all update types
        };

        _botClient.StartReceiving(
            HandleUpdateAsync,
            HandlePollingErrorAsync,
            receiverOptions,
            stoppingToken
        );

        _logger.LogInformation("Bot is now receiving messages. Press Ctrl+C to stop.");

        // Keep the service running until cancellation is requested
        await Task.Delay(Timeout.Infinite, stoppingToken);
    }

    private async Task HandleUpdateAsync(ITelegramBotClient botClient, Update update, CancellationToken cancellationToken)
    {
        if (update.Message is not { } message)
            return;

        if (message.Text is not { } messageText)
            return;

        var chatId = message.Chat.Id;

        _logger.LogInformation("Received a '{MessageText}' message in chat {ChatId}", messageText, chatId);

        // Echo the message back
        await botClient.SendMessage(
            chatId: chatId,
            text: $"You said: {messageText}",
            cancellationToken: cancellationToken);
    }

    private Task HandlePollingErrorAsync(ITelegramBotClient botClient, Exception exception, CancellationToken cancellationToken)
    {
        _logger.LogError(exception, "Error occurred while receiving updates");
        return Task.CompletedTask;
    }

    public override async Task StopAsync(CancellationToken cancellationToken)
    {
        _logger.LogInformation("Stopping bot service...");
        await base.StopAsync(cancellationToken);
    }
}
