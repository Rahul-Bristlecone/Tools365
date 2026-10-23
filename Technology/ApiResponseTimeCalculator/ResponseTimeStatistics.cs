using System.Globalization;

namespace Tools365.Technology.ApiResponseTimeCalculator;

internal sealed record ResponseTimeStatistics(
    int SampleCount,
    double Minimum,
    double Average,
    double Median,
    double Percentile95,
    double Percentile99,
    double Maximum);

internal static class ResponseTimeAnalyzer
{
    public static bool TryAnalyze(string input, out ResponseTimeStatistics? statistics, out string error)
    {
        statistics = null;
        var values = new List<double>();
        foreach (var token in input.Split([' ', '\t', '\r', '\n', ',', ';'], StringSplitOptions.RemoveEmptyEntries))
        {
            if (!double.TryParse(token, NumberStyles.Float, CultureInfo.InvariantCulture, out var value) ||
                !double.IsFinite(value) ||
                value < 0)
            {
                error = $"“{token}” is not a valid non-negative time in milliseconds.";
                return false;
            }

            values.Add(value);
        }

        if (values.Count == 0)
        {
            error = "Enter one or more response times in milliseconds.";
            return false;
        }

        values.Sort();
        var middle = values.Count / 2;
        var median = values.Count % 2 == 0
            ? (values[middle - 1] + values[middle]) / 2
            : values[middle];
        statistics = new ResponseTimeStatistics(
            values.Count,
            values[0],
            values.Average(),
            median,
            Percentile(values, 0.95),
            Percentile(values, 0.99),
            values[^1]);
        error = string.Empty;
        return true;
    }

    private static double Percentile(IReadOnlyList<double> sortedValues, double percentile)
    {
        var rank = Math.Max(1, (int)Math.Ceiling(percentile * sortedValues.Count));
        return sortedValues[rank - 1];
    }
}
