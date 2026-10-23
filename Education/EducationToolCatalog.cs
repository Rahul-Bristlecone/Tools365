using Tools365.Pages;

namespace Tools365.Education;

public static class EducationToolCatalog
{
    public static IReadOnlyList<ToolCard> GetCards() => new[]
    {
        new ToolCard("GPA Calculator", "Calculate your credit-weighted grade point average."),
        new ToolCard("Units Converter", "Convert length, mass, temperature, area, volume, time, and speed."),
        new ToolCard("Radix (Base) Converter", "Convert whole numbers between bases 2 and 36."),
        new ToolCard("Indian history timeline", "Explore key events in Indian history."),
        new ToolCard("NEET Score calculator", "Estimate your score using +4 / −1 marking across Physics, Chemistry, Botany, and Zoology."),
        new ToolCard("ISRO and Space mission timeline", "Explore ISRO milestones and landmark global space missions.")
    };
}
