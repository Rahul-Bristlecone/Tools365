using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using Tools365.Education;
using Tools365.Business;
using Tools365.Family;
using Tools365.Family.CookingRandomiser;
using Tools365.Family.ElectricityCostEstimator;
using Tools365.Family.FuelCostCalculator;
using Tools365.Family.NakshatraCalculator;
using Tools365.Family.RoomAreaVolumeCalculator;
using Tools365.Family.SolarPanelOutputEstimator;
using Tools365.Family.TripPackingChecklist;
using Tools365.Family.VikramSamvatConverter;
using Tools365.Finance;
using Tools365.Health;
using Tools365.Health.BodyFatCalculator;
using Tools365.Technology;
using Tools365.Utility;
using Tools365.Utility.AgeCalculator;

namespace Tools365.Pages;

public sealed partial class CategoryPage : Page
{
    public ObservableCollection<ToolCard> Cards { get; } = new();
    private BmiCalculatorWindow? _bmiWindow;
    private BodyFatCalculatorWindow? _bodyFatCalculatorWindow;
    private AgeCalculatorWindow? _ageCalculatorWindow;
    private CagrCalculatorWindow? _cagrCalculatorWindow;
    private EpfCalculatorWindow? _epfCalculatorWindow;
    private GstCalculatorWindow? _gstCalculatorWindow;
    private ElectricityCostEstimatorWindow? _electricityCostEstimatorWindow;
    private FuelCostCalculatorWindow? _fuelCostCalculatorWindow;
    private SolarPanelOutputEstimatorWindow? _solarPanelOutputEstimatorWindow;
    private VikramSamvatConverterWindow? _vikramSamvatConverterWindow;
    private CookingRandomiserWindow? _cookingRandomiserWindow;
    private TripPackingChecklistWindow? _tripPackingChecklistWindow;
    private NakshatraCalculatorWindow? _nakshatraCalculatorWindow;
    private RoomAreaVolumeCalculatorWindow? _roomAreaVolumeCalculatorWindow;
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
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Body Fat calculator" } })
        {
            if (_bodyFatCalculatorWindow is null)
            {
                _bodyFatCalculatorWindow = new BodyFatCalculatorWindow();
                _bodyFatCalculatorWindow.Closed += BodyFatCalculatorWindow_Closed;
            }

            _bodyFatCalculatorWindow.Activate();
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
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "CAGR calculator" } })
        {
            if (_cagrCalculatorWindow is null)
            {
                _cagrCalculatorWindow = new CagrCalculatorWindow();
                _cagrCalculatorWindow.Closed += CagrCalculatorWindow_Closed;
            }

            _cagrCalculatorWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "EPF calculator" } })
        {
            if (_epfCalculatorWindow is null)
            {
                _epfCalculatorWindow = new EpfCalculatorWindow();
                _epfCalculatorWindow.Closed += EpfCalculatorWindow_Closed;
            }

            _epfCalculatorWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "GST calculator" } })
        {
            if (_gstCalculatorWindow is null)
            {
                _gstCalculatorWindow = new GstCalculatorWindow();
                _gstCalculatorWindow.Closed += GstCalculatorWindow_Closed;
            }

            _gstCalculatorWindow.Activate();
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
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Fuel cost calculator" } })
        {
            if (_fuelCostCalculatorWindow is null)
            {
                _fuelCostCalculatorWindow = new FuelCostCalculatorWindow();
                _fuelCostCalculatorWindow.Closed += FuelCostCalculatorWindow_Closed;
            }

            _fuelCostCalculatorWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Solar panel output estimator" } })
        {
            if (_solarPanelOutputEstimatorWindow is null)
            {
                _solarPanelOutputEstimatorWindow = new SolarPanelOutputEstimatorWindow();
                _solarPanelOutputEstimatorWindow.Closed += SolarPanelOutputEstimatorWindow_Closed;
            }

            _solarPanelOutputEstimatorWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Vikram Samvat converter" } })
        {
            if (_vikramSamvatConverterWindow is null)
            {
                _vikramSamvatConverterWindow = new VikramSamvatConverterWindow();
                _vikramSamvatConverterWindow.Closed += VikramSamvatConverterWindow_Closed;
            }

            _vikramSamvatConverterWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Cooking randomiser" } })
        {
            if (_cookingRandomiserWindow is null)
            {
                _cookingRandomiserWindow = new CookingRandomiserWindow();
                _cookingRandomiserWindow.Closed += CookingRandomiserWindow_Closed;
            }

            _cookingRandomiserWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Trip Packing checklist" } })
        {
            if (_tripPackingChecklistWindow is null)
            {
                _tripPackingChecklistWindow = new TripPackingChecklistWindow();
                _tripPackingChecklistWindow.Closed += TripPackingChecklistWindow_Closed;
            }

            _tripPackingChecklistWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Nakshatra calculator" } })
        {
            if (_nakshatraCalculatorWindow is null)
            {
                _nakshatraCalculatorWindow = new NakshatraCalculatorWindow();
                _nakshatraCalculatorWindow.Closed += NakshatraCalculatorWindow_Closed;
            }

            _nakshatraCalculatorWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Room area & volume calculator" } })
        {
            if (_roomAreaVolumeCalculatorWindow is null)
            {
                _roomAreaVolumeCalculatorWindow = new RoomAreaVolumeCalculatorWindow();
                _roomAreaVolumeCalculatorWindow.Closed += RoomAreaVolumeCalculatorWindow_Closed;
            }

            _roomAreaVolumeCalculatorWindow.Activate();
        }
    }

    private void BmiWindow_Closed(object sender, WindowEventArgs e) =>
        _bmiWindow = null;

    private void BodyFatCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _bodyFatCalculatorWindow = null;

    private void AgeCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _ageCalculatorWindow = null;

    private void CagrCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _cagrCalculatorWindow = null;

    private void EpfCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _epfCalculatorWindow = null;

    private void GstCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _gstCalculatorWindow = null;

    private void ElectricityCostEstimatorWindow_Closed(object sender, WindowEventArgs e) =>
        _electricityCostEstimatorWindow = null;

    private void FuelCostCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _fuelCostCalculatorWindow = null;

    private void SolarPanelOutputEstimatorWindow_Closed(object sender, WindowEventArgs e) =>
        _solarPanelOutputEstimatorWindow = null;

    private void VikramSamvatConverterWindow_Closed(object sender, WindowEventArgs e) =>
        _vikramSamvatConverterWindow = null;

    private void CookingRandomiserWindow_Closed(object sender, WindowEventArgs e) =>
        _cookingRandomiserWindow = null;

    private void TripPackingChecklistWindow_Closed(object sender, WindowEventArgs e) =>
        _tripPackingChecklistWindow = null;

    private void NakshatraCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _nakshatraCalculatorWindow = null;

    private void RoomAreaVolumeCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _roomAreaVolumeCalculatorWindow = null;

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
