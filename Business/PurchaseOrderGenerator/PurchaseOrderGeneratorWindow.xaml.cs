using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Automation;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Business.PurchaseOrderGenerator;

public sealed partial class PurchaseOrderGeneratorWindow : Window
{
    private int _lineItemCount;

    public PurchaseOrderGeneratorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(880, 1040));
        OrderDatePicker.Date = DateTimeOffset.Now.Date;
        PurchaseOrderNumberBox.Text = $"PO-{DateTime.Now:yyyyMMdd}-001";
        AddLineItem();

        PurchaseOrderNumberBox.TextChanged += SettingsChanged;
        SupplierBox.TextChanged += SettingsChanged;
        BuyerBox.TextChanged += SettingsChanged;
        CurrencyComboBox.SelectionChanged += SettingsChanged;
        OrderDatePicker.DateChanged += SettingsChanged;
        DeliveryDatePicker.DateChanged += SettingsChanged;
        TaxRateBox.ValueChanged += SettingsChanged;
    }

    private void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (_lineItemCount >= 100)
        {
            ShowValidation("A purchase order can contain up to 100 line items.");
            return;
        }

        AddLineItem();
        ClearResult();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var lines = new List<PurchaseOrderLine>();
        foreach (var child in LineItemsPanel.Children)
        {
            if (child is not Grid row ||
                row.Children[0] is not TextBox descriptionBox ||
                row.Children[1] is not NumberBox quantityBox ||
                row.Children[2] is not NumberBox unitPriceBox)
            {
                continue;
            }

            lines.Add(new PurchaseOrderLine(descriptionBox.Text, quantityBox.Value, unitPriceBox.Value));
        }

        var result = PurchaseOrderBuilder.Build(
            PurchaseOrderNumberBox.Text,
            SupplierBox.Text,
            BuyerBox.Text,
            OrderDatePicker.Date,
            DeliveryDatePicker.Date,
            GetCurrencyCode(),
            TaxRateBox.Value,
            lines);

        if (!result.IsValid)
        {
            ShowValidation(result.ErrorMessage);
            return;
        }

        ValidationBar.IsOpen = false;
        OutputBox.Text = result.Document;
        TotalText.Text = $"{result.Total.ToString("N2", GetCurrencyCulture())} {GetCurrencyCode()}";
        CopyButton.IsEnabled = true;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(OutputBox.Text);
        Clipboard.SetContent(package);
    }

    private void AddLineItem()
    {
        var row = new Grid { ColumnSpacing = 8 };
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(3, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = new GridLength(1, GridUnitType.Star) });
        row.ColumnDefinitions.Add(new ColumnDefinition { Width = GridLength.Auto });

        var descriptionBox = new TextBox { PlaceholderText = "Item or service description" };
        AutomationProperties.SetName(descriptionBox, $"Line item {_lineItemCount + 1} description");
        Grid.SetColumn(descriptionBox, 0);

        var quantityBox = new NumberBox
        {
            Value = 1,
            Minimum = 0.01,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        };
        AutomationProperties.SetName(quantityBox, $"Line item {_lineItemCount + 1} quantity");
        Grid.SetColumn(quantityBox, 1);

        var unitPriceBox = new NumberBox
        {
            Value = 0,
            Minimum = 0,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        };
        AutomationProperties.SetName(unitPriceBox, $"Line item {_lineItemCount + 1} unit price");
        Grid.SetColumn(unitPriceBox, 2);

        var removeButton = new Button
        {
            Content = "Remove",
            IsEnabled = _lineItemCount > 0
        };
        AutomationProperties.SetName(removeButton, $"Remove line item {_lineItemCount + 1}");
        removeButton.Click += RemoveLineButton_Click;
        Grid.SetColumn(removeButton, 3);

        descriptionBox.TextChanged += SettingsChanged;
        quantityBox.ValueChanged += SettingsChanged;
        unitPriceBox.ValueChanged += SettingsChanged;
        row.Children.Add(descriptionBox);
        row.Children.Add(quantityBox);
        row.Children.Add(unitPriceBox);
        row.Children.Add(removeButton);
        LineItemsPanel.Children.Add(row);

        _lineItemCount++;
        UpdateRemoveButtons();
    }

    private void RemoveLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button button && button.Parent is Grid row)
        {
            LineItemsPanel.Children.Remove(row);
            _lineItemCount--;
            UpdateRemoveButtons();
            ClearResult();
        }
    }

    private void UpdateRemoveButtons()
    {
        foreach (var child in LineItemsPanel.Children)
        {
            if (child is Grid row && row.Children[3] is Button removeButton)
            {
                removeButton.IsEnabled = _lineItemCount > 1;
            }
        }
    }

    private void SettingsChanged(object sender, TextChangedEventArgs e) => ClearResult();

    private void SettingsChanged(object sender, SelectionChangedEventArgs e) => ClearResult();

    private void SettingsChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) => ClearResult();

    private void SettingsChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => ClearResult();

    private string GetCurrencyCode() =>
        CurrencyComboBox.SelectedIndex switch
        {
            0 => "INR",
            1 => "USD",
            2 => "EUR",
            3 => "GBP",
            _ => string.Empty
        };

    private CultureInfo GetCurrencyCulture() =>
        GetCurrencyCode() switch
        {
            "INR" => CultureInfo.GetCultureInfo("en-IN"),
            "EUR" => CultureInfo.GetCultureInfo("de-DE"),
            "GBP" => CultureInfo.GetCultureInfo("en-GB"),
            _ => CultureInfo.GetCultureInfo("en-US")
        };

    private void ClearResult()
    {
        OutputBox.Text = string.Empty;
        TotalText.Text = "—";
        CopyButton.IsEnabled = false;
        ValidationBar.IsOpen = false;
    }

    private void ShowValidation(string message)
    {
        OutputBox.Text = string.Empty;
        TotalText.Text = "—";
        CopyButton.IsEnabled = false;
        ValidationBar.Title = "Check the purchase order details";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }
}
