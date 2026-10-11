using System.Text.RegularExpressions;

namespace Wuse.Services;

/// <summary>
/// Pure, fully local natural-language parser that turns free text into a
/// reminder due date/time plus the cleaned reminder text. No network, no AI.
///
/// Understands both "remind me &lt;text&gt; &lt;when&gt;" and "&lt;when&gt; &lt;text&gt;" forms:
///   today / tomorrow / tonight
///   weekday names ("friday", "next monday")
///   "in N minutes/hours/days/weeks"
///   "at 3pm", "at 15:00", "3:30 pm", "at noon", "at midnight"
///   "on June 5", "June 5th", "6/5", "6/5/2027"
///
/// Rules: a bare day defaults to 9:00 AM; a bare time with no day means today
/// (or tomorrow if that time already passed); with no when-pattern at all the
/// reminder defaults to tomorrow at 9:00 AM.
/// </summary>
public sealed class ReminderParser
{
    private const string MonthPattern =
        @"(january|february|march|april|may|june|july|august|september|october|november|december|jan|feb|mar|apr|jun|jul|aug|sep|sept|oct|nov|dec)";

    private static readonly string[] MonthNames =
    {
        "january", "february", "march", "april", "may", "june",
        "july", "august", "september", "october", "november", "december",
    };

    private readonly DateTime _now;

    public ReminderParser() : this(DateTime.Now)
    {
    }

    internal ReminderParser(DateTime now)
    {
        _now = now;
    }

    /// <summary>
    /// Parses input into a due date/time and the cleaned reminder text.
    /// </summary>
    public (DateTime Due, string Text) Parse(string input)
    {
        if (string.IsNullOrWhiteSpace(input))
            return (TomorrowAtNine(), "Reminder");

        string work = input.Trim();

        // 1. Strip a leading "remind me" style prefix.
        work = Regex.Replace(work, @"^(please\s+)?(could you|can you)\s+remind\s+me(\s+to)?\s+", string.Empty, RegexOptions.IgnoreCase);
        work = Regex.Replace(work, @"^(please\s+)?remind\s+me(\s+to)?\s+", string.Empty, RegexOptions.IgnoreCase);

        DateTime? explicitDate = null;
        TimeSpan? explicitTime = null;

        // 2. "in N minutes/hours/days/weeks" — relative offset from now.
        bool relativeIsDayGranularity = false;
        DateTime? relativeDue = null;
        var inMatch = Regex.Match(work, @"\bin\s+(\d+)\s*(minutes?|mins?|hours?|hrs?|h|days?|weeks?)\b", RegexOptions.IgnoreCase);
        if (inMatch.Success && int.TryParse(inMatch.Groups[1].Value, out int amount) && amount >= 0)
        {
            string unit = inMatch.Groups[2].Value.ToLowerInvariant();
            if (unit.StartsWith("min"))
            {
                relativeDue = _now + TimeSpan.FromMinutes(amount);
            }
            else if (unit.StartsWith("hour") || unit == "hrs" || unit == "hr" || unit == "h")
            {
                relativeDue = _now + TimeSpan.FromHours(amount);
            }
            else
            {
                int days = unit.StartsWith("week") ? amount * 7 : amount;
                relativeDue = _now + TimeSpan.FromDays(days);
                relativeIsDayGranularity = true;
            }
            work = RemoveOnce(work, inMatch);
        }

        // 3. today / tomorrow / tonight.
        var dayWord = Regex.Match(work, @"\b(today|tomorrow|tonight)\b", RegexOptions.IgnoreCase);
        if (dayWord.Success && relativeDue == null)
        {
            string word = dayWord.Groups[1].Value.ToLowerInvariant();
            explicitDate = word == "tomorrow" ? _now.Date.AddDays(1) : _now.Date;
            if (word == "tonight" && explicitTime == null)
                explicitTime = new TimeSpan(20, 0, 0);
            work = RemoveOnce(work, dayWord);
        }

        // 4. Weekday names — the next upcoming one ("next friday" = the one after that).
        var weekday = Regex.Match(
            work,
            @"\b(next\s+)?(monday|tuesday|wednesday|thursday|friday|saturday|sunday|mon|tue|wed|thu|fri|sat|sun)\b",
            RegexOptions.IgnoreCase);
        if (weekday.Success && relativeDue == null)
        {
            bool next = weekday.Groups[1].Value.Length > 0;
            DayOfWeek target = ParseDayOfWeek(weekday.Groups[2].Value);
            int delta = ((int)target - (int)_now.DayOfWeek + 7) % 7;
            if (next)
                delta = delta == 0 ? 7 : delta + 7;
            explicitDate = _now.Date.AddDays(delta);
            work = RemoveOnce(work, weekday);
        }

        // 5. Times of day ("at 3pm", "at 15:00", "3:30 pm", "at noon").
        //    Allowed alongside day-granularity relatives ("in 2 days at 3pm").
        if (relativeDue == null || relativeIsDayGranularity)
        {
            if (TryParseTimeToken(work, out TimeSpan timeOfDay, out Match timeMatch))
            {
                explicitTime = timeOfDay;
                work = RemoveOnce(work, timeMatch);
            }
        }

        // 6. Calendar dates ("on June 5", "June 5th", "5th of June", "6/5", "6/5/2027").
        if (relativeDue == null && explicitDate == null)
        {
            string? monthText = null;
            string? dayText = null;
            Match? dateMatch = null;

            var monthDay = Regex.Match(
                work,
                @"(?:\bon\s+)?\b" + MonthPattern + @"\s+(\d{1,2})(?:st|nd|rd|th)?\b",
                RegexOptions.IgnoreCase);
            if (monthDay.Success)
            {
                // Group 1 = month name, group 2 = day.
                monthText = monthDay.Groups[1].Value;
                dayText = monthDay.Groups[2].Value;
                dateMatch = monthDay;
            }
            else
            {
                var dayFirst = Regex.Match(
                    work,
                    @"\b(\d{1,2})(?:st|nd|rd|th)?\s+of\s+" + MonthPattern + @"\b",
                    RegexOptions.IgnoreCase);
                if (dayFirst.Success)
                {
                    // Group 1 = day, group 2 = month name.
                    dayText = dayFirst.Groups[1].Value;
                    monthText = dayFirst.Groups[2].Value;
                    dateMatch = dayFirst;
                }
            }

            bool dateParsed = false;
            if (dateMatch != null
                && monthText != null && dayText != null
                && int.TryParse(dayText, out int day) && day >= 1 && day <= 31
                && MonthNumber(monthText) is int month && month > 0)
            {
                explicitDate = NextMonthDay(month, day);
                work = RemoveOnce(work, dateMatch);
                dateParsed = true;
            }

            if (!dateParsed)
            {
                var numeric = Regex.Match(work, @"(?:\bon\s+)?\b(\d{1,2})/(\d{1,2})(?:/(\d{2,4}))?\b");
                if (numeric.Success
                    && int.TryParse(numeric.Groups[1].Value, out int m)
                    && int.TryParse(numeric.Groups[2].Value, out int d)
                    && m >= 1 && m <= 12 && d >= 1 && d <= 31)
                {
                    if (numeric.Groups[3].Success && int.TryParse(numeric.Groups[3].Value, out int year))
                    {
                        if (year < 100)
                            year += 2000;
                        explicitDate = SafeDate(year, m, d) ?? NextMonthDay(m, d);
                    }
                    else
                    {
                        explicitDate = NextMonthDay(m, d);
                    }
                    work = RemoveOnce(work, numeric);
                }
            }
        }

        // 7. Combine date and time into the final due value.
        DateTime due;
        if (relativeDue.HasValue)
        {
            due = relativeIsDayGranularity && explicitTime.HasValue
                ? relativeDue.Value.Date + explicitTime.Value
                : relativeDue.Value;
        }
        else if (explicitDate.HasValue && explicitTime.HasValue)
        {
            due = explicitDate.Value.Date + explicitTime.Value;
        }
        else if (explicitDate.HasValue)
        {
            due = explicitDate.Value.Date + new TimeSpan(9, 0, 0); // default: 9:00 AM
            if (due <= _now)
                due = due.AddDays(1);
        }
        else if (explicitTime.HasValue)
        {
            due = _now.Date + explicitTime.Value;
            if (due <= _now)
                due = due.AddDays(1);
        }
        else
        {
            due = TomorrowAtNine();
        }

        // 8. Clean up the remaining text.
        string text = Regex.Replace(work, @"\s+", " ").Trim();
        text = Regex.Replace(text, @"^(to|that)\s+", string.Empty, RegexOptions.IgnoreCase);
        text = Regex.Replace(text, @"\s+at$", string.Empty, RegexOptions.IgnoreCase);
        text = text.Trim(' ', ',', '.', ';', ':', '-', '–', '—', '(', ')', '"', '\'').Trim();
        if (text.Length == 0)
            text = "Reminder";

        return (due, text);
    }

    private DateTime TomorrowAtNine() => _now.Date.AddDays(1) + new TimeSpan(9, 0, 0);

    private static string RemoveOnce(string source, Match match)
    {
        return source.Substring(0, match.Index) + " " + source.Substring(match.Index + match.Length);
    }

    private static DayOfWeek ParseDayOfWeek(string text)
    {
        switch (text.ToLowerInvariant())
        {
            case "monday":
            case "mon": return DayOfWeek.Monday;
            case "tuesday":
            case "tue": return DayOfWeek.Tuesday;
            case "wednesday":
            case "wed": return DayOfWeek.Wednesday;
            case "thursday":
            case "thu": return DayOfWeek.Thursday;
            case "friday":
            case "fri": return DayOfWeek.Friday;
            case "saturday":
            case "sat": return DayOfWeek.Saturday;
            default: return DayOfWeek.Sunday;
        }
    }

    private static int MonthNumber(string text)
    {
        string lower = text.ToLowerInvariant();
        for (int i = 0; i < MonthNames.Length; i++)
        {
            if (MonthNames[i] == lower || MonthNames[i].StartsWith(lower) || lower.StartsWith(MonthNames[i].Substring(0, 3)))
                return i + 1;
        }
        return 0;
    }

    private DateTime NextMonthDay(int month, int day)
    {
        int year = _now.Year;
        int safeDay = Math.Min(day, DateTime.DaysInMonth(year, month));
        var candidate = new DateTime(year, month, safeDay);
        if (candidate < _now.Date)
        {
            int nextYear = year + 1;
            safeDay = Math.Min(day, DateTime.DaysInMonth(nextYear, month));
            candidate = new DateTime(nextYear, month, safeDay);
        }
        return candidate.Date;
    }

    private static DateTime? SafeDate(int year, int month, int day)
    {
        if (year < 1 || year > 9999)
            return null;
        int safeDay = Math.Min(day, DateTime.DaysInMonth(year, month));
        return new DateTime(year, month, safeDay);
    }

    private static bool TryParseTimeToken(string work, out TimeSpan time, out Match match)
    {
        var m = Regex.Match(work, @"\bat\s+(noon|midnight)\b", RegexOptions.IgnoreCase);
        if (m.Success)
        {
            time = m.Groups[1].Value.Equals("noon", StringComparison.OrdinalIgnoreCase)
                ? new TimeSpan(12, 0, 0)
                : TimeSpan.Zero;
            match = m;
            return true;
        }

        m = Regex.Match(work, @"\bat\s+(\d{1,2})(?::(\d{2}))?\s*(am|pm|a\.m\.|p\.m\.)", RegexOptions.IgnoreCase);
        if (m.Success && TryBuildTime(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, out time))
        {
            match = m;
            return true;
        }

        m = Regex.Match(work, @"\bat\s+(\d{1,2}):(\d{2})\b", RegexOptions.IgnoreCase);
        if (m.Success && TryBuildTime(m.Groups[1].Value, m.Groups[2].Value, null, out time))
        {
            match = m;
            return true;
        }

        m = Regex.Match(work, @"\b(\d{1,2})(?::(\d{2}))?\s*(am|pm|a\.m\.|p\.m\.)", RegexOptions.IgnoreCase);
        if (m.Success && TryBuildTime(m.Groups[1].Value, m.Groups[2].Value, m.Groups[3].Value, out time))
        {
            match = m;
            return true;
        }

        m = Regex.Match(work, @"\b([01]?\d|2[0-3]):([0-5]\d)\b");
        if (m.Success && TryBuildTime(m.Groups[1].Value, m.Groups[2].Value, null, out time))
        {
            match = m;
            return true;
        }

        time = default;
        match = null!;
        return false;
    }

    private static bool TryBuildTime(string hourText, string minuteText, string? meridiem, out TimeSpan time)
    {
        time = default;
        if (!int.TryParse(hourText, out int hour))
            return false;
        int minute = 0;
        if (minuteText.Length > 0 && !int.TryParse(minuteText, out minute))
            return false;
        if (minute < 0 || minute > 59)
            return false;

        if (!string.IsNullOrEmpty(meridiem))
        {
            if (hour < 1 || hour > 12)
                return false;
            string m = meridiem.Replace(".", string.Empty, StringComparison.Ordinal).ToLowerInvariant();
            if (m == "pm" && hour != 12)
                hour += 12;
            else if (m == "am" && hour == 12)
                hour = 0;
        }
        else if (hour < 0 || hour > 23)
        {
            return false;
        }

        time = new TimeSpan(hour, minute, 0);
        return true;
    }
}
