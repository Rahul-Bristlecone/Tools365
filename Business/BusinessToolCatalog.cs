using Tools365.Pages;

namespace Tools365.Business;

public static class BusinessToolCatalog
{
    public static IReadOnlyList<ToolCard> GetCards()
    {
        return new[]
        {
            new ToolCard("Serial Number generator", "Generate serial numbers for products and records."),
            new ToolCard("Product SKU generator", "Generate product stock keeping units."),
            new ToolCard("CNC feed and Speed calculator", "Calculate CNC feed and speed values."),
            new ToolCard("PO generator", "Create purchase orders."),
            new ToolCard("Quotation maker", "Create business quotations."),
            new ToolCard("Invoice maker", "Create invoices for customers."),
            new ToolCard("Format Validator - GST, PAN, AADHAR", "Validate GST, PAN, and AADHAR formats.")
        };
    }
}