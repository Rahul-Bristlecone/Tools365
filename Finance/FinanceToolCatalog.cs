using Tools365.Pages;

namespace Tools365.Finance;

public static class FinanceToolCatalog
{
    public static IReadOnlyList<ToolCard> GetCards()
    {
        return new[]
        {
            new ToolCard("CAGR calculator", "Calculate the compound annual growth rate of an investment."),
            new ToolCard("EPF calculator", "Estimate employee provident fund contributions and maturity value."),
            new ToolCard("GST calculator", "Calculate GST-inclusive and GST-exclusive amounts.")
        };
    }
}
