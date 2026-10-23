using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Business.SerialNumberGenerator;

public sealed partial class SerialNumberGeneratorWindow : Window
{
    public SerialNumberGeneratorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(720, 760));
        PrefixBox.TextChanged += SettingsChanged;
        SuffixBox.TextChanged += SettingsChanged;
        StartingNumberBox.ValueChanged += SettingsChanged;
        QuantityBox.ValueChanged += SettingsChanged;
        DigitsBox.ValueChanged += SettingsChanged;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var result = SerialNumberBuilder.Build(
            PrefixBox.Text,
            SuffixBox.Text,
            StartingNumberBox.Value,
            QuantityBox.Value,
            DigitsBox.Value);

        if (!result.IsValid)
        {
            ShowValidation(result.ErrorMessage);
            return;
        }

        ValidationBar.IsOpen = false;
        NumbersBox.Text = result.Numbers;
        CopyButton.IsEnabled = true;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(NumbersBox.Text);
        Clipboard.SetContent(package);
    }

    private void SettingsChanged(object sender, TextChangedEventArgs e) => ClearResult();

    private void SettingsChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => ClearResult();

    private void ClearResult()
    {
        NumbersBox.Text = string.Empty;
        CopyButton.IsEnabled = false;
        ValidationBar.IsOpen = false;
    }

    private void ShowValidation(string message)
    {
        NumbersBox.Text = string.Empty;
        CopyButton.IsEnabled = false;
        ValidationBar.Title = "Check the serial number settings";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }
}
