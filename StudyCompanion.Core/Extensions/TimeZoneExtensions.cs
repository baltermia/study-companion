namespace StudyCompanion.Core.Extensions;

public static class TimeZoneExtensions
{
    public static DateTime ToMiddayUtc(this TimeZoneInfo timezone, DateOnly date)
    {
        DateTime time = date.ToDateTime(new TimeOnly(12, 00));

        TimeSpan offset = timezone.GetUtcOffset(time);
        DateTimeOffset localMiddayOffset = new(time, offset);
        return localMiddayOffset.UtcDateTime;
    }
}