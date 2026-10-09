using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Tools365.Finance;

public sealed partial class CagrCalculatorWindow : Window
{
    public CagrCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(520, 420));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(InitialValueBox.Text, out var initialValue) || initialValue <= 0)
        {
            ShowResult("Enter a valid initial value", "Initial value must be greater than zero.", InfoBarSeverity.Warning);
            return;
        }

        if (!double.TryParse(FinalValueBox.Text, out var finalValue) || finalValue <= 0)
        {
            ShowResult("Enter a valid final value", "Final value must be greater than zero.", InfoBarSeverity.Warning);
            return;
        }

        if (!double.TryParse(YearsBox.Text, out var years) || years <= 0)
        {
            ShowResult("Enter a valid period", "Years must be greater than zero.", InfoBarSeverity.Warning);
            return;
        }

        var cagr = Math.Pow(finalValue / initialValue, 1.0 / years) - 1.0;
        var cagrPercent = cagr * 100;
        ShowResult($"CAGR: {cagrPercent:F2}%", $"The investment grew by {cagrPercent:F2}% per year over {years:F0} years.", InfoBarSeverity.Success);
    }

    private void ShowResult(string title, string message, InfoBarSeverity severity)
    {
        ResultBar.Title = title;
        ResultBar.Message = message;
        ResultBar.Severity = severity;
        ResultBar.IsOpen = true;
    }
}
