using Microsoft.UI.Xaml;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Family.SolarPanelOutputEstimator;

public sealed partial class SolarPanelOutputEstimatorWindow : Window
{
    private const double DaysPerMonth = 30;
    private const double DaysPerYear = 365;

    public SolarPanelOutputEstimatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(820, 700));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;

        if (!TryGetPositiveValue(SystemSizeBox.Value, out var systemSizeKw))
        {
            ShowValidation("Enter a positive solar system size in kW.");
            return;
        }

        if (!TryGetPositiveValue(SunlightBox.Value, out var sunlightHours))
        {
            ShowValidation("Enter positive daily sunlight hours.");
            return;
        }

        if (!TryGetValidPercentage(LossesBox.Value, out var lossesPercent))
        {
            ShowValidation("System losses must be between 0 and 100 percent.");
            return;
        }

        if (!TryGetPositiveValue(RateBox.Value, out var ratePerKwh))
        {
            ShowValidation("Enter a positive electricity rate in ₹/kWh.");
            return;
        }

        var effectiveFactor = 1m - ((decimal)lossesPercent / 100m);
        var systemSize = (decimal)systemSizeKw;
        var sunlight = (decimal)sunlightHours;
        var rate = (decimal)ratePerKwh;

        var dailyOutput = systemSize * sunlight * effectiveFactor;
        var monthlyOutput = dailyOutput * (decimal)DaysPerMonth;
        var annualOutput = dailyOutput * (decimal)DaysPerYear;
        var monthlySavings = monthlyOutput * rate;
        var annualSavings = annualOutput * rate;

        DailyOutputText.Text = $"{dailyOutput:N2} kWh/day";
        MonthlyOutputText.Text = $"{monthlyOutput:N2} kWh/month";
        AnnualOutputText.Text = $"{annualOutput:N2} kWh/year";
        MonthlySavingsText.Text = FormatRupees(monthlySavings);
        AnnualSavingsText.Text = FormatRupees(annualSavings);
    }

    private void ShowValidation(string message)
    {
        ValidationBar.Title = "Check the inputs";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }

    private static bool TryGetPositiveValue(double value, out double result)
    {
        result = value;
        return !double.IsNaN(value) && value > 0;
    }

    private static bool TryGetValidPercentage(double value, out double result)
    {
        result = value;
        return !double.IsNaN(value) && value >= 0 && value <= 100;
    }

    private static string FormatRupees(decimal amount)
    {
        var culture = CultureInfo.GetCultureInfo("en-IN");
        return $"₹ {amount.ToString("N2", culture)}";
    }
}
