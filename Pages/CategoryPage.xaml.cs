using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Input;
using Microsoft.UI.Xaml.Media;
using System.Collections.ObjectModel;
using Tools365.Education;
using Tools365.Education.GpaCalculator;
using Tools365.Education.IndianHistoryTimeline;
using Tools365.Education.NeetScoreCalculator;
using Tools365.Education.RadixConverter;
using Tools365.Education.SpaceMissionTimeline;
using Tools365.Education.UnitsConverter;
using Tools365.Business;
using Tools365.Business.SerialNumberGenerator;
using Tools365.Business.ProductSkuGenerator;
using Tools365.Business.CncFeedSpeedCalculator;
using Tools365.Business.PurchaseOrderGenerator;
using Tools365.Business.BusinessDocumentGenerator;
using Tools365.Business.FormatValidator;
using Tools365.Family;
using Tools365.Family.CookingRandomiser;
using Tools365.Family.ElectricityCostEstimator;
using Tools365.Family.FuelCostCalculator;
using Tools365.Family.NakshatraCalculator;
using Tools365.Family.RoomAreaVolumeCalculator;
using Tools365.Family.SolarPanelOutputEstimator;
using Tools365.Family.TripPackingChecklist;
using Tools365.Family.VaccinationReminder;
using Tools365.Family.ApplianceWarrantyTracker;
using Tools365.Family.VikramSamvatConverter;
using Tools365.Finance;
using Tools365.Health;
using Tools365.Health.BodyFatCalculator;
using Tools365.Health.IdealWeightCalculator;
using Tools365.Health.OvulationPeriodTracker;
using Tools365.Technology;
using Tools365.Technology.ApiResponseTimeCalculator;
using Tools365.Technology.CidrCalculator;
using Tools365.Technology.CronExpressionGenerator;
using Tools365.Technology.CurlGenerator;
using Tools365.Technology.DbConnectionStringGenerator;
using Tools365.Technology.JwtDecoder;
using Tools365.Technology.SslCertificateChecker;
using Tools365.Technology.TlsCertificateGenerator;
using Tools365.Technology.YamlJsonConverter;
using Tools365.Utility;
using Tools365.Utility.AgeCalculator;

namespace Tools365.Pages;

public sealed partial class CategoryPage : Page
{
    public ObservableCollection<ToolCard> Cards { get; } = new();
    private IReadOnlyList<ToolCard> _categoryCards = Array.Empty<ToolCard>();
    public bool IsGlobalSearch => _activeCategory == "GlobalSearch";
    private BmiCalculatorWindow? _bmiWindow;
    private BodyFatCalculatorWindow? _bodyFatCalculatorWindow;
    private IdealWeightCalculatorWindow? _idealWeightCalculatorWindow;
    private OvulationPeriodTrackerWindow? _ovulationPeriodTrackerWindow;
    private AgeCalculatorWindow? _ageCalculatorWindow;
    private CagrCalculatorWindow? _cagrCalculatorWindow;
    private EpfCalculatorWindow? _epfCalculatorWindow;
    private GstCalculatorWindow? _gstCalculatorWindow;
    private GpaCalculatorWindow? _gpaCalculatorWindow;
    private UnitsConverterWindow? _unitsConverterWindow;
    private RadixConverterWindow? _radixConverterWindow;
    private NeetScoreCalculatorWindow? _neetScoreCalculatorWindow;
    private IndianHistoryTimelineWindow? _indianHistoryTimelineWindow;
    private SpaceMissionTimelineWindow? _spaceMissionTimelineWindow;
    private ElectricityCostEstimatorWindow? _electricityCostEstimatorWindow;
    private FuelCostCalculatorWindow? _fuelCostCalculatorWindow;
    private SolarPanelOutputEstimatorWindow? _solarPanelOutputEstimatorWindow;
    private VikramSamvatConverterWindow? _vikramSamvatConverterWindow;
    private CookingRandomiserWindow? _cookingRandomiserWindow;
    private TripPackingChecklistWindow? _tripPackingChecklistWindow;
    private VaccinationReminderWindow? _vaccinationReminderWindow;
    private ApplianceWarrantyTrackerWindow? _applianceWarrantyTrackerWindow;
    private NakshatraCalculatorWindow? _nakshatraCalculatorWindow;
    private RoomAreaVolumeCalculatorWindow? _roomAreaVolumeCalculatorWindow;
    private KubernetesResourceCalculatorWindow? _kubernetesCalculatorWindow;
    private CidrCalculatorWindow? _cidrCalculatorWindow;
    private YamlJsonConverterWindow? _yamlJsonConverterWindow;
    private CronExpressionGeneratorWindow? _cronExpressionGeneratorWindow;
    private DbConnectionStringGeneratorWindow? _dbConnectionStringGeneratorWindow;
    private ApiResponseTimeCalculatorWindow? _apiResponseTimeCalculatorWindow;
    private CurlGeneratorWindow? _curlGeneratorWindow;
    private TlsCertificateGeneratorWindow? _tlsCertificateGeneratorWindow;
    private JwtDecoderWindow? _jwtDecoderWindow;
    private SslCertificateCheckerWindow? _sslCertificateCheckerWindow;
    private SerialNumberGeneratorWindow? _serialNumberGeneratorWindow;
    private ProductSkuGeneratorWindow? _productSkuGeneratorWindow;
    private CncFeedSpeedCalculatorWindow? _cncFeedSpeedCalculatorWindow;
    private PurchaseOrderGeneratorWindow? _purchaseOrderGeneratorWindow;
    private BusinessDocumentGeneratorWindow? _quotationMakerWindow;
    private BusinessDocumentGeneratorWindow? _invoiceMakerWindow;
    private BusinessFormatValidatorWindow? _businessFormatValidatorWindow;
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
        SetCategoryCardColors(IsGlobalSearch ? "Family" : category);
        _categoryCards = IsGlobalSearch ? GetAllCards() : GetCards(category);
        UpdateCardSearch(string.Empty);
    }

    public void UpdateGlobalSearch(string searchText, string category)
    {
        if (!IsGlobalSearch)
        {
            return;
        }

        SetCategoryCardColors(category);
        UpdateCardSearch(searchText);
    }

    private void UpdateCardSearch(string searchText)
    {
        IReadOnlyList<ToolCard> matchingCards = string.IsNullOrWhiteSpace(searchText)
            ? _categoryCards
            : _categoryCards.Where(card => MatchesSearch(card, searchText)).ToArray();

        Cards.Clear();
        foreach (var card in matchingCards)
        {
            Cards.Add(card);
        }

        EmptyState.Text = string.IsNullOrWhiteSpace(searchText)
            ? IsGlobalSearch ? "No tools available yet." : "No tools available in this category yet."
            : "No matching tools found.";
        EmptyState.Visibility = matchingCards.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
    }

    private static bool MatchesSearch(ToolCard card, string searchText)
    {
        return card.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || card.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase)
            || card.SubCards.Any(subCard =>
                subCard.Title.Contains(searchText, StringComparison.OrdinalIgnoreCase)
                || subCard.Description.Contains(searchText, StringComparison.OrdinalIgnoreCase));
    }

    private void ToolCard_Click(object sender, RoutedEventArgs e)
    {
        if (sender is FrameworkElement { DataContext: ToolCard { Title: "CIDR calculator" } })
        {
            if (_cidrCalculatorWindow is null)
            {
                _cidrCalculatorWindow = new CidrCalculatorWindow();
                _cidrCalculatorWindow.Closed += CidrCalculatorWindow_Closed;
            }

            _cidrCalculatorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "API response time calculator" } })
        {
            if (_apiResponseTimeCalculatorWindow is null)
            {
                _apiResponseTimeCalculatorWindow = new ApiResponseTimeCalculatorWindow();
                _apiResponseTimeCalculatorWindow.Closed += ApiResponseTimeCalculatorWindow_Closed;
            }

            _apiResponseTimeCalculatorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "CURL Generator" } })
        {
            if (_curlGeneratorWindow is null)
            {
                _curlGeneratorWindow = new CurlGeneratorWindow();
                _curlGeneratorWindow.Closed += CurlGeneratorWindow_Closed;
            }

            _curlGeneratorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "TLS certificate generator" } })
        {
            if (_tlsCertificateGeneratorWindow is null)
            {
                _tlsCertificateGeneratorWindow = new TlsCertificateGeneratorWindow();
                _tlsCertificateGeneratorWindow.Closed += TlsCertificateGeneratorWindow_Closed;
            }

            _tlsCertificateGeneratorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "JWT Decoder" } })
        {
            if (_jwtDecoderWindow is null)
            {
                _jwtDecoderWindow = new JwtDecoderWindow();
                _jwtDecoderWindow.Closed += JwtDecoderWindow_Closed;
            }

            _jwtDecoderWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "SSL Certificate Checker" } })
        {
            if (_sslCertificateCheckerWindow is null)
            {
                _sslCertificateCheckerWindow = new SslCertificateCheckerWindow();
                _sslCertificateCheckerWindow.Closed += SslCertificateCheckerWindow_Closed;
            }

            _sslCertificateCheckerWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "Serial Number generator" } })
        {
            if (_serialNumberGeneratorWindow is null)
            {
                _serialNumberGeneratorWindow = new SerialNumberGeneratorWindow();
                _serialNumberGeneratorWindow.Closed += SerialNumberGeneratorWindow_Closed;
            }

            _serialNumberGeneratorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "Product SKU generator" } })
        {
            if (_productSkuGeneratorWindow is null)
            {
                _productSkuGeneratorWindow = new ProductSkuGeneratorWindow();
                _productSkuGeneratorWindow.Closed += ProductSkuGeneratorWindow_Closed;
            }

            _productSkuGeneratorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "CNC feed and Speed calculator" } })
        {
            if (_cncFeedSpeedCalculatorWindow is null)
            {
                _cncFeedSpeedCalculatorWindow = new CncFeedSpeedCalculatorWindow();
                _cncFeedSpeedCalculatorWindow.Closed += CncFeedSpeedCalculatorWindow_Closed;
            }

            _cncFeedSpeedCalculatorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "PO generator" } })
        {
            if (_purchaseOrderGeneratorWindow is null)
            {
                _purchaseOrderGeneratorWindow = new PurchaseOrderGeneratorWindow();
                _purchaseOrderGeneratorWindow.Closed += PurchaseOrderGeneratorWindow_Closed;
            }

            _purchaseOrderGeneratorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "Quotation maker" } })
        {
            if (_quotationMakerWindow is null)
            {
                _quotationMakerWindow = new BusinessDocumentGeneratorWindow(BusinessDocumentType.Quotation);
                _quotationMakerWindow.Closed += QuotationMakerWindow_Closed;
            }

            _quotationMakerWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "Invoice maker" } })
        {
            if (_invoiceMakerWindow is null)
            {
                _invoiceMakerWindow = new BusinessDocumentGeneratorWindow(BusinessDocumentType.Invoice);
                _invoiceMakerWindow.Closed += InvoiceMakerWindow_Closed;
            }

            _invoiceMakerWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "Format Validator - GST, PAN, AADHAR" } })
        {
            if (_businessFormatValidatorWindow is null)
            {
                _businessFormatValidatorWindow = new BusinessFormatValidatorWindow();
                _businessFormatValidatorWindow.Closed += BusinessFormatValidatorWindow_Closed;
            }

            _businessFormatValidatorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "DB connection string generator" } })
        {
            if (_dbConnectionStringGeneratorWindow is null)
            {
                _dbConnectionStringGeneratorWindow = new DbConnectionStringGeneratorWindow();
                _dbConnectionStringGeneratorWindow.Closed += DbConnectionStringGeneratorWindow_Closed;
            }

            _dbConnectionStringGeneratorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard { Title: "Cron expression generator" } })
        {
            if (_cronExpressionGeneratorWindow is null)
            {
                _cronExpressionGeneratorWindow = new CronExpressionGeneratorWindow();
                _cronExpressionGeneratorWindow.Closed += CronExpressionGeneratorWindow_Closed;
            }

            _cronExpressionGeneratorWindow.Activate();
            return;
        }

        if (sender is FrameworkElement { DataContext: ToolCard card } &&
            card.Title == "YAML / JSON converter")
        {
            if (_yamlJsonConverterWindow is null)
            {
                _yamlJsonConverterWindow = new YamlJsonConverterWindow();
                _yamlJsonConverterWindow.Closed += YamlJsonConverterWindow_Closed;
            }

            _yamlJsonConverterWindow.SetSourceFormat(card.SourceFormat);
            _yamlJsonConverterWindow.Activate();
            return;
        }

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
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Ideal weight calculator" } })
        {
            if (_idealWeightCalculatorWindow is null)
            {
                _idealWeightCalculatorWindow = new IdealWeightCalculatorWindow();
                _idealWeightCalculatorWindow.Closed += IdealWeightCalculatorWindow_Closed;
            }

            _idealWeightCalculatorWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Ovulation | Period tracker" } })
        {
            if (_ovulationPeriodTrackerWindow is null)
            {
                _ovulationPeriodTrackerWindow = new OvulationPeriodTrackerWindow();
                _ovulationPeriodTrackerWindow.Closed += OvulationPeriodTrackerWindow_Closed;
            }

            _ovulationPeriodTrackerWindow.Activate();
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
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "GPA Calculator" } })
        {
            if (_gpaCalculatorWindow is null)
            {
                _gpaCalculatorWindow = new GpaCalculatorWindow();
                _gpaCalculatorWindow.Closed += GpaCalculatorWindow_Closed;
            }

            _gpaCalculatorWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Units Converter" } })
        {
            if (_unitsConverterWindow is null)
            {
                _unitsConverterWindow = new UnitsConverterWindow();
                _unitsConverterWindow.Closed += UnitsConverterWindow_Closed;
            }

            _unitsConverterWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Radix (Base) Converter" } })
        {
            if (_radixConverterWindow is null)
            {
                _radixConverterWindow = new RadixConverterWindow();
                _radixConverterWindow.Closed += RadixConverterWindow_Closed;
            }

            _radixConverterWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "NEET Score calculator" } })
        {
            if (_neetScoreCalculatorWindow is null)
            {
                _neetScoreCalculatorWindow = new NeetScoreCalculatorWindow();
                _neetScoreCalculatorWindow.Closed += NeetScoreCalculatorWindow_Closed;
            }

            _neetScoreCalculatorWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Indian history timeline" } })
        {
            if (_indianHistoryTimelineWindow is null)
            {
                _indianHistoryTimelineWindow = new IndianHistoryTimelineWindow();
                _indianHistoryTimelineWindow.Closed += IndianHistoryTimelineWindow_Closed;
            }

            _indianHistoryTimelineWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "ISRO and Space mission timeline" } })
        {
            if (_spaceMissionTimelineWindow is null)
            {
                _spaceMissionTimelineWindow = new SpaceMissionTimelineWindow();
                _spaceMissionTimelineWindow.Closed += SpaceMissionTimelineWindow_Closed;
            }

            _spaceMissionTimelineWindow.Activate();
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
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Vaccination Reminder (India)" } })
        {
            if (_vaccinationReminderWindow is null)
            {
                _vaccinationReminderWindow = new VaccinationReminderWindow();
                _vaccinationReminderWindow.Closed += VaccinationReminderWindow_Closed;
            }

            _vaccinationReminderWindow.Activate();
        }
        else if (sender is FrameworkElement { DataContext: ToolCard { Title: "Appliance Warranty Tracker" } })
        {
            if (_applianceWarrantyTrackerWindow is null)
            {
                _applianceWarrantyTrackerWindow = new ApplianceWarrantyTrackerWindow();
                _applianceWarrantyTrackerWindow.Closed += ApplianceWarrantyTrackerWindow_Closed;
            }

            _applianceWarrantyTrackerWindow.Activate();
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

    private void IdealWeightCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _idealWeightCalculatorWindow = null;

    private void OvulationPeriodTrackerWindow_Closed(object sender, WindowEventArgs e) =>
        _ovulationPeriodTrackerWindow = null;

    private void AgeCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _ageCalculatorWindow = null;

    private void CagrCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _cagrCalculatorWindow = null;

    private void EpfCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _epfCalculatorWindow = null;

    private void GstCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _gstCalculatorWindow = null;

    private void GpaCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _gpaCalculatorWindow = null;

    private void UnitsConverterWindow_Closed(object sender, WindowEventArgs e) =>
        _unitsConverterWindow = null;

    private void RadixConverterWindow_Closed(object sender, WindowEventArgs e) =>
        _radixConverterWindow = null;

    private void NeetScoreCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _neetScoreCalculatorWindow = null;

    private void IndianHistoryTimelineWindow_Closed(object sender, WindowEventArgs e) =>
        _indianHistoryTimelineWindow = null;

    private void SpaceMissionTimelineWindow_Closed(object sender, WindowEventArgs e) =>
        _spaceMissionTimelineWindow = null;

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

    private void VaccinationReminderWindow_Closed(object sender, WindowEventArgs e) =>
        _vaccinationReminderWindow = null;

    private void ApplianceWarrantyTrackerWindow_Closed(object sender, WindowEventArgs e) =>
        _applianceWarrantyTrackerWindow = null;

    private void NakshatraCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _nakshatraCalculatorWindow = null;

    private void RoomAreaVolumeCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _roomAreaVolumeCalculatorWindow = null;

    private void YamlJsonConverterWindow_Closed(object sender, WindowEventArgs e) =>
        _yamlJsonConverterWindow = null;

    private void CronExpressionGeneratorWindow_Closed(object sender, WindowEventArgs e) =>
        _cronExpressionGeneratorWindow = null;

    private void DbConnectionStringGeneratorWindow_Closed(object sender, WindowEventArgs e) =>
        _dbConnectionStringGeneratorWindow = null;

    private void ApiResponseTimeCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _apiResponseTimeCalculatorWindow = null;

    private void CidrCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _cidrCalculatorWindow = null;

    private void CurlGeneratorWindow_Closed(object sender, WindowEventArgs e) =>
        _curlGeneratorWindow = null;

    private void TlsCertificateGeneratorWindow_Closed(object sender, WindowEventArgs e) =>
        _tlsCertificateGeneratorWindow = null;

    private void JwtDecoderWindow_Closed(object sender, WindowEventArgs e) =>
        _jwtDecoderWindow = null;

    private void SslCertificateCheckerWindow_Closed(object sender, WindowEventArgs e) =>
        _sslCertificateCheckerWindow = null;

    private void SerialNumberGeneratorWindow_Closed(object sender, WindowEventArgs e) =>
        _serialNumberGeneratorWindow = null;

    private void ProductSkuGeneratorWindow_Closed(object sender, WindowEventArgs e) =>
        _productSkuGeneratorWindow = null;

    private void CncFeedSpeedCalculatorWindow_Closed(object sender, WindowEventArgs e) =>
        _cncFeedSpeedCalculatorWindow = null;

    private void PurchaseOrderGeneratorWindow_Closed(object sender, WindowEventArgs e) =>
        _purchaseOrderGeneratorWindow = null;

    private void QuotationMakerWindow_Closed(object sender, WindowEventArgs e) =>
        _quotationMakerWindow = null;

    private void InvoiceMakerWindow_Closed(object sender, WindowEventArgs e) =>
        _invoiceMakerWindow = null;

    private void BusinessFormatValidatorWindow_Closed(object sender, WindowEventArgs e) =>
        _businessFormatValidatorWindow = null;

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

    private static IReadOnlyList<ToolCard> GetAllCards()
    {
        return new[] { "Family", "Health", "Finance", "Utility", "Education", "Technology", "Business" }
            .SelectMany(GetCards)
            .ToArray();
    }
}
