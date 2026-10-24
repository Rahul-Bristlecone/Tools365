using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Text.Json;
using Windows.Graphics;
using Windows.Storage;

namespace Tools365.Family.ApplianceWarrantyTracker;

public sealed class ApplianceWarrantyRecord
{
    public string Id { get; set; } = Guid.NewGuid().ToString("N");

    public string Name { get; set; } = string.Empty;

    public string Brand { get; set; } = string.Empty;

    public string ModelOrSerialNumber { get; set; } = string.Empty;

    public DateOnly PurchaseDate { get; set; }

    public int WarrantyMonths { get; set; }
}

public sealed class ApplianceWarrantyViewModel
{
    public ApplianceWarrantyViewModel(ApplianceWarrantyRecord record, DateOnly today)
    {
        Id = record.Id;
        DisplayName = string.IsNullOrWhiteSpace(record.Brand)
            ? record.Name
            : $"{record.Brand} {record.Name}";

        var details = new List<string>();
        if (!string.IsNullOrWhiteSpace(record.ModelOrSerialNumber))
        {
            details.Add($"Model / serial: {record.ModelOrSerialNumber}");
        }

        details.Add($"{record.WarrantyMonths} month{(record.WarrantyMonths == 1 ? string.Empty : "s")} warranty");
        Details = string.Join(" · ", details);

        var expiryDate = record.PurchaseDate.AddMonths(record.WarrantyMonths);
        PurchaseDetails = $"Purchased {record.PurchaseDate:dd MMM yyyy} · Coverage through {expiryDate:dd MMM yyyy}";
        var daysUntilExpiry = expiryDate.DayNumber - today.DayNumber;
        Status = daysUntilExpiry < 0
            ? $"Expired {Math.Abs(daysUntilExpiry)} day{(daysUntilExpiry == -1 ? string.Empty : "s")} ago"
            : daysUntilExpiry == 0
                ? "Expires today"
                : daysUntilExpiry <= 30
                    ? $"Expires in {daysUntilExpiry} day{(daysUntilExpiry == 1 ? string.Empty : "s")}"
                    : "Warranty active";
        IsExpired = daysUntilExpiry < 0;
        IsExpiringSoon = daysUntilExpiry is >= 0 and <= 30;
        ExpiryDate = expiryDate;
    }

    public string Id { get; }

    public string DisplayName { get; }

    public string Details { get; }

    public string PurchaseDetails { get; }

    public string Status { get; }

    public DateOnly ExpiryDate { get; }

    public bool IsExpired { get; }

    public bool IsExpiringSoon { get; }
}

public sealed partial class ApplianceWarrantyTrackerWindow : Window
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly ObservableCollection<ApplianceWarrantyViewModel> _appliances = new();
    private readonly List<ApplianceWarrantyRecord> _records = new();
    private readonly string _statePath = Path.Combine(
        ApplicationData.Current.LocalFolder.Path,
        "appliance-warranty-tracker.json");
    private bool _storageReady;

    public DateTimeOffset Today => DateTimeOffset.Now.Date;

    public ApplianceWarrantyTrackerWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(760, 800));
        PurchaseDatePicker.Date = Today;
        PurchaseDatePicker.MaxDate = Today;
        AppliancesList.ItemsSource = _appliances;
        LoadRecords();
    }

    private void AddApplianceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_storageReady)
        {
            return;
        }

        var name = ApplianceNameBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(name))
        {
            ShowMessage("Appliance name required", "Enter an appliance name before adding it.", InfoBarSeverity.Warning);
            return;
        }

        if (PurchaseDatePicker.Date is null)
        {
            ShowMessage("Purchase date required", "Choose the appliance's purchase date.", InfoBarSeverity.Warning);
            return;
        }

        if (!double.IsFinite(WarrantyMonthsBox.Value) ||
            WarrantyMonthsBox.Value != Math.Truncate(WarrantyMonthsBox.Value) ||
            WarrantyMonthsBox.Value is < 1 or > 600)
        {
            ShowMessage("Check warranty duration", "Enter a whole number of months between 1 and 600.", InfoBarSeverity.Warning);
            return;
        }

        var purchaseDate = DateOnly.FromDateTime(PurchaseDatePicker.Date.Value.LocalDateTime);
        var record = new ApplianceWarrantyRecord
        {
            Name = name,
            Brand = BrandBox.Text.Trim(),
            ModelOrSerialNumber = ModelBox.Text.Trim(),
            PurchaseDate = purchaseDate,
            WarrantyMonths = (int)WarrantyMonthsBox.Value
        };

        _records.Add(record);
        RefreshList();
        if (!SaveRecords())
        {
            return;
        }

        ApplianceNameBox.Text = string.Empty;
        BrandBox.Text = string.Empty;
        ModelBox.Text = string.Empty;
        WarrantyMonthsBox.Value = 12;
        ShowMessage("Appliance added", $"{record.Name} was added to your warranty tracker.", InfoBarSeverity.Success);
    }

    private async void RemoveApplianceButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_storageReady || sender is not Button { Tag: string applianceId })
        {
            return;
        }

        var record = _records.FirstOrDefault(item => item.Id == applianceId);
        if (record is null)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Remove appliance?",
            Content = $"Remove {record.Name} and its warranty details from this device?",
            PrimaryButtonText = "Remove",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = AppliancesList.XamlRoot
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        _records.Remove(record);
        RefreshList();
        if (!SaveRecords())
        {
            return;
        }

        ShowMessage("Appliance removed", $"{record.Name} was removed from your tracker.", InfoBarSeverity.Success);
    }

    private async void ClearAllButton_Click(object sender, RoutedEventArgs e)
    {
        if (!_storageReady || _records.Count == 0)
        {
            return;
        }

        var dialog = new ContentDialog
        {
            Title = "Clear all appliances?",
            Content = "This removes all tracked appliances and warranty details from this device.",
            PrimaryButtonText = "Clear all",
            CloseButtonText = "Cancel",
            DefaultButton = ContentDialogButton.Close,
            XamlRoot = AppliancesList.XamlRoot
        };

        if (await dialog.ShowAsync() != ContentDialogResult.Primary)
        {
            return;
        }

        _records.Clear();
        RefreshList();
        if (!SaveRecords())
        {
            return;
        }

        ShowMessage("Tracker cleared", "All appliance records were removed.", InfoBarSeverity.Success);
    }

    private void LoadRecords()
    {
        try
        {
            if (File.Exists(_statePath))
            {
                var json = File.ReadAllText(_statePath);
                var records = JsonSerializer.Deserialize<List<ApplianceWarrantyRecord>>(json, JsonOptions)
                    ?? throw new JsonException("The saved appliance list is empty.");
                if (records.Any(record => !IsValidRecord(record)))
                {
                    throw new JsonException("The saved appliance list contains invalid records.");
                }

                _records.AddRange(records);
            }

            _storageReady = true;
            RefreshList();
        }
        catch (IOException ex)
        {
            ShowStorageError(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowStorageError(ex);
        }
        catch (JsonException ex)
        {
            ShowStorageError(ex);
        }
    }

    private bool SaveRecords()
    {
        if (!_storageReady)
        {
            return false;
        }

        try
        {
            var json = JsonSerializer.Serialize(_records, JsonOptions);
            File.WriteAllText(_statePath, json);
            return true;
        }
        catch (IOException ex)
        {
            ShowStorageError(ex);
            return false;
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowStorageError(ex);
            return false;
        }
    }

    private void RefreshList()
    {
        var today = DateOnly.FromDateTime(DateTime.Today);
        var appliances = _records
            .Select(record => new ApplianceWarrantyViewModel(record, today))
            .OrderBy(appliance => appliance.ExpiryDate)
            .ToArray();

        _appliances.Clear();
        foreach (var appliance in appliances)
        {
            _appliances.Add(appliance);
        }

        var activeCount = appliances.Count(appliance => !appliance.IsExpired && !appliance.IsExpiringSoon);
        var expiringCount = appliances.Count(appliance => appliance.IsExpiringSoon);
        var expiredCount = appliances.Count(appliance => appliance.IsExpired);
        SummaryText.Text = appliances.Length == 0
            ? "No appliances tracked yet."
            : $"{appliances.Length} tracked · {activeCount} active · {expiringCount} expiring within 30 days · {expiredCount} expired";
        EmptyState.Visibility = appliances.Length == 0 ? Visibility.Visible : Visibility.Collapsed;
        ClearAllButton.IsEnabled = appliances.Length > 0;
    }

    private static bool IsValidRecord(ApplianceWarrantyRecord record) =>
        !string.IsNullOrWhiteSpace(record.Id) &&
        !string.IsNullOrWhiteSpace(record.Name) &&
        record.PurchaseDate <= DateOnly.FromDateTime(DateTime.Today) &&
        record.WarrantyMonths is >= 1 and <= 600;

    private void ShowStorageError(Exception exception)
    {
        _storageReady = false;
        ApplianceNameBox.IsEnabled = false;
        BrandBox.IsEnabled = false;
        ModelBox.IsEnabled = false;
        PurchaseDatePicker.IsEnabled = false;
        WarrantyMonthsBox.IsEnabled = false;
        AddApplianceButton.IsEnabled = false;
        ClearAllButton.IsEnabled = false;
        AppliancesList.IsEnabled = false;
        ShowMessage(
            "Unable to load or save appliance records",
            exception.Message,
            InfoBarSeverity.Error);
    }

    private void ShowMessage(string title, string message, InfoBarSeverity severity)
    {
        StatusBar.Title = title;
        StatusBar.Message = message;
        StatusBar.Severity = severity;
        StatusBar.IsOpen = true;
    }
}
