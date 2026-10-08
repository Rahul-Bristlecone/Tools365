using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Family.FuelCostCalculator;

public sealed partial class FuelCostCalculatorWindow : Window
{
    private readonly ObservableCollection<VehicleEntry> _vehicleEntries = [];

    public FuelCostCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(860, 820));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }

        VehicleEntriesControl.ItemsSource = _vehicleEntries;
        AddVehicleEntry();
    }

    private void AddVehicleButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;
        AddVehicleEntry();
    }

    private void RemoveVehicleButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: VehicleEntry entry })
        {
            _vehicleEntries.Remove(entry);
            if (_vehicleEntries.Count == 0)
            {
                AddVehicleEntry();
            }
        }
    }

    private void AddVehicleEntry() => _vehicleEntries.Add(new VehicleEntry());

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;

        if (!TryGetPositiveValue(DistanceBox.Value, out var distance))
        {
            ShowValidation("Enter a positive journey distance in kilometres.");
            return;
        }

        if (!TryGetPositiveValue(FuelPriceBox.Value, out var fuelPrice))
        {
            ShowValidation("Enter a positive fuel price per litre.");
            return;
        }

        if (_vehicleEntries.Count == 0)
        {
            ShowValidation("Add at least one vehicle to estimate the trip cost.");
            return;
        }

        var distanceKm = (decimal)distance;
        var fuelPricePerLitre = (decimal)fuelPrice;
        var totalFuelPerTrip = 0m;
        var totalCostPerTrip = 0m;
        var monthlyFuel = 0m;
        var monthlyCost = 0m;

        for (var index = 0; index < _vehicleEntries.Count; index++)
        {
            var vehicle = _vehicleEntries[index];
            if (!TryGetPositiveValue(vehicle.Economy, out var economy))
            {
                ShowValidation($"Enter a positive economy for {GetVehicleName(vehicle, index)}.");
                return;
            }

            if (!TryGetPositiveValue(vehicle.TripsPerMonth, out var tripsPerMonth))
            {
                ShowValidation($"Enter at least one trip per month for {GetVehicleName(vehicle, index)}.");
                return;
            }

            var fuelPerTrip = distanceKm / (decimal)economy;
            var tripCost = fuelPerTrip * fuelPricePerLitre;
            var vehicleTrips = (decimal)tripsPerMonth;

            totalFuelPerTrip += fuelPerTrip;
            totalCostPerTrip += tripCost;
            monthlyFuel += fuelPerTrip * vehicleTrips;
            monthlyCost += tripCost * vehicleTrips;
        }

        FuelPerTripText.Text = $"{totalFuelPerTrip:N2} L";
        CostPerTripText.Text = FormatRupees(totalCostPerTrip);
        MonthlyFuelText.Text = $"{monthlyFuel:N2} L";
        MonthlyCostText.Text = FormatRupees(monthlyCost);
    }

    private static string GetVehicleName(VehicleEntry vehicle, int index) =>
        string.IsNullOrWhiteSpace(vehicle.Name) ? $"vehicle {index + 1}" : vehicle.Name;

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

    private static string FormatRupees(decimal amount)
    {
        var culture = CultureInfo.GetCultureInfo("en-IN");
        return $"₹ {amount.ToString("N2", culture)}";
    }

    public sealed class VehicleEntry : INotifyPropertyChanged
    {
        private string _name = string.Empty;
        private double _economy = double.NaN;
        private double _tripsPerMonth = 1;

        public event PropertyChangedEventHandler? PropertyChanged;

        public string Name
        {
            get => _name;
            set
            {
                if (_name == value)
                {
                    return;
                }

                _name = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Name)));
            }
        }

        public double Economy
        {
            get => _economy;
            set
            {
                if (_economy == value)
                {
                    return;
                }

                _economy = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(Economy)));
            }
        }

        public double TripsPerMonth
        {
            get => _tripsPerMonth;
            set
            {
                if (_tripsPerMonth == value)
                {
                    return;
                }

                _tripsPerMonth = value;
                PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(TripsPerMonth)));
            }
        }
    }
}
