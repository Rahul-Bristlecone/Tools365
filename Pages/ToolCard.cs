namespace Tools365.Pages;

public sealed class ToolCard
{
    public ToolCard(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }

    public string Description { get; }
}
