using System.Globalization;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Automation;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Business.BusinessDocumentGenerator;

public sealed partial class BusinessDocumentGeneratorWindow : Window
{
    private readonly BusinessDocumentType _documentType;
    private int _lineItemCount;

    public BusinessDocumentGeneratorWindow(BusinessDocumentType documentType)
    {
        _documentType = documentType;
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(880, 1080));
        ConfigureDocumentType();
        DocumentDatePicker.Date = DateTimeOffset.Now.Date;
        DocumentNumberBox.Text = $"{GetPrefix()}-{DateTime.Now:yyyyMMdd}-001";
        AddLineItem();

        DocumentNumberBox.TextChanged += SettingsChanged;
        SellerBox.TextChanged += SettingsChanged;
        CustomerBox.TextChanged += SettingsChanged;
        TermsBox.TextChanged += SettingsChanged;
        CurrencyComboBox.SelectionChanged += SettingsChanged;
        DocumentDatePicker.DateChanged += SettingsChanged;
        EndDatePicker.DateChanged += SettingsChanged;
        TaxRateBox.ValueChanged += SettingsChanged;
    }

    private void ConfigureDocumentType()
    {
        var isInvoice = _documentType == BusinessDocumentType.Invoice;
        var title = isInvoice ? "Invoice maker" : "Quotation maker";
        Title = title;
        TitleText.Text = isInvoice ? "Invoice maker" : "Quotation maker";
        SubtitleText.Text = isInvoice
            ? "Create a customer invoice with line items, tax, and payment details."
            : "Prepare a customer quotation with line items, tax, and validity details.";
        NumberLabel.Text = isInvoice ? "Invoice number" : "Quotation number";
        DocumentDateLabel.Text = isInvoice ? "Invoice date" : "Quotation date";
        EndDateLabel.Text = isInvoice ? "Due date (optional)" : "Valid until (optional)";
        DocumentDatePicker.PlaceholderText = isInvoice ? "Choose invoice date" : "Choose quotation date";
        EndDatePicker.PlaceholderText = isInvoice ? "Choose due date" : "Choose validity date";
        GenerateButton.Content = isInvoice ? "Generate invoice" : "Generate quotation";
        CopyButton.Content = isInvoice ? "Copy invoice" : "Copy quotation";
        OutputBox.PlaceholderText = isInvoice
            ? "Generated invoice will appear here..."
            : "Generated quotation will appear here...";
    }

    private string GetPrefix() =>
        _documentType == BusinessDocumentType.Invoice ? "INV" : "QUO";

    private void AddLineButton_Click(object sender, RoutedEventArgs e)
    {
        if (_lineItemCount >= 100)
        {
            ShowValidation("A document can contain up to 100 line items.");
            return;
        }

        AddLineItem();
        ClearResult();
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var lines = new List<BusinessDocumentLine>();
        foreach (var child in LineItemsPanel.Children)
        {
            if (child is Grid row &&
                row.Children[0] is TextBox descriptionBox &&
                row.Children[1] is NumberBox quantityBox &&
                row.Children[2] is NumberBox unitPriceBox)
            {
                lines.Add(new BusinessDocumentLine(descriptionBox.Text, quantityBox.Value, unitPriceBox.Value));
            }
        }

        var result = BusinessDocumentBuilder.Build(
            _documentType,
            DocumentNumberBox.Text,
            SellerBox.Text,
            CustomerBox.Text,
            TermsBox.Text,
            DocumentDatePicker.Date,
            EndDatePicker.Date,
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

        var description = new TextBox { PlaceholderText = "Item or service description" };
        AutomationProperties.SetName(description, $"Line item {_lineItemCount + 1} description");
        Grid.SetColumn(description, 0);

        var quantity = new NumberBox
        {
            Value = 1,
            Minimum = 0.01,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        };
        AutomationProperties.SetName(quantity, $"Line item {_lineItemCount + 1} quantity");
        Grid.SetColumn(quantity, 1);

        var unitPrice = new NumberBox
        {
            Value = 0,
            Minimum = 0,
            SpinButtonPlacementMode = NumberBoxSpinButtonPlacementMode.Compact
        };
        AutomationProperties.SetName(unitPrice, $"Line item {_lineItemCount + 1} unit price");
        Grid.SetColumn(unitPrice, 2);

        var removeButton = new Button { Content = "Remove" };
        AutomationProperties.SetName(removeButton, $"Remove line item {_lineItemCount + 1}");
        removeButton.Click += RemoveLineButton_Click;
        Grid.SetColumn(removeButton, 3);

        description.TextChanged += SettingsChanged;
        quantity.ValueChanged += SettingsChanged;
        unitPrice.ValueChanged += SettingsChanged;
        row.Children.Add(description);
        row.Children.Add(quantity);
        row.Children.Add(unitPrice);
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
            if (child is Grid row && row.Children[3] is Button button)
            {
                button.IsEnabled = _lineItemCount > 1;
            }
        }
    }

    private void SettingsChanged(object sender, TextChangedEventArgs e) => ClearResult();

    private void SettingsChanged(object sender, SelectionChangedEventArgs e) => ClearResult();

    private void SettingsChanged(CalendarDatePicker sender, CalendarDatePickerDateChangedEventArgs args) =>
        ClearResult();

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
        ValidationBar.Title = $"Check the {(_documentType == BusinessDocumentType.Invoice ? "invoice" : "quotation")} details";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }
}
