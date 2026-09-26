using System.ComponentModel;
using System.Runtime.CompilerServices;

namespace Tools365.Pages;

public sealed class ToolCard : INotifyPropertyChanged
{
    public ToolCard(
        string title,
        string description,
        bool isConverter = false,
        bool isKubernetesCalculator = false,
        IEnumerable<ToolSubCard>? subCards = null)
    {
        Title = title;
        Description = description;
        IsConverter = isConverter;
        IsKubernetesCalculator = isKubernetesCalculator;
        SubCards = subCards?.ToArray() ?? Array.Empty<ToolSubCard>();
    }

    public string Title { get; }

    public string Description { get; }

    public bool IsConverter { get; }

    public bool IsKubernetesCalculator { get; }

    public IReadOnlyList<ToolSubCard> SubCards { get; }

    public double CardWidth => IsKubernetesCalculator ? 680 : 320;

    public string SourceFormat { get; private set; } = "YAML";

    public string TargetFormat { get; private set; } = "JSON";

    public event PropertyChangedEventHandler? PropertyChanged;

    public void SwapFormats()
    {
        (SourceFormat, TargetFormat) = (TargetFormat, SourceFormat);
        OnPropertyChanged(nameof(SourceFormat));
        OnPropertyChanged(nameof(TargetFormat));
    }

    private void OnPropertyChanged([CallerMemberName] string? propertyName = null)
    {
        PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(propertyName));
    }
}
