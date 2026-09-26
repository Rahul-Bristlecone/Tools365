using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using Tools365.Education;
using Tools365.Family;
using Tools365.Finance;
using Tools365.Health;
using Tools365.Technology;
using Tools365.Utility;

namespace Tools365.Pages;

public sealed partial class CategoryPage : Page
{
    public ObservableCollection<ToolCard> Cards { get; } = new();
    private BmiCalculatorWindow? _bmiWindow;

    public CategoryPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var category = e.Parameter as string ?? string.Empty;
        CategoryTitle.Text = category;
        Cards.Clear();

        foreach (var card in GetCards(category))
        {
            Cards.Add(card);
        }

        EmptyState.Visibility = Cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ToolCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ToolCard { Title: "BMI calculator" } })
        {
            _bmiWindow ??= new BmiCalculatorWindow();
            _bmiWindow.Activate();
        }
    }

    private void ExchangeFormats_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ToolCard card })
        {
            card.SwapFormats();
        }
    }

    private async void InfoButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: ToolCard card })
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = card.Title,
            Content = card.Description,
            CloseButtonText = "Close",
            XamlRoot = XamlRoot
        };

        await dialog.ShowAsync();
    }

    public static Visibility ConverterVisibility(bool isConverter) =>
        isConverter ? Visibility.Visible : Visibility.Collapsed;

    public static Visibility StandardCardVisibility(bool isConverter, bool isKubernetesCalculator) =>
        isConverter || isKubernetesCalculator ? Visibility.Collapsed : Visibility.Visible;

    public static Visibility KubernetesVisibility(bool isKubernetesCalculator) =>
        isKubernetesCalculator ? Visibility.Visible : Visibility.Collapsed;

    private static IReadOnlyList<ToolCard> GetCards(string category)
    {
        return category switch
    {
            "Family" => FamilyToolCatalog.GetCards(),
            "Health" => HealthToolCatalog.GetCards(),
            "Finance" => FinanceToolCatalog.GetCards(),
            "Utility" => UtilityToolCatalog.GetCards(),
            "Education" => EducationToolCatalog.GetCards(),
            "Technology" => TechnologyToolCatalog.GetCards(),
            _ => Array.Empty<ToolCard>()
        };
    }
}
