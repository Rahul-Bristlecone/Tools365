using System.Globalization;
using System.Text;

namespace Tools365.Business.SerialNumberGenerator;

internal sealed record SerialNumberResult(string Numbers, string ErrorMessage = "")
{
    public bool IsValid => ErrorMessage.Length == 0;
}

internal static class SerialNumberBuilder
{
    private const long MaximumSerialNumber = 999_999_999_999;

    public static SerialNumberResult Build(
        string prefix,
        string suffix,
        double startingNumber,
        double quantity,
        double minimumDigits)
    {
        if (!IsWholeNumberInRange(startingNumber, 0, MaximumSerialNumber))
        {
            return Invalid("The starting number must be a whole number from 0 to 999,999,999,999.");
        }

        if (!IsWholeNumberInRange(quantity, 1, 1000))
        {
            return Invalid("The quantity must be a whole number from 1 to 1,000.");
        }

        if (!IsWholeNumberInRange(minimumDigits, 1, 12))
        {
            return Invalid("The number of digits must be a whole number from 1 to 12.");
        }

        if (prefix.Contains('\r') || prefix.Contains('\n') || suffix.Contains('\r') || suffix.Contains('\n'))
        {
            return Invalid("Prefix and suffix must not contain line breaks.");
        }

        var firstNumber = (long)startingNumber;
        var count = (int)quantity;
        if (firstNumber > MaximumSerialNumber - (count - 1))
        {
            return Invalid("The generated range must not exceed 999,999,999,999.");
        }

        var digitWidth = (int)minimumDigits;
        var numbers = new StringBuilder();
        for (var offset = 0; offset < count; offset++)
        {
            if (offset > 0)
            {
                numbers.AppendLine();
            }

            numbers.Append(prefix);
            numbers.Append((firstNumber + offset).ToString($"D{digitWidth}", CultureInfo.InvariantCulture));
            numbers.Append(suffix);
        }

        return new SerialNumberResult(numbers.ToString());
    }

    private static bool IsWholeNumberInRange(double value, long minimum, long maximum) =>
        double.IsFinite(value) &&
        value >= minimum &&
        value <= maximum &&
        value == Math.Truncate(value);

    private static SerialNumberResult Invalid(string message) =>
        new(string.Empty, message);
}
