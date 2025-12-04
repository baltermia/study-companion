namespace StudyCompanion.Core.Services;

public class CalendarService
{
    public async Task<string> FetchCalendar(string link, CancellationToken token = default)
    {
        using HttpClient client = new() ;
        return await client.GetStringAsync(link, token);
    }
}