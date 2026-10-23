using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Text.Json;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;
using YamlDotNet.Core;

namespace Tools365.Technology.YamlJsonConverter;

public sealed partial class YamlJsonConverterWindow : Window
{
    public YamlJsonConverterWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(1080, 760));
        DirectionComboBox.SelectionChanged += DirectionComboBox_SelectionChanged;
        InputTextBox.TextChanged += InputTextBox_TextChanged;
        UpdateFormatLabels();
    }

    public void SetSourceFormat(string sourceFormat)
    {
        DirectionComboBox.SelectedIndex = sourceFormat switch
        {
            "YAML" => 0,
            "JSON" => 1,
            _ => throw new ArgumentOutOfRangeException(nameof(sourceFormat), sourceFormat, "Unsupported source format.")
        };
    }

    private void ConvertButton_Click(object sender, RoutedEventArgs e)
    {
        if (string.IsNullOrWhiteSpace(InputTextBox.Text))
        {
            ShowValidation("Enter a document to convert.");
            return;
        }

        try
        {
            OutputTextBox.Text = DirectionComboBox.SelectedIndex == 0
                ? YamlJsonConversion.YamlToJson(InputTextBox.Text)
                : YamlJsonConversion.JsonToYaml(InputTextBox.Text);
            ValidationBar.IsOpen = false;
            CopyButton.IsEnabled = true;
        }
        catch (YamlException exception)
        {
            ShowValidation(exception.Message);
        }
        catch (JsonException exception)
        {
            ShowValidation(exception.Message);
        }
        catch (NotSupportedException exception)
        {
            ShowValidation(exception.Message);
        }
    }

    private void DirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        ValidationBar.IsOpen = false;
        UpdateFormatLabels();
        ClearOutput();
    }

    private void InputTextBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        ValidationBar.IsOpen = false;
        ClearOutput();
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(OutputTextBox.Text);
        Clipboard.SetContent(package);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        InputTextBox.Text = string.Empty;
        OutputTextBox.Text = string.Empty;
        ValidationBar.IsOpen = false;
    }

    private void UpdateFormatLabels()
    {
        var sourceFormat = DirectionComboBox.SelectedIndex == 0 ? "YAML" : "JSON";
        var targetFormat = DirectionComboBox.SelectedIndex == 0 ? "JSON" : "YAML";
        SourceFormatText.Text = $"{sourceFormat} INPUT";
        TargetFormatText.Text = $"{targetFormat} OUTPUT";
        InputTextBox.PlaceholderText = $"Paste {sourceFormat} here...";
    }

    private void ClearOutput()
    {
        OutputTextBox.Text = string.Empty;
        CopyButton.IsEnabled = false;
    }

    private void ShowValidation(string message)
    {
        ClearOutput();
        ValidationBar.Title = "Could not convert document";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }
}
