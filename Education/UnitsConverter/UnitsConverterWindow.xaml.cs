using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Education.UnitsConverter;

public sealed partial class UnitsConverterWindow : Window
{
    private static readonly UnitCategory[] Categories =
    [
        new("Length", [
            new("Millimeters (mm)", 0.001),
            new("Centimeters (cm)", 0.01),
            new("Meters (m)", 1),
            new("Kilometers (km)", 1000),
            new("Inches (in)", 0.0254),
            new("Feet (ft)", 0.3048),
            new("Yards (yd)", 0.9144),
            new("Miles (mi)", 1609.344)
        ]),
        new("Weight / mass", [
            new("Milligrams (mg)", 0.000001),
            new("Grams (g)", 0.001),
            new("Kilograms (kg)", 1),
            new("Metric tonnes (t)", 1000),
            new("Ounces (oz)", 0.028349523125),
            new("Pounds (lb)", 0.45359237),
            new("US tons (short tons)", 907.18474)
        ]),
        new("Temperature", [
            new("Celsius (°C)", 1, 0),
            new("Fahrenheit (°F)", 5.0 / 9.0, -32.0 * 5.0 / 9.0),
            new("Kelvin (K)", 1, -273.15)
        ]),
        new("Area", [
            new("Square centimeters (cm²)", 0.0001),
            new("Square meters (m²)", 1),
            new("Hectares (ha)", 10000),
            new("Square kilometers (km²)", 1_000_000),
            new("Square feet (ft²)", 0.09290304),
            new("Acres (ac)", 4046.8564224),
            new("Square miles (mi²)", 2_589_988.110336)
        ]),
        new("Volume", [
            new("Milliliters (mL)", 0.001),
            new("Liters (L)", 1),
            new("Cubic meters (m³)", 1000),
            new("US fluid ounces (fl oz)", 0.0295735295625),
            new("US cups", 0.2365882365),
            new("US gallons (gal)", 3.785411784),
            new("Imperial gallons (imp gal)", 4.54609)
        ]),
        new("Time", [
            new("Milliseconds (ms)", 0.001),
            new("Seconds (s)", 1),
            new("Minutes (min)", 60),
            new("Hours (h)", 3600),
            new("Days (d)", 86400),
            new("Weeks (wk)", 604800)
        ]),
        new("Speed", [
            new("Meters per second (m/s)", 1),
            new("Kilometers per hour (km/h)", 1.0 / 3.6),
            new("Miles per hour (mph)", 0.44704),
            new("Knots (kn)", 0.514444444444)
        ])
    ];

    private UnitCategory? _selectedCategory;
    private bool _isUpdatingUnits;

    public UnitsConverterWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(760, 430));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

        CategoryComboBox.ItemsSource = Categories.Select(category => category.Name).ToArray();
        CategoryComboBox.SelectedIndex = 0;
    }

    private void CategoryComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (CategoryComboBox.SelectedIndex < 0)
        {
            return;
        }

        _selectedCategory = Categories[CategoryComboBox.SelectedIndex];
        _isUpdatingUnits = true;
        var unitNames = _selectedCategory.Units.Select(unit => unit.Name).ToArray();
        FromUnitComboBox.ItemsSource = unitNames;
        ToUnitComboBox.ItemsSource = unitNames;
        FromUnitComboBox.SelectedIndex = 0;
        ToUnitComboBox.SelectedIndex = Math.Min(1, unitNames.Length - 1);
        _isUpdatingUnits = false;
        UpdateConversion();
    }

    private void UnitComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (!_isUpdatingUnits)
        {
            UpdateConversion();
        }
    }

    private void InputValueBox_ValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) =>
        UpdateConversion();

    private void SwapUnits_Click(object sender, RoutedEventArgs e)
    {
        var fromIndex = FromUnitComboBox.SelectedIndex;
        var toIndex = ToUnitComboBox.SelectedIndex;
        _isUpdatingUnits = true;
        FromUnitComboBox.SelectedIndex = toIndex;
        ToUnitComboBox.SelectedIndex = fromIndex;
        _isUpdatingUnits = false;
        UpdateConversion();
    }

    private void UpdateConversion()
    {
        if (_selectedCategory is null ||
            FromUnitComboBox.SelectedIndex < 0 ||
            ToUnitComboBox.SelectedIndex < 0)
        {
            return;
        }

        var input = InputValueBox.Value;
        if (!double.IsFinite(input))
        {
            ResultValueText.Text = "—";
            ConversionFormulaText.Text = "Enter a finite number to convert.";
            return;
        }

        var fromUnit = _selectedCategory.Units[FromUnitComboBox.SelectedIndex];
        var toUnit = _selectedCategory.Units[ToUnitComboBox.SelectedIndex];
        var baseValue = input * fromUnit.Factor + fromUnit.Offset;
        var result = (baseValue - toUnit.Offset) / toUnit.Factor;

        if (!double.IsFinite(result))
        {
            ResultValueText.Text = "—";
            ConversionFormulaText.Text = "The converted value is outside the supported range.";
            return;
        }

        var formattedInput = input.ToString("G10", CultureInfo.CurrentCulture);
        var formattedResult = result.ToString("G10", CultureInfo.CurrentCulture);
        ResultValueText.Text = formattedResult;
        ConversionFormulaText.Text = $"{formattedInput} {fromUnit.Symbol} = {formattedResult} {toUnit.Symbol}";
    }

    private sealed record UnitCategory(string Name, UnitDefinition[] Units);

    private sealed record UnitDefinition(string Name, double Factor, double Offset = 0)
    {
        public string Symbol => Name[..Name.LastIndexOf('(')].Trim();
    }
}
