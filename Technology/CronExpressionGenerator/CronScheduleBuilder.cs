using System.Globalization;

namespace Tools365.Technology.CronExpressionGenerator;

internal sealed record CronScheduleResult(string Expression, string Description, string ErrorMessage = "")
{
    public bool IsValid => ErrorMessage.Length == 0;
}

internal static class CronScheduleBuilder
{
    private static readonly string[] WeekdayNames =
    [
        "Sunday",
        "Monday",
        "Tuesday",
        "Wednesday",
        "Thursday",
        "Friday",
        "Saturday"
    ];

    public static CronScheduleResult Build(
        int scheduleIndex,
        double interval,
        double hour,
        double minute,
        int weekday,
        double monthDay)
    {
        if (scheduleIndex is < 0 or > 6)
        {
            return Invalid("Choose a supported schedule.");
        }

        if (scheduleIndex == 0)
        {
            return Valid("* * * * *", "Runs every minute.");
        }

        if (scheduleIndex == 1)
        {
            if (!IsWholeNumberInRange(interval, 1, 59))
            {
                return Invalid("The minute interval must be a whole number from 1 to 59.");
            }

            var intervalText = Format(interval);
            return Valid($"*/{intervalText} * * * *", $"Runs every {intervalText} minutes.");
        }

        if (!IsWholeNumberInRange(hour, 0, 23) || !IsWholeNumberInRange(minute, 0, 59))
        {
            return Invalid("Hour must be a whole number from 0 to 23 and minute from 0 to 59.");
        }

        var hourText = Format(hour);
        var minuteText = Format(minute);
        var time = $"{hourText.PadLeft(2, '0')}:{minuteText.PadLeft(2, '0')}";

        return scheduleIndex switch
        {
            2 => Valid($"{minuteText} * * * *", $"Runs at minute {minuteText} of every hour."),
            3 => Valid($"{minuteText} {hourText} * * *", $"Runs every day at {time}."),
            4 => Valid($"{minuteText} {hourText} * * 1-5", $"Runs every weekday (Monday–Friday) at {time}."),
            5 when weekday is >= 0 and < 7 =>
                Valid($"{minuteText} {hourText} * * {weekday}", $"Runs every {WeekdayNames[weekday]} at {time}."),
            5 => Invalid("Choose a day of the week."),
            6 when IsWholeNumberInRange(monthDay, 1, 31) =>
                Valid($"{minuteText} {hourText} {Format(monthDay)} * *", $"Runs on day {Format(monthDay)} of every month at {time}."),
            6 => Invalid("The day of the month must be a whole number from 1 to 31."),
            _ => Invalid("Choose a supported schedule.")
        };
    }

    private static bool IsWholeNumberInRange(double value, int minimum, int maximum) =>
        double.IsFinite(value) &&
        value >= minimum &&
        value <= maximum &&
        value == Math.Truncate(value);

    private static string Format(double value) => value.ToString("0", CultureInfo.InvariantCulture);

    private static CronScheduleResult Valid(string expression, string description) =>
        new(expression, description);

    private static CronScheduleResult Invalid(string message) =>
        new(string.Empty, string.Empty, message);
}
