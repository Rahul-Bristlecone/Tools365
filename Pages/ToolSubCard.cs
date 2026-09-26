namespace Tools365.Pages;

public sealed class ToolSubCard
{
    public ToolSubCard(string title, string description)
    {
        Title = title;
        Description = description;
    }

    public string Title { get; }

    public string Description { get; }
}