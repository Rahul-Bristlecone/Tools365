using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using System.Text.Json;
using Windows.Graphics;

namespace Tools365.Family.ElectricityCostEstimator;

public sealed partial class ElectricityCostEstimatorWindow : Window
{
    private const double DaysPerMonth = 30;

    private readonly IReadOnlyList<CityTariff> _cityTariffs;
    private readonly ObservableCollection<ApplianceEntry> _applianceEntries = [];

    public ElectricityCostEstimatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(840, 950));
        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }

        _cityTariffs = LoadCityTariffs();
        RegionComboBox.ItemsSource = _cityTariffs;
        RegionComboBox.SelectedIndex = 0;
        ApplianceEntriesControl.ItemsSource = _applianceEntries;
        AddApplianceEntry();
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;
        SubsidyBar.IsOpen = false;

        if (RegionComboBox.SelectedItem is not CityTariff cityTariff)
        {
            ShowValidation("Select a region with tariff data.");
            return;
        }

        if (!TryGetPositiveValue(SanctionedLoadBox.Value))
        {
            ShowValidation("Enter a positive sanctioned load.");
            return;
        }

        if (!TryCalculateMonthlyUnits(out var unitsConsumed, out var inputError))
        {
            ShowValidation(inputError);
            return;
        }

        var energyCharges = CalculateEnergyCharges(cityTariff, (decimal)unitsConsumed);
        var ppac = (energyCharges + cityTariff.FixedCharge) * cityTariff.PpacPercent / 100m;
        var electricityDuty = energyCharges * cityTariff.ElectricityDutyPercent / 100m;
        var grossBill = energyCharges + cityTariff.FixedCharge + ppac + electricityDuty;
        var subsidyRule = GetSubsidyRule(cityTariff, (decimal)unitsConsumed);
        var subsidy = CalculateSubsidy(subsidyRule, grossBill);
        var finalBill = grossBill - subsidy;

        UnitsConsumedText.Text = $"{unitsConsumed:N1} units";
        EnergyChargesText.Text = FormatRupees(energyCharges);
        FixedChargesText.Text = FormatRupees(cityTariff.FixedCharge);
        PpacText.Text = FormatRupees(ppac);
        ElectricityDutyText.Text = FormatRupees(electricityDuty);
        GrossBillText.Text = FormatRupees(grossBill);
        SubsidyText.Text = $"-{FormatRupees(subsidy)}";
        FinalBillText.Text = FormatRupees(finalBill);
        PpacLabel.Text = $"PPAC ({cityTariff.PpacPercent:0.##}%)";
        ElectricityDutyLabel.Text = $"Electricity duty ({cityTariff.ElectricityDutyPercent:0.##}%)";
        ShowSubsidy($"{cityTariff.Name} subsidy", GetSubsidyMessage(subsidyRule, subsidy), GetSubsidySeverity(subsidyRule));
    }

    private void AddApplianceButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;
        if (!TryCalculateMonthlyUnits(out _, out var inputError))
        {
            ShowValidation(inputError);
            return;
        }

        AddApplianceEntry();
    }

    private void AddApplianceEntry()
    {
        _applianceEntries.Add(new ApplianceEntry());
    }

    private void DecreaseQuantityButton_Click(object sender, RoutedEventArgs e) =>
        ChangeQuantity(sender, -1);

    private void IncreaseQuantityButton_Click(object sender, RoutedEventArgs e) =>
        ChangeQuantity(sender, 1);

    private static void ChangeQuantity(object sender, double change)
    {
        if (sender is Button { DataContext: ApplianceEntry entry })
        {
            entry.Quantity = Math.Max(1, entry.Quantity + change);
        }
    }

    private void RemoveApplianceButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: ApplianceEntry entry })
        {
            _applianceEntries.Remove(entry);
        }

    }

    private void ApplianceInput_Loaded(object sender, RoutedEventArgs e)
    {
        if (sender is NumberBox numberBox)
        {
            numberBox.RegisterPropertyChangedCallback(NumberBox.TextProperty, ApplianceInput_TextChanged);
        }
    }

    private void ApplianceInput_TextChanged(DependencyObject sender, DependencyProperty property)
    {
        if (sender is not NumberBox { DataContext: ApplianceEntry entry, Tag: string propertyName } numberBox)
        {
            return;
        }

        var value = double.TryParse(
            numberBox.Text,
            NumberStyles.Float | NumberStyles.AllowThousands,
            CultureInfo.CurrentCulture,
            out var parsedValue)
            ? parsedValue
            : double.NaN;

        switch (propertyName)
        {
            case "Watts":
                entry.Watts = value;
                break;
            case "HoursPerDay":
                entry.HoursPerDay = value;
                break;
        }

    }

    private bool TryCalculateMonthlyUnits(out double unitsConsumed, out string error)
    {
        unitsConsumed = 0;

        for (var index = 0; index < _applianceEntries.Count; index++)
        {
            var entry = _applianceEntries[index];
            if (!TryGetPositiveValue(entry.Quantity) ||
                !TryGetPositiveValue(entry.Watts) ||
                !TryGetPositiveValue(entry.HoursPerDay))
            {
                error = $"Enter positive quantity, watts, and daily hours for appliance {index + 1}.";
                return false;
            }

            unitsConsumed += entry.Quantity * entry.Watts / 1000 * entry.HoursPerDay * DaysPerMonth;
        }

        error = string.Empty;
        return true;
    }

    private static decimal CalculateEnergyCharges(CityTariff cityTariff, decimal units)
    {
        decimal energyCharges = 0;
        decimal previousLimit = 0;

        foreach (var slab in cityTariff.EnergySlabs)
        {
            var slabUnits = slab.UpToUnits.HasValue
                ? Math.Min(Math.Max(units - previousLimit, 0m), slab.UpToUnits.Value - previousLimit)
                : Math.Max(units - previousLimit, 0m);
            energyCharges += slabUnits * slab.RatePerUnit;

            if (!slab.UpToUnits.HasValue)
            {
                break;
            }

            previousLimit = slab.UpToUnits.Value;
        }

        return energyCharges;
    }

    private static SubsidyRule GetSubsidyRule(CityTariff cityTariff, decimal units) =>
        cityTariff.SubsidyRules.First(rule => !rule.UpToUnits.HasValue || units <= rule.UpToUnits.Value);

    private static decimal CalculateSubsidy(SubsidyRule subsidyRule, decimal grossBill) =>
        subsidyRule.Type switch
        {
            "GrossBill" => grossBill,
            "FixedAmount" => subsidyRule.Amount,
            _ => 0m
        };

    private static string GetSubsidyMessage(SubsidyRule subsidyRule, decimal subsidy) =>
        subsidyRule.Type switch
        {
            "GrossBill" => "Full subsidy applied. Your final bill is Rs. 0.00.",
            "FixedAmount" => $"A fixed {FormatRupees(subsidy)} subsidy has been applied.",
            _ => "No subsidy applies for this usage."
        };

    private static InfoBarSeverity GetSubsidySeverity(SubsidyRule subsidyRule) =>
        subsidyRule.Type switch
        {
            "GrossBill" => InfoBarSeverity.Success,
            "FixedAmount" => InfoBarSeverity.Informational,
            _ => InfoBarSeverity.Warning
        };

    private static IReadOnlyList<CityTariff> LoadCityTariffs()
    {
        var catalogPath = Path.Combine(AppContext.BaseDirectory, "Family", "ElectricityCostEstimator", "CityTariffs.json");
        var catalog = JsonSerializer.Deserialize<CityTariffCatalog>(
            File.ReadAllText(catalogPath),
            new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        return catalog?.Cities is { Count: > 0 }
            ? catalog.Cities
            : throw new InvalidOperationException("City tariff data is missing or empty.");
    }

    private void ShowValidation(string message)
    {
        ValidationBar.Title = "Check the inputs";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }

    private void ShowSubsidy(string title, string message, InfoBarSeverity severity)
    {
        SubsidyBar.Title = title;
        SubsidyBar.Message = message;
        SubsidyBar.Severity = severity;
        SubsidyBar.IsOpen = true;
    }

    private static bool TryGetPositiveValue(double value) => !double.IsNaN(value) && value > 0;

    private static string FormatRupees(decimal amount) =>
        $"Rs. {amount.ToString("N2", CultureInfo.CurrentCulture)}";

    private sealed class CityTariffCatalog
    {
        public List<CityTariff> Cities { get; init; } = [];
    }

    private sealed class CityTariff
    {
        public string Name { get; init; } = string.Empty;
        public List<EnergySlab> EnergySlabs { get; init; } = [];
        public decimal FixedCharge { get; init; }
        public decimal PpacPercent { get; init; }
        public decimal ElectricityDutyPercent { get; init; }
        public List<SubsidyRule> SubsidyRules { get; init; } = [];
    }

    private sealed class EnergySlab
    {
        public decimal? UpToUnits { get; init; }
        public decimal RatePerUnit { get; init; }
    }

    private sealed class SubsidyRule
    {
        public decimal? UpToUnits { get; init; }
        public string Type { get; init; } = string.Empty;
        public decimal Amount { get; init; }
    }

    public sealed class ApplianceEntry : INotifyPropertyChanged
    {
        private double _quantity = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public int ApplianceIndex { get; set; }
        public double Quantity
        {
            get => _quantity;
            set
            {
                if (_quantity == value)
                {
                    return;
                }

                _quantity = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Quantity)));
            }
        }

        public double Watts { get; set; } = double.NaN;
        public double HoursPerDay { get; set; } = double.NaN;
    }
}