using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Tools365.Technology.ApiResponseTimeCalculator;

public sealed partial class ApiResponseTimeCalculatorWindow : Window
{
    public ApiResponseTimeCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(760, 780));
        SamplesBox.TextChanged += SamplesBox_TextChanged;
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!ResponseTimeAnalyzer.TryAnalyze(SamplesBox.Text, out var statistics, out var error))
        {
            ValidationBar.Title = "Check the response-time samples";
            ValidationBar.Message = error;
            ValidationBar.IsOpen = true;
            ClearResults();
            return;
        }

        ValidationBar.IsOpen = false;
        CountText.Text = statistics!.SampleCount.ToString("N0");
        AverageText.Text = Format(statistics.Average);
        MedianText.Text = Format(statistics.Median);
        MinimumText.Text = Format(statistics.Minimum);
        P95Text.Text = Format(statistics.Percentile95);
        P99Text.Text = Format(statistics.Percentile99);
        MaximumText.Text = Format(statistics.Maximum);
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        SamplesBox.Text = string.Empty;
        ValidationBar.IsOpen = false;
        ClearResults();
    }

    private void SamplesBox_TextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e)
    {
        ValidationBar.IsOpen = false;
        ClearResults();
    }

    private void ClearResults()
    {
        CountText.Text = "—";
        AverageText.Text = "—";
        MedianText.Text = "—";
        MinimumText.Text = "—";
        P95Text.Text = "—";
        P99Text.Text = "—";
        MaximumText.Text = "—";
    }

    private static string Format(double value) => $"{value:0.##} ms";
}
