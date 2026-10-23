using System.Text.RegularExpressions;

namespace Tools365.Business.FormatValidator;

internal sealed record IdentifierValidationResult(bool IsValid, string Message);

internal static partial class BusinessIdentifierValidator
{
    public static IdentifierValidationResult ValidateGstin(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return GstinRegex().IsMatch(normalized)
            ? Valid("GSTIN format looks valid.")
            : Invalid("Enter a 15-character GSTIN in the expected format.");
    }

    public static IdentifierValidationResult ValidatePan(string value)
    {
        var normalized = value.Trim().ToUpperInvariant();
        return PanRegex().IsMatch(normalized)
            ? Valid("PAN format looks valid.")
            : Invalid("Enter a 10-character PAN in the expected format.");
    }

    public static IdentifierValidationResult ValidateAadhaar(string value)
    {
        var normalized = value.Where(character => !char.IsWhiteSpace(character) && character != '-').ToArray();
        return AadhaarRegex().IsMatch(new string(normalized))
            ? Valid("Aadhaar number format looks valid.")
            : Invalid("Enter 12 digits; the first digit must be from 2 to 9. Spaces and hyphens are allowed.");
    }

    [GeneratedRegex(@"^[0-9]{2}[A-Z]{5}[0-9]{4}[A-Z][A-Z0-9]Z[A-Z0-9]$", RegexOptions.CultureInvariant)]
    private static partial Regex GstinRegex();

    [GeneratedRegex(@"^[A-Z]{5}[0-9]{4}[A-Z]$", RegexOptions.CultureInvariant)]
    private static partial Regex PanRegex();

    [GeneratedRegex(@"^[2-9][0-9]{11}$", RegexOptions.CultureInvariant)]
    private static partial Regex AadhaarRegex();

    private static IdentifierValidationResult Valid(string message) => new(true, message);

    private static IdentifierValidationResult Invalid(string message) => new(false, message);
}
