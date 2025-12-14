using System.Text.Json;
using Ical.Net;
using Ical.Net.DataTypes;
using OpenAI.Chat;
using StudyCompanion.Core.Commands;
using StudyCompanion.Shared.Contracts;
using StudyCompanion.Shared.Models;
using Calendar = Ical.Net.Calendar;

namespace StudyCompanion.Core.Services;

public class OpenAiService(ChatClient chat) : IAiService
{
    private static readonly List<ChatMessage> _messages =
    [
        new SystemChatMessage("You summarize calendar items and homework for a user in a concise and clear manner."),
        new SystemChatMessage("Use emojis. Also use <b> as first titles, and <i> as second titles for markup. Do NOT use any other markup."),
        new SystemChatMessage("Please consider the language that gets provided later as user message."),
        new SystemChatMessage("Don't include the users name."),
        new SystemChatMessage("You will receive the User information as a JSON object, and the Calender items in a list."),
        new SystemChatMessage("Also remember, the Time is in UTC but you get the Timezone in the user Settings."),
        new SystemChatMessage("You also get the language of the user, so please summarize in that language. 0 = English, 1 = German."),
    ];
    
    private record Event(DateTime Start, DateTime End, string Description);
    
    public async Task<string> GetUserSummary(User user)
    {
        string userJson = JsonSerializer.Serialize(user);

        List<ChatMessage> messages = new(_messages)
        {
            new UserChatMessage($"User JSON: {userJson}"),
        };
        
        if (user.Settings.Calender?.Data is string data && Calendar.Load(data) is Calendar ical)
        {
            (DateTime start, DateTime end) = CalendarCommand.GetWeekRange(0);

            CalDateTime calStart = new(start);
            CalDateTime calEnd = new(end);
            
            List<Event> events = ical.Events
                .Where(e => e.GetOccurrences(calStart).TakeWhileBefore(calEnd).Any())
                .Select(e => new Event(e.Start.AsUtc, e.End.AsUtc, e.Summary))
                .ToList();
            
            string eventJson = JsonSerializer.Serialize(events);

            messages.Add(new UserChatMessage($"Calendar Items: {eventJson}"));
        }
        
        ChatCompletion completion = await chat.CompleteChatAsync(messages);
        
        return completion.Content[0].Text;
    }

    private static readonly List<ChatMessage> _morningMessages =
    [
        new SystemChatMessage("You create a morning message which summarizes the daily calendar items and homework for a user in a concise and clear manner."),
        new SystemChatMessage("Use emojis. Also use <b> as first titles, and <i> as second titles for markup. Do NOT use any other markup."),
        new SystemChatMessage("Please consider the language that gets provided later as user message."),
        new SystemChatMessage("Don't include the users name."),
        new SystemChatMessage("You will receive the User information as a JSON object, and the Calender items in a list."),
        new SystemChatMessage("Also remember, the Time is in UTC but you get the Timezone in the user Settings."),
        new SystemChatMessage("You also get the language of the user, so please summarize in that language. 0 = English, 1 = German."),
    ];
    
    public async Task<string> GetUserMorningMessage(User user)
    {
        string userJson = JsonSerializer.Serialize(user);

        List<ChatMessage> messages = new(_morningMessages)
        {
            new UserChatMessage($"User JSON: {userJson}"),
        };
        
        if (user.Settings.Calender?.Data is string data && Calendar.Load(data) is Calendar ical)
        {
            (DateTime start, DateTime end) = CalendarCommand.GetWeekRange(0);

            CalDateTime calStart = new(start);
            CalDateTime calEnd = new(end);
            
            List<Event> events = ical.Events
                .Where(e => e.GetOccurrences(calStart).TakeWhileBefore(calEnd).Any())
                .Select(e => new Event(e.Start.AsUtc, e.End.AsUtc, e.Summary))
                .ToList();
            
            string eventJson = JsonSerializer.Serialize(events);

            messages.Add(new UserChatMessage($"Calendar Items: {eventJson}"));
        }
        
        ChatCompletion completion = await chat.CompleteChatAsync(messages);
        
        return completion.Content[0].Text;
    }
}