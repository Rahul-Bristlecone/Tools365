using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Health.BodyFatCalculator;

public sealed partial class BodyFatCalculatorWindow : Window
{
    public BodyFatCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(700, 760));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }

        GenderComboBox.ItemsSource = new[] { "Male", "Female" };
        GenderComboBox.SelectedIndex = 0;
        GenderComboBox.SelectionChanged += GenderComboBox_SelectionChanged;
        UpdateGenderFields();
    }

    private void GenderComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateGenderFields();

    private void UpdateGenderFields()
    {
        var isFemale = GenderComboBox.SelectedIndex == 1;
        HipPanel.Visibility = isFemale ? Visibility.Visible : Visibility.Collapsed;
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;

        if (!TryGetPositiveValue(AgeBox.Value, out var age))
        {
            ShowValidation("Enter a valid age in years.");
            return;
        }

        if (!TryGetPositiveValue(WeightBox.Value, out var weightKg))
        {
            ShowValidation("Enter a positive weight in kilograms.");
            return;
        }

        if (!TryGetPositiveValue(HeightBox.Value, out var heightCm))
        {
            ShowValidation("Enter a positive height in centimetres.");
            return;
        }

        if (!TryGetPositiveValue(WaistBox.Value, out var waistCm))
        {
            ShowValidation("Enter a positive waist measurement in centimetres.");
            return;
        }

        if (!TryGetPositiveValue(NeckBox.Value, out var neckCm))
        {
            ShowValidation("Enter a positive neck measurement in centimetres.");
            return;
        }

        var isFemale = GenderComboBox.SelectedIndex == 1;
        double hipCm = 0;
        if (isFemale && !TryGetPositiveValue(HipBox.Value, out hipCm))
        {
            ShowValidation("Enter a positive hip measurement in centimetres for females.");
            return;
        }

        var heightIn = heightCm / 2.54;
        var waistIn = waistCm / 2.54;
        var neckIn = neckCm / 2.54;
        var hipIn = isFemale ? hipCm / 2.54 : 0.0;

        double bodyFatPercent;
        if (isFemale)
        {
            var femaleFactor = waistIn + hipIn - neckIn;
            if (femaleFactor <= 0)
            {
                ShowValidation("Waist + hip must be greater than neck for the female calculation.");
                return;
            }

            bodyFatPercent = 495.0 / (1.29579 - 0.35004 * Math.Log10(femaleFactor) + 0.221 * Math.Log10(heightIn)) - 450.0;
        }
        else
        {
            var maleFactor = waistIn - neckIn;
            if (maleFactor <= 0)
            {
                ShowValidation("Waist must be greater than neck for the male calculation.");
                return;
            }

            bodyFatPercent = 495.0 / (1.0324 - 0.19077 * Math.Log10(maleFactor) + 0.15456 * Math.Log10(heightIn)) - 450.0;
        }

        if (bodyFatPercent < 0)
        {
            bodyFatPercent = 0;
        }

        var leanMass = CalculateLeanMass(weightKg, bodyFatPercent);

        BodyFatText.Text = $"{bodyFatPercent:N1}%";
        CategoryText.Text = GetBodyFatCategory(bodyFatPercent, isFemale);
        LeanMassText.Text = $"{leanMass:N1} kg";
    }

    private static double CalculateLeanMass(double weightKg, double bodyFatPercent)
    {
        var fatKg = weightKg * (bodyFatPercent / 100.0);
        return weightKg - fatKg;
    }

    private static string GetBodyFatCategory(double bodyFatPercent, bool isFemale)
    {
        if (isFemale)
        {
            return bodyFatPercent switch
            {
                < 14 => "Essential fat",
                < 21 => "Athletic",
                < 24 => "Fit",
                < 31 => "Average",
                _ => "Above average"
            };
        }

        return bodyFatPercent switch
        {
            < 6 => "Essential fat",
            < 14 => "Athletic",
            < 18 => "Fit",
            < 25 => "Average",
            _ => "Above average"
        };
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
}
