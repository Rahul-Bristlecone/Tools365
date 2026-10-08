using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Tools365.Family.TripPackingChecklist;

public sealed partial class TripPackingChecklistWindow : Window
{
    private readonly ObservableCollection<PackingItem> _packingItems = new();
    private readonly Dictionary<string, string[]> _tripTemplates = new()
    {
        ["Weekend getaway"] = ["Travel documents", "Comfortable outfit", "Toiletries", "Phone charger", "Water bottle", "Snacks", "Sunglasses"],
        ["Business trip"] = ["Laptop", "Chargers", "Formal outfit", "Toiletry kit", "Notebook", "Presentation copies", "Travel adapter", "Wallet"],
        ["Beach holiday"] = ["Swimsuit", "Sunscreen", "Flip-flops", "Beach towel", "Hat", "Waterproof pouch", "Sunglasses", "Light cover-up"],
        ["Mountain trek"] = ["Trekking shoes", "Warm jacket", "Water bottle", "Energy snacks", "Headlamp", "First-aid kit", "Hiking socks", "Trekking poles"],
        ["International tour"] = ["Passport", "Travel insurance", "Currency", "Travel adapter", "Medication", "Extra outfit", "Power bank", "Comfort item"]
    };

    public TripPackingChecklistWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(930, 760));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }

        TripTypeComboBox.ItemsSource = _tripTemplates.Keys.ToList();
        TripTypeComboBox.SelectedIndex = 0;

        TripLengthComboBox.ItemsSource = new[] { "1-2 days", "3-5 days", "1 week", "2+ weeks" };
        TripLengthComboBox.SelectedIndex = 0;

        CategoryComboBox.ItemsSource = new[] { "Essentials", "Clothing", "Toiletries", "Technology", "Health", "Accessories", "Miscellaneous" };
        CategoryComboBox.SelectedIndex = 0;

        ChecklistListView.ItemsSource = _packingItems;
        ApplyChecklist();
    }

    private void TripTypeComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyChecklist();

    private void TripLengthComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ApplyChecklist();

    private void AddItemButton_Click(object sender, RoutedEventArgs e)
    {
        var itemName = ItemNameTextBox.Text.Trim();
        if (string.IsNullOrWhiteSpace(itemName))
        {
            StatusTextBlock.Text = "Please enter an item name before adding it to your checklist.";
            return;
        }

        if (_packingItems.Any(item => string.Equals(item.Name, itemName, StringComparison.OrdinalIgnoreCase)))
        {
            StatusTextBlock.Text = $"'{itemName}' is already on your checklist.";
            return;
        }

        var category = (CategoryComboBox.SelectedItem as string) ?? "Miscellaneous";
        _packingItems.Add(new PackingItem
        {
            Name = itemName,
            Category = category,
            IsPacked = false,
            IsEssential = false
        });

        StatusTextBlock.Text = $"Added '{itemName}' to the checklist.";
        ItemNameTextBox.Text = string.Empty;
        UpdateSummary();
    }

    private void ResetChecklistButton_Click(object sender, RoutedEventArgs e) => ApplyChecklist();

    private void MarkAllPackedButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _packingItems)
        {
            item.IsPacked = true;
        }

        UpdateSummary();
    }

    private void ClearPackedButton_Click(object sender, RoutedEventArgs e)
    {
        foreach (var item in _packingItems)
        {
            item.IsPacked = false;
        }

        UpdateSummary();
    }

    private void ChecklistItem_Checked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { DataContext: PackingItem item })
        {
            item.IsPacked = true;
            UpdateSummary();
        }
    }

    private void ChecklistItem_Unchecked(object sender, RoutedEventArgs e)
    {
        if (sender is CheckBox { DataContext: PackingItem item })
        {
            item.IsPacked = false;
            UpdateSummary();
        }
    }

    private void ApplyChecklist()
    {
        var tripType = (TripTypeComboBox.SelectedItem as string) ?? _tripTemplates.Keys.First();
        var tripLength = (TripLengthComboBox.SelectedItem as string) ?? "1-2 days";

        _packingItems.Clear();

        var baseItems = _tripTemplates.TryGetValue(tripType, out var templateItems)
            ? templateItems.ToList()
            : _tripTemplates.Values.First().ToList();

        var extraItems = GetTripLengthExtras(tripLength);
        var combinedItems = baseItems.Concat(extraItems).Distinct(StringComparer.OrdinalIgnoreCase).ToList();

        foreach (var itemName in combinedItems)
        {
            var category = DetermineCategory(itemName);
            _packingItems.Add(new PackingItem
            {
                Name = itemName,
                Category = category,
                IsPacked = false,
                IsEssential = itemName.Contains("documents", StringComparison.OrdinalIgnoreCase) || itemName.Contains("passport", StringComparison.OrdinalIgnoreCase) || itemName.Contains("charger", StringComparison.OrdinalIgnoreCase) || itemName.Contains("adapter", StringComparison.OrdinalIgnoreCase) || itemName.Contains("medication", StringComparison.OrdinalIgnoreCase)
            });
        }

        StatusTextBlock.Text = $"Loaded a {tripLength} {tripType.ToLowerInvariant()} checklist.";
        UpdateSummary();
    }

    private void UpdateSummary()
    {
        var total = _packingItems.Count;
        var packed = _packingItems.Count(item => item.IsPacked);
        var remaining = total - packed;
        var essentials = _packingItems.Count(item => item.IsEssential);
        var completedPercentage = total == 0 ? 0 : (double)packed / total * 100;

        TotalItemsText.Text = total.ToString();
        PackedItemsText.Text = packed.ToString();
        RemainingItemsText.Text = remaining.ToString();
        EssentialItemsText.Text = essentials.ToString();
        PackingProgressBar.Value = completedPercentage;

        if (remaining == 0)
        {
            TripReadinessText.Text = "Ready to go!";
        }
        else if (remaining <= 2)
        {
            TripReadinessText.Text = "Almost packed!";
        }
        else if (packed == 0)
        {
            TripReadinessText.Text = "Packing not started";
        }
        else
        {
            TripReadinessText.Text = "Packing in progress";
        }
    }

    private static List<string> GetTripLengthExtras(string tripLength) => tripLength switch
    {
        "1-2 days" => ["Travel wallet", "Light snack pack"],
        "3-5 days" => ["Laundry bag", "Extra underwear", "Day bag"],
        "1 week" => ["Extra toiletries", "Rechargeable power bank", "Universal travel pouch"],
        "2+ weeks" => ["Travel-sized laundry kit", "Backup essentials", "Portable toiletries pouch", "Extra footwear"],
        _ => []
    };

    private static string DetermineCategory(string itemName) => itemName.ToLowerInvariant() switch
    {
        var value when value.Contains("charger") || value.Contains("laptop") || value.Contains("adapter") || value.Contains("power") => "Technology",
        var value when value.Contains("toile") || value.Contains("soap") || value.Contains("cream") => "Toiletries",
        var value when value.Contains("shirt") || value.Contains("outfit") || value.Contains("clothes") || value.Contains("socks") || value.Contains("shoes") || value.Contains("jacket") => "Clothing",
        var value when value.Contains("passport") || value.Contains("documents") || value.Contains("insurance") || value.Contains("currency") || value.Contains("wallet") => "Essentials",
        var value when value.Contains("medication") || value.Contains("first-aid") || value.Contains("sunscreen") || value.Contains("hat") => "Health",
        var value when value.Contains("sunglasses") || value.Contains("bag") || value.Contains("pouch") || value.Contains("poles") => "Accessories",
        _ => "Miscellaneous"
    };
}

public sealed class PackingItem : INotifyPropertyChanged
{
    private bool _isPacked;

    public string Name { get; set; } = string.Empty;

    public string Category { get; set; } = "Miscellaneous";

    public bool IsPacked
    {
        get => _isPacked;
        set
        {
            if (_isPacked == value)
            {
                return;
            }

            _isPacked = value;
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(IsPacked)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(EssentialVisibility)));
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(nameof(PackedOpacity)));
        }
    }

    public bool IsEssential { get; set; }

    public Visibility EssentialVisibility => IsEssential ? Visibility.Visible : Visibility.Collapsed;

    public double PackedOpacity => IsPacked ? 0.55 : 1.0;

    public event PropertyChangedEventHandler? PropertyChanged;
}
