using Microsoft.UI.Xaml;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Family.RoomAreaVolumeCalculator;

public sealed partial class RoomAreaVolumeCalculatorWindow : Window
{
    public RoomAreaVolumeCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(700, 640));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;

        if (!TryGetPositiveValue(LengthBox.Value, out var length))
        {
            ShowValidation("Enter a positive room length in metres.");
            return;
        }

        if (!TryGetPositiveValue(WidthBox.Value, out var width))
        {
            ShowValidation("Enter a positive room width in metres.");
            return;
        }

        if (!TryGetPositiveValue(HeightBox.Value, out var height))
        {
            ShowValidation("Enter a positive room height in metres.");
            return;
        }

        var area = length * width;
        var volume = area * height;
        var perimeter = 2 * (length + width);

        AreaText.Text = $"{area:N2} m²";
        VolumeText.Text = $"{volume:N2} m³";
        PerimeterText.Text = $"{perimeter:N2} m";
    }

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
}
