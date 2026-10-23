using System.Globalization;
using System.Text;

namespace Tools365.Business.ProductSkuGenerator;

internal sealed record ProductSkuResult(string Skus, string ErrorMessage = "")
{
    public bool IsValid => ErrorMessage.Length == 0;
}

internal static class ProductSkuBuilder
{
    private const long MaximumSequenceNumber = 999_999_999_999;

    public static ProductSkuResult Build(
        string prefix,
        string categoryCode,
        string productCode,
        string separator,
        double startingNumber,
        double quantity,
        double minimumDigits)
    {
        if (!IsWholeNumberInRange(startingNumber, 0, MaximumSequenceNumber))
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

        if (separator != "-" && separator != "_" && separator.Length != 0)
        {
            return Invalid("Choose a supported separator.");
        }

        var components = new List<string>();
        if (!TryAddCode(prefix, "Prefix", required: false, components, out var error) ||
            !TryAddCode(categoryCode, "Category code", required: false, components, out error) ||
            !TryAddCode(productCode, "Product code", required: true, components, out error))
        {
            return Invalid(error);
        }

        var firstNumber = (long)startingNumber;
        var count = (int)quantity;
        if (firstNumber > MaximumSequenceNumber - (count - 1))
        {
            return Invalid("The generated range must not exceed 999,999,999,999.");
        }

        var digitWidth = (int)minimumDigits;
        var skuPrefix = string.Join(separator, components);
        var skus = new StringBuilder();
        for (var offset = 0; offset < count; offset++)
        {
            if (offset > 0)
            {
                skus.AppendLine();
            }

            if (skuPrefix.Length > 0)
            {
                skus.Append(skuPrefix);
                skus.Append(separator);
            }

            skus.Append((firstNumber + offset).ToString($"D{digitWidth}", CultureInfo.InvariantCulture));
        }

        return new ProductSkuResult(skus.ToString());
    }

    private static bool TryAddCode(
        string value,
        string fieldName,
        bool required,
        ICollection<string> components,
        out string error)
    {
        var code = value.Trim().ToUpperInvariant();
        if (code.Length == 0)
        {
            error = required ? $"{fieldName} is required." : string.Empty;
            return !required;
        }

        if (code.Any(character => !char.IsAsciiLetterOrDigit(character)))
        {
            error = $"{fieldName} must contain only letters and numbers, without spaces.";
            return false;
        }

        components.Add(code);
        error = string.Empty;
        return true;
    }

    private static bool IsWholeNumberInRange(double value, long minimum, long maximum) =>
        double.IsFinite(value) &&
        value >= minimum &&
        value <= maximum &&
        value == Math.Truncate(value);

    private static ProductSkuResult Invalid(string message) =>
        new(string.Empty, message);
}
