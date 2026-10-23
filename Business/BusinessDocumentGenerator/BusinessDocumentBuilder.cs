using System.Globalization;
using System.Text;

namespace Tools365.Business.BusinessDocumentGenerator;

public enum BusinessDocumentType
{
    Quotation,
    Invoice
}

internal sealed record BusinessDocumentLine(string Description, double Quantity, double UnitPrice);

internal sealed record BusinessDocumentResult(string Document, decimal Total, string ErrorMessage = "")
{
    public bool IsValid => ErrorMessage.Length == 0;
}

internal static class BusinessDocumentBuilder
{
    public static BusinessDocumentResult Build(
        BusinessDocumentType documentType,
        string documentNumber,
        string seller,
        string customer,
        string terms,
        DateTimeOffset? documentDate,
        DateTimeOffset? dueOrValidUntilDate,
        string currencyCode,
        double taxRate,
        IReadOnlyList<BusinessDocumentLine> lines)
    {
        var number = documentNumber.Trim();
        if (number.Length == 0)
        {
            return Invalid($"{GetNumberLabel(documentType)} is required.");
        }

        if (seller.Trim().Length == 0)
        {
            return Invalid("Your business name is required.");
        }

        if (customer.Trim().Length == 0)
        {
            return Invalid("Customer name is required.");
        }

        if (documentDate is null)
        {
            return Invalid($"Choose the {GetDateLabel(documentType)}.");
        }

        if (dueOrValidUntilDate is not null && dueOrValidUntilDate.Value.Date < documentDate.Value.Date)
        {
            return Invalid($"The {GetEndDateLabel(documentType)} cannot be earlier than the document date.");
        }

        if (currencyCode is not ("INR" or "USD" or "EUR" or "GBP"))
        {
            return Invalid("Choose a supported currency.");
        }

        if (!double.IsFinite(taxRate) || taxRate < 0 || taxRate > 100)
        {
            return Invalid("Tax rate must be between 0% and 100%.");
        }

        if (lines.Count is < 1 or > 100)
        {
            return Invalid("A document must contain between 1 and 100 line items.");
        }

        decimal subtotal = 0;
        foreach (var line in lines)
        {
            if (string.IsNullOrWhiteSpace(line.Description))
            {
                return Invalid("Each line item must have a description.");
            }

            if (!double.IsFinite(line.Quantity) || line.Quantity <= 0)
            {
                return Invalid("Each item quantity must be a positive number.");
            }

            if (!double.IsFinite(line.UnitPrice) || line.UnitPrice < 0)
            {
                return Invalid("Each unit price must be zero or greater.");
            }

            try
            {
                var lineTotal = decimal.Round(
                    (decimal)line.Quantity * (decimal)line.UnitPrice,
                    2,
                    MidpointRounding.AwayFromZero);
                subtotal += lineTotal;
            }
            catch (OverflowException)
            {
                return Invalid("The amounts are too large to calculate.");
            }
        }

        decimal taxAmount;
        decimal total;
        try
        {
            taxAmount = decimal.Round(subtotal * (decimal)taxRate / 100m, 2, MidpointRounding.AwayFromZero);
            total = subtotal + taxAmount;
        }
        catch (OverflowException)
        {
            return Invalid("The order total is too large to calculate.");
        }

        var culture = GetCurrencyCulture(currencyCode);
        var title = documentType == BusinessDocumentType.Invoice ? "INVOICE" : "QUOTATION";
        var dateLabel = documentType == BusinessDocumentType.Invoice ? "Invoice date" : "Quotation date";
        var endDateLabel = documentType == BusinessDocumentType.Invoice ? "Due date" : "Valid until";
        var document = new StringBuilder();
        document.AppendLine(title);
        document.AppendLine($"{GetNumberLabel(documentType)}: {number}");
        document.AppendLine($"From: {seller.Trim()}");
        document.AppendLine($"Customer: {customer.Trim()}");
        document.AppendLine($"{dateLabel}: {documentDate.Value:yyyy-MM-dd}");
        if (dueOrValidUntilDate is not null)
        {
            document.AppendLine($"{endDateLabel}: {dueOrValidUntilDate.Value:yyyy-MM-dd}");
        }

        document.AppendLine();
        document.AppendLine("ITEMS");
        document.AppendLine("Description | Quantity | Unit price | Line total");
        foreach (var line in lines)
        {
            var lineTotal = decimal.Round(
                (decimal)line.Quantity * (decimal)line.UnitPrice,
                2,
                MidpointRounding.AwayFromZero);
            document.AppendLine(
                $"{line.Description.Trim()} | {line.Quantity.ToString("0.##", CultureInfo.InvariantCulture)} | " +
                $"{line.UnitPrice.ToString("N2", culture)} | {lineTotal.ToString("N2", culture)}");
        }

        document.AppendLine();
        document.AppendLine($"Subtotal: {subtotal.ToString("N2", culture)} {currencyCode}");
        document.AppendLine($"Tax ({taxRate.ToString("0.##", CultureInfo.InvariantCulture)}%): {taxAmount.ToString("N2", culture)} {currencyCode}");
        document.AppendLine($"TOTAL: {total.ToString("N2", culture)} {currencyCode}");
        if (!string.IsNullOrWhiteSpace(terms))
        {
            document.AppendLine();
            document.AppendLine($"Terms and notes: {terms.Trim()}");
        }

        return new BusinessDocumentResult(document.ToString(), total);
    }

    private static string GetNumberLabel(BusinessDocumentType type) =>
        type == BusinessDocumentType.Invoice ? "Invoice number" : "Quotation number";

    private static string GetDateLabel(BusinessDocumentType type) =>
        type == BusinessDocumentType.Invoice ? "invoice date" : "quotation date";

    private static string GetEndDateLabel(BusinessDocumentType type) =>
        type == BusinessDocumentType.Invoice ? "due date" : "valid-until date";

    private static CultureInfo GetCurrencyCulture(string currencyCode) =>
        currencyCode switch
        {
            "INR" => CultureInfo.GetCultureInfo("en-IN"),
            "EUR" => CultureInfo.GetCultureInfo("de-DE"),
            "GBP" => CultureInfo.GetCultureInfo("en-GB"),
            _ => CultureInfo.GetCultureInfo("en-US")
        };

    private static BusinessDocumentResult Invalid(string message) =>
        new(string.Empty, 0, message);
}
