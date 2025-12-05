using System.Text;

namespace StudyCompanion.Shared.Extensions;

public static class TimeSpanExtensions
{
    public static string ToCompactString(this TimeSpan ts)
    {
        if (ts == TimeSpan.Zero) return "0s";

        StringBuilder sb = new();
        if (ts < TimeSpan.Zero)
        {
            sb.Append("-");
            ts = ts.Negate();
        }

        if (ts.Days > 0) sb.Append($"{ts.Days}d ");
        if (ts.Hours > 0) sb.Append($"{ts.Hours}h ");
        if (ts.Minutes > 0) sb.Append($"{ts.Minutes}m ");
        if (ts.Seconds > 0) sb.Append($"{ts.Seconds}s ");

        // Optionally include milliseconds if <1s and non-zero
        if (sb.Length == 0 && ts.Milliseconds > 0) sb.Append($"{ts.Milliseconds}ms ");

        return sb.ToString().TrimEnd();
    }
}
