using Microsoft.UI.Dispatching;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Health;

public sealed partial class BmiCalculatorWindow : Window
{
    private bool _isSynchronizing;
    private readonly DispatcherQueueTimer _notificationTimer;

    public BmiCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(520, 620));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

        _notificationTimer = DispatcherQueue.GetForCurrentThread().CreateTimer();
        _notificationTimer.Interval = TimeSpan.FromSeconds(2.5);
        _notificationTimer.Tick += NotificationTimer_Tick;
    }

    private void WeightKilogramsBox_TextChanged(object sender, TextChangedEventArgs args)
    {
        if (_isSynchronizing || !TryGetTextValue(WeightKilogramsBox, out var kilograms))
        {
            return;
        }

        _isSynchronizing = true;
        WeightPoundsBox.Text = (kilograms * 2.2046226218).ToString("0.0", CultureInfo.CurrentCulture);
        _isSynchronizing = false;
    }

    private void WeightPoundsBox_TextChanged(object sender, TextChangedEventArgs args)
    {
        if (_isSynchronizing || !TryGetTextValue(WeightPoundsBox, out var pounds))
        {
            return;
        }

        _isSynchronizing = true;
        WeightKilogramsBox.Text = (pounds * 0.45359237).ToString("0.0", CultureInfo.CurrentCulture);
        _isSynchronizing = false;
    }

    private void HeightCentimetersBox_TextChanged(object sender, TextChangedEventArgs args)
    {
        if (_isSynchronizing || !TryGetTextValue(HeightCentimetersBox, out var centimeters))
        {
            return;
        }

        var totalInches = centimeters / 2.54;
        _isSynchronizing = true;
        HeightFeetBox.Text = Math.Floor(totalInches / 12).ToString("0", CultureInfo.CurrentCulture);
        HeightInchesBox.Text = Math.Round(totalInches % 12, 1).ToString("0.0", CultureInfo.CurrentCulture);
        _isSynchronizing = false;
    }

    private void HeightFeetBox_TextChanged(object sender, TextChangedEventArgs args)
    {
        UpdateHeightCentimetersFromImperial();
    }

    private void HeightInchesBox_TextChanged(object sender, TextChangedEventArgs args)
    {
        UpdateHeightCentimetersFromImperial();
    }

    private void UpdateHeightCentimetersFromImperial()
    {
        if (_isSynchronizing)
        {
            return;
        }

        var feet = GetValue(HeightFeetBox) ?? 0;
        var inches = GetValue(HeightInchesBox) ?? 0;
        if (feet <= 0 && inches <= 0)
        {
            return;
        }

        _isSynchronizing = true;
        HeightCentimetersBox.Text = ((feet * 12 + inches) * 2.54).ToString("0.0", CultureInfo.CurrentCulture);
        _isSynchronizing = false;
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        var weightKilograms = GetValue(WeightKilogramsBox);
        if (!weightKilograms.HasValue)
        {
            var weightPounds = GetValue(WeightPoundsBox);
            if (weightPounds.HasValue)
            {
                weightKilograms = weightPounds.Value * 0.45359237;
            }
        }

        var heightCentimeters = GetValue(HeightCentimetersBox);
        if (!heightCentimeters.HasValue)
        {
            var feet = GetValue(HeightFeetBox) ?? 0;
            var inches = GetValue(HeightInchesBox) ?? 0;
            if (feet > 0 || inches > 0)
            {
                heightCentimeters = (feet * 12 + inches) * 2.54;
            }
        }

        if (!weightKilograms.HasValue || !heightCentimeters.HasValue)
        {
            ShowNotification(
                "Enter a weight and height",
                "Provide kilograms or pounds, and centimeters or feet/inches.",
                InfoBarSeverity.Warning);
            return;
        }

        if (!double.IsFinite(weightKilograms.Value) || weightKilograms.Value <= 0 ||
            !double.IsFinite(heightCentimeters.Value) || heightCentimeters.Value <= 0)
        {
            ShowNotification(
                "Enter valid measurements",
                "Weight and height must be positive numbers.",
                InfoBarSeverity.Warning);
            return;
        }

        var heightMeters = heightCentimeters.Value / 100;
        var bmi = weightKilograms.Value / (heightMeters * heightMeters);
        ShowNotification($"BMI: {bmi:0.0}", GetCategory(bmi), InfoBarSeverity.Success);
    }

    private void ShowNotification(string title, string message, InfoBarSeverity severity)
    {
        ResultBar.Title = title;
        ResultBar.Message = message;
        ResultBar.Severity = severity;
        ResultBar.IsOpen = true;
        _notificationTimer.Stop();
        _notificationTimer.Start();
    }

    private void NotificationTimer_Tick(DispatcherQueueTimer sender, object args)
    {
        sender.Stop();
        ResultBar.IsOpen = false;
    }

    private static bool TryGetTextValue(TextBox textBox, out double value)
    {
        return double.TryParse(
            textBox.Text,
            NumberStyles.Float,
            CultureInfo.CurrentCulture,
            out value);
    }

    private static double? GetValue(TextBox textBox)
    {
        return double.TryParse(textBox.Text, NumberStyles.Float, CultureInfo.CurrentCulture, out var value)
            ? value
            : null;
    }

    private static string GetCategory(double bmi)
    {
        return bmi switch
        {
            < 18.5 => "Below the typical healthy range.",
            < 25 => "Within the typical healthy range.",
            < 30 => "Above the typical healthy range.",
            _ => "In the obesity range by standard BMI categories."
        };
    }
}
