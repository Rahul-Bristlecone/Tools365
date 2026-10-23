using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Tools365.Business.CncFeedSpeedCalculator;

public sealed partial class CncFeedSpeedCalculatorWindow : Window
{
    private bool _isMetric = true;

    public CncFeedSpeedCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(720, 820));
        UnitsComboBox.SelectionChanged += UnitsComboBox_SelectionChanged;
        CuttingSpeedBox.ValueChanged += SettingsChanged;
        CutterDiameterBox.ValueChanged += SettingsChanged;
        FluteCountBox.ValueChanged += SettingsChanged;
        ChipLoadBox.ValueChanged += SettingsChanged;
        UpdateUnitLabels();
    }

    private void UnitsComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        var isMetric = UnitsComboBox.SelectedIndex == 0;
        if (isMetric != _isMetric)
        {
            var speedConversion = isMetric ? 1 / 3.280839895 : 3.280839895;
            var lengthConversion = isMetric ? 25.4 : 1 / 25.4;
            CuttingSpeedBox.Value *= speedConversion;
            CutterDiameterBox.Value *= lengthConversion;
            ChipLoadBox.Value *= lengthConversion;
            _isMetric = isMetric;
        }

        UpdateUnitLabels();
        ClearResult();
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        if (UnitsComboBox.SelectedIndex is not (0 or 1))
        {
            ShowValidation("Choose metric or imperial units.");
            return;
        }

        var isMetric = UnitsComboBox.SelectedIndex == 0;
        var result = CncFeedSpeedCalculator.Calculate(
            isMetric,
            CuttingSpeedBox.Value,
            CutterDiameterBox.Value,
            FluteCountBox.Value,
            ChipLoadBox.Value);

        if (!result.IsValid)
        {
            ShowValidation(result.ErrorMessage);
            return;
        }

        ValidationBar.IsOpen = false;
        SpindleSpeedText.Text = result.SpindleSpeedRpm.ToString("N0", CultureInfo.CurrentCulture);
        FeedRateText.Text = result.FeedRate.ToString("N2", CultureInfo.CurrentCulture);
        FeedRateUnitText.Text = result.FeedUnit;
    }

    private void SettingsChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) =>
        ClearResult();

    private void UpdateUnitLabels()
    {
        var isMetric = UnitsComboBox.SelectedIndex == 0;
        CuttingSpeedLabel.Text = isMetric ? "Cutting speed (m/min)" : "Cutting speed (SFM)";
        CutterDiameterLabel.Text = isMetric ? "Cutter diameter (mm)" : "Cutter diameter (in)";
        ChipLoadLabel.Text = isMetric ? "Chip load (mm/tooth)" : "Chip load (in/tooth)";
        FeedRateUnitText.Text = isMetric ? "mm/min" : "in/min";
    }

    private void ClearResult()
    {
        SpindleSpeedText.Text = "—";
        FeedRateText.Text = "—";
        ValidationBar.IsOpen = false;
    }

    private void ShowValidation(string message)
    {
        SpindleSpeedText.Text = "—";
        FeedRateText.Text = "—";
        ValidationBar.Title = "Check the cutting parameters";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }
}
