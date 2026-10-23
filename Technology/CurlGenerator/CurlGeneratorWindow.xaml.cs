using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Technology.CurlGenerator;

public sealed partial class CurlGeneratorWindow : Window
{
    public CurlGeneratorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(760, 880));
        UrlBox.TextChanged += InputChanged;
        HeadersBox.TextChanged += InputChanged;
        BodyBox.TextChanged += InputChanged;
        MethodComboBox.SelectionChanged += InputChanged;
        FollowRedirectsCheckBox.Checked += InputChanged;
        FollowRedirectsCheckBox.Unchecked += InputChanged;
        FailOnHttpErrorCheckBox.Checked += InputChanged;
        FailOnHttpErrorCheckBox.Unchecked += InputChanged;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var result = CurlCommandBuilder.Build(
            (MethodComboBox.SelectedItem as ComboBoxItem)?.Content?.ToString() ?? string.Empty,
            UrlBox.Text,
            HeadersBox.Text,
            BodyBox.Text,
            FollowRedirectsCheckBox.IsChecked == true,
            FailOnHttpErrorCheckBox.IsChecked == true);

        if (!result.IsValid)
        {
            CommandBox.Text = string.Empty;
            CopyButton.IsEnabled = false;
            ValidationBar.Title = "Check the request";
            ValidationBar.Message = result.ErrorMessage;
            ValidationBar.IsOpen = true;
            return;
        }

        ValidationBar.IsOpen = false;
        CommandBox.Text = result.Command;
        CopyButton.IsEnabled = true;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(CommandBox.Text);
        Clipboard.SetContent(package);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        UrlBox.Text = string.Empty;
        HeadersBox.Text = string.Empty;
        BodyBox.Text = string.Empty;
        CommandBox.Text = string.Empty;
        ValidationBar.IsOpen = false;
        CopyButton.IsEnabled = false;
    }

    private void InputChanged(object sender, TextChangedEventArgs e) => ClearResult();

    private void InputChanged(object sender, SelectionChangedEventArgs e) => ClearResult();

    private void InputChanged(object sender, RoutedEventArgs e) => ClearResult();

    private void ClearResult()
    {
        CommandBox.Text = string.Empty;
        CopyButton.IsEnabled = false;
        ValidationBar.IsOpen = false;
    }
}
