using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Business.ProductSkuGenerator;

public sealed partial class ProductSkuGeneratorWindow : Window
{
    public ProductSkuGeneratorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(760, 820));
        PrefixBox.TextChanged += SettingsChanged;
        CategoryCodeBox.TextChanged += SettingsChanged;
        ProductCodeBox.TextChanged += SettingsChanged;
        SeparatorComboBox.SelectionChanged += SettingsChanged;
        StartingNumberBox.ValueChanged += SettingsChanged;
        QuantityBox.ValueChanged += SettingsChanged;
        DigitsBox.ValueChanged += SettingsChanged;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var result = ProductSkuBuilder.Build(
            PrefixBox.Text,
            CategoryCodeBox.Text,
            ProductCodeBox.Text,
            SeparatorComboBox.SelectedIndex switch
            {
                0 => "-",
                1 => "_",
                2 => string.Empty,
                _ => "\0"
            },
            StartingNumberBox.Value,
            QuantityBox.Value,
            DigitsBox.Value);

        if (!result.IsValid)
        {
            ShowValidation(result.ErrorMessage);
            return;
        }

        ValidationBar.IsOpen = false;
        SkusBox.Text = result.Skus;
        CopyButton.IsEnabled = true;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(SkusBox.Text);
        Clipboard.SetContent(package);
    }

    private void SettingsChanged(object sender, TextChangedEventArgs e) => ClearResult();

    private void SettingsChanged(object sender, SelectionChangedEventArgs e) => ClearResult();

    private void SettingsChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => ClearResult();

    private void ClearResult()
    {
        SkusBox.Text = string.Empty;
        CopyButton.IsEnabled = false;
        ValidationBar.IsOpen = false;
    }

    private void ShowValidation(string message)
    {
        SkusBox.Text = string.Empty;
        CopyButton.IsEnabled = false;
        ValidationBar.Title = "Check the SKU settings";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }
}
