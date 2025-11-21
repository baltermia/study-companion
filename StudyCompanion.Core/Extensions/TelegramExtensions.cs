using StudyCompanion.Shared.Models;
using Telegram.Bot.Types.Enums;

namespace StudyCompanion.Core.Extensions;

public static class TelegramExtensions
{
    public static Message? ConvertMessage(this Telegram.Bot.Types.Message? message)
    {
        if (message == null)
            return null;

        return message.Chat.Type switch
        {
            ChatType.Private => new()
            {
                Chat = new TelegramUser()
                {
                    Id = message.Chat.Id,
                    Username =
                        message.Chat.Username != null
                            ? $"@{message.Chat.Username}"
                            : $"{message.Chat.FirstName} {message.Chat.LastName}".Trim(),
                },
                Text = message.Text,
                Id = message.MessageId
            },

            ChatType.Group or ChatType.Supergroup => new()
            {
                Chat = new Group()
                {
                    Id = message.Chat.Id,
                    Title =
                        message.Chat.Username != null
                            ? $"@{message.Chat.Username}"
                            : $"{message.Chat.FirstName} {message.Chat.LastName}".Trim(),
                    MessageThreadId = message.MessageThreadId,
                },
                Text = message.Text,
                Id = message.MessageId
            },

            _ => null,
        };
    }
}
