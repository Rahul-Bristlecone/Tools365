using Tools365.Pages;

namespace Tools365.Utility;

public static class UtilityToolCatalog
{
    public static IReadOnlyList<ToolCard> GetCards()
    {
        return new[]
        {
            new ToolCard("Age calculator", "Calculate your age from your date of birth.")
        };
    }
}
