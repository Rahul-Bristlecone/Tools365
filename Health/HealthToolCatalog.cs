using Tools365.Pages;

namespace Tools365.Health;

public static class HealthToolCatalog
{
    public static IReadOnlyList<ToolCard> GetCards()
    {
        return new[]
        {
            new ToolCard("BMI calculator", "Calculate your body mass index."),
            new ToolCard("Body Fat calculator", "Estimate your body fat percentage."),
            new ToolCard("Ideal weight calculator", "Find a healthy target weight range."),
            new ToolCard("Ovulation | fertility | Period calculator", "Track cycle timing and fertile days.")
        };
    }
}
