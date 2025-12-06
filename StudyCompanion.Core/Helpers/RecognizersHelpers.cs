using System.Globalization;
using Microsoft.Recognizers.Text;
using Microsoft.Recognizers.Text.DateTime;

namespace StudyCompanion.Core.Helpers;

/// <summary>
/// Helper wrapper around Microsoft.Recognizers.Text to parse a single datetime from free text.
/// Returns the first parseable DateTime (local) or null if none found.
/// </summary>
public static class RecognizerDateTimeHelper
{
    /// <summary>
    /// Parse the input text for a single datetime (e.g. "tomorrow", "next thursday", "15. december").
    /// Returns the first DateTime found (converted to local time) or null when no parseable date/time is detected.
    /// </summary>
    /// <param name="text">Text containing a date/time expression.</param>
    /// <param name="culture">Culture constant from Microsoft.Recognizers.Text.Culture (defaults to English).</param>
    /// <param name="referenceDate">Optional reference date used for relative expressions (defaults to DateTime.Now).</param>
    /// <returns>Parsed DateTime (local) or null.</returns>
    public static DateTime? ParseDateTime(string text, string culture = Culture.English, DateTime? referenceDate = null)
    {
        if (string.IsNullOrWhiteSpace(text))
        {
            return null;
        }

        var refDate = referenceDate ?? DateTime.Now;

        // RecognizeDateTime returns a list of ModelResult objects.
        var results = DateTimeRecognizer.RecognizeDateTime(text, culture, refTime: refDate);

        if (results == null || results.Count == 0)
        {
            return null;
        }

        // Iterate results and resolution values; prefer "value", fall back to "start"
        foreach (var result in results)
        {
            if (result.Resolution == null || !result.Resolution.ContainsKey("values"))
            {
                continue;
            }

            if (!(result.Resolution["values"] is IList<Dictionary<string, string>> values))
            {
                continue;
            }

            foreach (var v in values)
            {
                // value usually contains a single datetime like "2025-12-07" or "2025-12-07T14:00:00"
                v.TryGetValue("value", out var rawValue);
                if (string.IsNullOrEmpty(rawValue))
                {
                    // For ranges, recognizer uses "start" and "end"
                    v.TryGetValue("start", out rawValue);
                }

                if (string.IsNullOrEmpty(rawValue))
                {
                    continue;
                }

                // Try parse as DateTime first (invariant culture because recognizer returns ISO-like values)
                if (DateTime.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal, out var dt))
                {
                    // Ensure result is in local time
                    return dt.ToLocalTime();
                }

                // If it contains offset, try DateTimeOffset
                if (DateTimeOffset.TryParse(rawValue, CultureInfo.InvariantCulture, DateTimeStyles.AssumeLocal | DateTimeStyles.AdjustToUniversal, out var dto))
                {
                    return dto.LocalDateTime;
                }

                // Last resort: try current culture parse
                if (DateTime.TryParse(rawValue, CultureInfo.CurrentCulture, DateTimeStyles.AssumeLocal, out dt))
                {
                    return dt;
                }
            }
        }

        return null;
    }
}
