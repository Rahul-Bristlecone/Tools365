using Tools365.Pages;

namespace Tools365.Business;

public static class BusinessToolCatalog
{
    public static IReadOnlyList<ToolCard> GetCards()
    {
        return new[]
        {
            new ToolCard("Serial Number generator", "Create formatted serial numbers for products and records."),
            new ToolCard("Product SKU generator", "Create consistent SKUs from product codes and a sequence."),
            new ToolCard("CNC feed and Speed calculator", "Calculate spindle speed and feed rate from cutting parameters."),
            new ToolCard("PO generator", "Create purchase orders with supplier details, line items, and totals."),
            new ToolCard("Quotation maker", "Prepare a customer quotation with line items, tax, and totals."),
            new ToolCard("Invoice maker", "Create a customer invoice with line items, tax, and totals."),
            new ToolCard("Format Validator - GST, PAN, AADHAR", "Check GSTIN, PAN, and Aadhaar number formats locally.")
        };
    }
}