using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using Tools365.Education;
using Tools365.Business;
using Tools365.Family;
using Tools365.Family.ElectricityCostEstimator;
using Tools365.Finance;
using Tools365.Health;
using Tools365.Technology;
using Tools365.Utility;
using Tools365.Utility.AgeCalculator;

namespace Tools365.Pages;

public sealed partial class CategoryPage : Page
{
    public ObservableCollection<ToolCard> Cards { get; } = new();
    private BmiCalculatorWindow? _bmiWindow;
    private AgeCalculatorWindow? _ageCalculatorWindow;
    private ElectricityCostEstimatorWindow? _electricityCostEstimatorWindow;
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
            if (_bmiWindow is null)
            {
                _bmiWindow = new BmiCalculatorWindow();
                _bmiWindow.Closed += BmiWindow_Closed;
            }

            _bmiWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Age calculator" } })
        {
            if (_ageCalculatorWindow is null)
            {
                _ageCalculatorWindow = new AgeCalculatorWindow();
                _ageCalculatorWindow.Closed += AgeCalculatorWindow_Closed;
            }

            _ageCalculatorWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Electricity cost estimator" } })
        {
            if (_electricityCostEstimatorWindow is null)
            {
                _electricityCostEstimatorWindow = new ElectricityCostEstimatorWindow();
                _electricityCostEstimatorWindow.Closed += ElectricityCostEstimatorWindow_Closed;
            }

            _electricityCostEstimatorWindow.Activate();
        }
    }

    private void BmiWindow_Closed(object sender, WindowEventArgs e) =>
        _bmiWindow = null;

    private void AgeCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _ageCalculatorWindow = null;

    private void ElectricityCostEstimatorWindow_Closed(object sender, WindowEventArgs e) =>
        _electricityCostEstimatorWindow = null;

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
            border.Background = (SolidColorBrush)Resources["CategoryCardHoverBrush"];
        }
    }

    private void ToolCard_PointerExited(object sender, PointerRoutedEventArgs e)
    {
        if (sender is Border border)
        {
            border.Background = (SolidColorBrush)Resources[border.Tag as string == "SubCard"
                ? "CategorySubCardBackgroundBrush"
                : "CategoryCardBackgroundBrush"];
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
        ((SolidColorBrush)Resources["CategoryCardHoverBrush"]).Color = CategoryColorPalette.Lighten(color, 0.78);
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
            "Business" => BusinessToolCatalog.GetCards(),
            _ => Array.Empty<ToolCard>()
        };
    }
}
