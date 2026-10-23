namespace Tools365.Business.CncFeedSpeedCalculator;

internal sealed record CncFeedSpeedResult(double SpindleSpeedRpm, double FeedRate, string FeedUnit, string ErrorMessage = "")
{
    public bool IsValid => ErrorMessage.Length == 0;
}

internal static class CncFeedSpeedCalculator
{
    public static CncFeedSpeedResult Calculate(
        bool isMetric,
        double cuttingSpeed,
        double cutterDiameter,
        double fluteCount,
        double chipLoad)
    {
        if (!IsPositiveFinite(cuttingSpeed))
        {
            return Invalid("Cutting speed must be a positive number.");
        }

        if (!IsPositiveFinite(cutterDiameter))
        {
            return Invalid("Cutter diameter must be a positive number.");
        }

        if (!double.IsFinite(fluteCount) || fluteCount < 1 || fluteCount > 100 || fluteCount != Math.Truncate(fluteCount))
        {
            return Invalid("Flute count must be a whole number from 1 to 100.");
        }

        if (!IsPositiveFinite(chipLoad))
        {
            return Invalid("Chip load must be a positive number.");
        }

        var spindleSpeed = isMetric
            ? cuttingSpeed * 1000 / (Math.PI * cutterDiameter)
            : cuttingSpeed * 12 / (Math.PI * cutterDiameter);
        var feedRate = spindleSpeed * fluteCount * chipLoad;

        if (!double.IsFinite(spindleSpeed) || !double.IsFinite(feedRate))
        {
            return Invalid("These values are too large to calculate. Check the inputs.");
        }

        return new CncFeedSpeedResult(
            spindleSpeed,
            feedRate,
            isMetric ? "mm/min" : "in/min");
    }

    private static bool IsPositiveFinite(double value) =>
        double.IsFinite(value) && value > 0;

    private static CncFeedSpeedResult Invalid(string message) =>
        new(0, 0, string.Empty, message);
}
