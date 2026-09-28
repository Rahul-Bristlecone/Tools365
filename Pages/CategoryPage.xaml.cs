using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
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
    private KubernetesResourceCalculatorWindow? _kubernetesCalculatorWindow;
    private readonly List<PinnedToolWidgetWindow> _pinnedWidgets = new();
    private string _activeCategory = "Family";

    public CategoryPage()
    {
        InitializeComponent();
    }

    protected override void OnNavigatedTo(Microsoft.UI.Xaml.Navigation.NavigationEventArgs e)
    {
        base.OnNavigatedTo(e);
        var category = e.Parameter as string ?? string.Empty;
        _activeCategory = category;
        CategoryTitle.Text = category;
        SetCategoryCardColors(category);
        Cards.Clear();

        foreach (var card in GetCards(category))
        {
            Cards.Add(card);
        }

        EmptyState.Visibility = Cards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void ToolCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ToolCard { Title: "BMI calculator" } })
        {
            _bmiWindow ??= new BmiCalculatorWindow();
            _bmiWindow.Activate();
        }
    }

    private void ToolCard_Tapped(object sender, TappedRoutedEventArgs e)
    {
        ToolCard_Click(sender, e);
    }

    private void KubernetesCard_Tapped(object sender, TappedRoutedEventArgs e)
    {
        _kubernetesCalculatorWindow ??= new KubernetesResourceCalculatorWindow();
        _kubernetesCalculatorWindow.Activate();
    }

    private void ToolCard_PointerEntered(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = new SolidColorBrush(CategoryColorPalette.Lighten(CategoryColorPalette.GetColor(_activeCategory), 0.78));
        }
    }

    private void ToolCard_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            var lightness = border.Tag as string == "SubCard" ? 0.9 : 0.82;
            border.Background = new SolidColorBrush(CategoryColorPalette.Lighten(CategoryColorPalette.GetColor(_activeCategory), lightness));
        }
    }

    private void ExchangeFormats_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ToolCard card })
        {
            card.SwapFormats();
        }
    }

    private void PinButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is not Button { DataContext: ToolCard card })
        {
            return;
        }

        var widget = new PinnedToolWidgetWindow(card);
        _pinnedWidgets.Add(widget);
        widget.Closed += (_, _) => _pinnedWidgets.Remove(widget);
        widget.Activate();
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

    private void SetCategoryCardColors(string category)
    {
        var color = CategoryColorPalette.GetColor(category);
        ((SolidColorBrush)Resources["CategoryCardBackgroundBrush"]).Color = CategoryColorPalette.Lighten(color, 0.82);
        ((SolidColorBrush)Resources["CategorySubCardBackgroundBrush"]).Color = CategoryColorPalette.Lighten(color, 0.9);
        ((SolidColorBrush)Resources["CategoryCardAccentBrush"]).Color = color;
        ((SolidColorBrush)Resources["CategoryCardAccentDepthBrush"]).Color = CategoryColorPalette.Darken(color, 0.18);
    }

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
