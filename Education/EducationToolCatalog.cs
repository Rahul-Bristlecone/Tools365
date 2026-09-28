using Tools365.Pages;

namespace Tools365.Education;

public static class EducationToolCatalog
{
    public static IReadOnlyList<ToolCard> GetCards() => new[]
    {
        new ToolCard("GPA Calculator", "Calculate your grade point average."),
        new ToolCard("Units Converter", "Convert values between common units."),
        new ToolCard("Radix (Base) Converter", "Convert numbers between different bases."),
        new ToolCard("Indian history timeline", "Explore key events in Indian history."),
        new ToolCard("NEET Score calculator", "Estimate your NEET examination score."),
        new ToolCard("ISRO and Space mission timeline", "Explore major ISRO and space missions.")
    };
}
