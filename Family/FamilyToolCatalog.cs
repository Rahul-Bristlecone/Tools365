using Tools365.Pages;

namespace Tools365.Family;

public static class FamilyToolCatalog
{
    public static IReadOnlyList<ToolCard> GetCards() => new[]
    {
        new ToolCard("Fuel cost calculator", "Estimate the fuel cost for a journey."),
        new ToolCard("Electricity cost estimator", "Estimate electricity usage and running costs."),
        new ToolCard("Cooking randomiser", "Pick a random cooking idea."),
        new ToolCard("Room area & volume calculator", "Calculate the area and volume of a room."),
        new ToolCard("Trip Packing checklist", "Organise the items you need for a trip."),
        new ToolCard("Solar panel output estimator", "Estimate energy output from solar panels.")
    };
}
