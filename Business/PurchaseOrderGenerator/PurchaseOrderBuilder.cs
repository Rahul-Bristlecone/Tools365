using System.Globalization;
using System.Text;

namespace Tools365.Business.PurchaseOrderGenerator;

internal sealed record PurchaseOrderLine(string Description, double Quantity, double UnitPrice);

internal sealed record PurchaseOrderResult(
    string Document,
    decimal Subtotal,
    decimal TaxAmount,
    decimal Total,
    string ErrorMessage = "")
{
    public bool IsValid => ErrorMessage.Length == 0;
}

internal static class PurchaseOrderBuilder
{
    public static PurchaseOrderResult Build(
        string purchaseOrderNumber,
        string supplier,
        string buyer,
        DateTimeOffset? orderDate,
        DateTimeOffset? deliveryDate,
        string currencyCode,
        double taxRate,
        IReadOnlyList<PurchaseOrderLine> lines)
    {
        var number = purchaseOrderNumber.Trim();
        var vendor = supplier.Trim();
        if (number.Length == 0)
        {
            return Invalid("Purchase order number is required.");
        }

        if (vendor.Length == 0)
        {
            return Invalid("Supplier name is required.");
        }

        if (orderDate is null)
        {
            return Invalid("Choose an order date.");
        }

        if (deliveryDate is not null && deliveryDate.Value.Date < orderDate.Value.Date)
        {
            return Invalid("The delivery date cannot be earlier than the order date.");
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
            return Invalid("A purchase order must contain between 1 and 100 line items.");
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
                subtotal += (decimal)line.Quantity * (decimal)line.UnitPrice;
            }
            catch (OverflowException)
            {
                return Invalid("The order amounts are too large to calculate.");
            }
        }

        decimal taxAmount;
        try
        {
            taxAmount = decimal.Round(subtotal * (decimal)taxRate / 100m, 2, MidpointRounding.AwayFromZero);
        }
        catch (OverflowException)
        {
            return Invalid("The tax amount is too large to calculate.");
        }

        decimal total;
        try
        {
            total = subtotal + taxAmount;
        }
        catch (OverflowException)
        {
            return Invalid("The order total is too large to calculate.");
        }

        var culture = GetCurrencyCulture(currencyCode);
        var document = new StringBuilder();
        document.AppendLine("PURCHASE ORDER");
        document.AppendLine($"PO number: {number}");
        document.AppendLine($"Supplier: {vendor}");
        if (!string.IsNullOrWhiteSpace(buyer))
        {
            document.AppendLine($"Buyer: {buyer.Trim()}");
        }

        document.AppendLine($"Order date: {orderDate.Value:yyyy-MM-dd}");
        if (deliveryDate is not null)
        {
            document.AppendLine($"Requested delivery date: {deliveryDate.Value:yyyy-MM-dd}");
        }

        document.AppendLine();
        document.AppendLine("ITEMS");
        document.AppendLine("Description | Quantity | Unit price | Line total");
        foreach (var line in lines)
        {
            var lineTotal = (decimal)line.Quantity * (decimal)line.UnitPrice;
            document.AppendLine(
                $"{line.Description.Trim()} | {line.Quantity.ToString("0.##", CultureInfo.InvariantCulture)} | " +
                $"{line.UnitPrice.ToString("N2", culture)} | {lineTotal.ToString("N2", culture)}");
        }

        document.AppendLine();
        document.AppendLine($"Subtotal: {subtotal.ToString("N2", culture)} {currencyCode}");
        document.AppendLine($"Tax ({taxRate.ToString("0.##", CultureInfo.InvariantCulture)}%): {taxAmount.ToString("N2", culture)} {currencyCode}");
        document.AppendLine($"TOTAL: {total.ToString("N2", culture)} {currencyCode}");

        return new PurchaseOrderResult(document.ToString(), subtotal, taxAmount, total);
    }

    private static CultureInfo GetCurrencyCulture(string currencyCode) =>
        currencyCode switch
        {
            "INR" => CultureInfo.GetCultureInfo("en-IN"),
            "EUR" => CultureInfo.GetCultureInfo("de-DE"),
            "GBP" => CultureInfo.GetCultureInfo("en-GB"),
            _ => CultureInfo.GetCultureInfo("en-US")
        };

    private static PurchaseOrderResult Invalid(string message) =>
        new(string.Empty, 0, 0, 0, message);
}
