using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Tools365.Health.IdealWeightCalculator;

public sealed partial class IdealWeightCalculatorWindow : Window
{
    private const double KilogramsPerPound = 0.45359237;
    private const double CentimetersPerInch = 2.54;

    public IdealWeightCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(640, 700));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;

        if (!double.IsFinite(HeightBox.Value) || HeightBox.Value < 120 || HeightBox.Value > 250)
        {
            ShowValidation("Enter a height between 120 and 250 centimetres.");
            return;
        }

        if (ProfileComboBox.SelectedIndex is not (0 or 1) ||
            FormulaComboBox.SelectedIndex is < 0 or > 3)
        {
            ShowValidation("Select a formula profile and an ideal-weight formula.");
            return;
        }

        var heightCm = HeightBox.Value;
        var heightInches = heightCm / CentimetersPerInch;
        var isMaleProfile = ProfileComboBox.SelectedIndex == 0;
        var formulaIndex = FormulaComboBox.SelectedIndex;
        var estimateKg = CalculateFormulaWeight(heightInches, isMaleProfile, formulaIndex);
        var heightMeters = heightCm / 100.0;
        var heightSquared = heightMeters * heightMeters;
        var lowerRangeKg = 18.5 * heightSquared;
        var upperRangeKg = 24.9 * heightSquared;

        FormulaEstimateText.Text =
            $"{estimateKg:N1} kg ({estimateKg / KilogramsPerPound:N1} lb)";
        BmiRangeText.Text =
            $"{lowerRangeKg:N1}-{upperRangeKg:N1} kg ({lowerRangeKg / KilogramsPerPound:N1}-{upperRangeKg / KilogramsPerPound:N1} lb)";
        FormulaDescriptionText.Text = GetFormulaDescription(formulaIndex);
    }

    private void ShowValidation(string message)
    {
        ValidationBar.Title = "Check the inputs";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }

    private static double CalculateFormulaWeight(double heightInches, bool isMaleProfile, int formulaIndex)
    {
        var inchesFromFiveFeet = heightInches - 60.0;
        return (formulaIndex, isMaleProfile) switch
        {
            (0, true) => 50.0 + 2.3 * inchesFromFiveFeet,
            (0, false) => 45.5 + 2.3 * inchesFromFiveFeet,
            (1, true) => 52.0 + 1.9 * inchesFromFiveFeet,
            (1, false) => 49.0 + 1.7 * inchesFromFiveFeet,
            (2, true) => 56.2 + 1.41 * inchesFromFiveFeet,
            (2, false) => 53.1 + 1.36 * inchesFromFiveFeet,
            (3, true) => 48.0 + 2.7 * inchesFromFiveFeet,
            (3, false) => 45.5 + 2.2 * inchesFromFiveFeet,
            _ => throw new ArgumentOutOfRangeException(nameof(formulaIndex))
        };
    }

    private static string GetFormulaDescription(int formulaIndex)
    {
        var formulaName = formulaIndex switch
        {
            0 => "Devine",
            1 => "Robinson",
            2 => "Miller",
            3 => "Hamwi",
            _ => throw new ArgumentOutOfRangeException(nameof(formulaIndex))
        };

        return $"{formulaName} estimates use a reference equation based on height and the selected formula profile. The BMI range uses adult BMI values of 18.5–24.9.";
    }
}
