using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Tools365.Finance;

public sealed partial class GstCalculatorWindow : Window
{
    public GstCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(520, 430));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

        RateBox.Text = "18";
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(AmountBox.Text, out var amount) || amount < 0)
        {
            ShowResult("Enter a valid amount", "Amount must be zero or greater.", InfoBarSeverity.Warning);
            return;
        }

        if (!double.TryParse(RateBox.Text, out var rate) || rate < 0)
        {
            ShowResult("Enter a valid GST rate", "GST rate must be zero or greater.", InfoBarSeverity.Warning);
            return;
        }

        var mode = ModeBox.SelectedIndex;
        var taxRate = rate / 100.0;

        if (mode == 0)
        {
            var gstAmount = amount * taxRate;
            var total = amount + gstAmount;
            ShowResult(
                $"GST amount: ₹{gstAmount:N2}; total: ₹{total:N2}",
                $"Exclusive amount: ₹{amount:N2}; GST @ {rate:F2}%: ₹{gstAmount:N2}; inclusive total: ₹{total:N2}.",
                InfoBarSeverity.Success);
            return;
        }

        var netAmount = amount / (1 + taxRate);
        var gstAmountExclusive = amount - netAmount;
        ShowResult(
            $"Net amount: ₹{netAmount:N2}; GST: ₹{gstAmountExclusive:N2}",
            $"Inclusive amount: ₹{amount:N2}; GST @ {rate:F2}%: ₹{gstAmountExclusive:N2}; exclusive base: ₹{netAmount:N2}.",
            InfoBarSeverity.Success);
    }

    private void ShowResult(string title, string message, InfoBarSeverity severity)
    {
        ResultBar.Title = title;
        ResultBar.Message = message;
        ResultBar.Severity = severity;
        ResultBar.IsOpen = true;
    }
}
