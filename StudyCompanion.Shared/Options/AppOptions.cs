namespace StudyCompanion.Shared.Options;

public class AppOptions
{
    public string? DefaultTimeZone { get; set; }
    public int CalendarCheckMinutes { get; set; }
    public int CalendarRefreshHours { get; set; }
    public int CalendarEventOffsetMinutes { get; set; }
    public TimeSpan MorningReminderTime { get; set; }
}
