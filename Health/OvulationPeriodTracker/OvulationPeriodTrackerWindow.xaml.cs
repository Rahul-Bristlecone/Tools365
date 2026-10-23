using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Tools365.Health.OvulationPeriodTracker;

public sealed partial class OvulationPeriodTrackerWindow : Window
{
    public OvulationPeriodTrackerWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(640, 700));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }

        var today = DateTime.Today;
        LastPeriodDatePicker.MinDate = new DateTimeOffset(today.AddYears(-2));
        LastPeriodDatePicker.MaxDate = new DateTimeOffset(today);
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;

        if (LastPeriodDatePicker.Date is not DateTimeOffset selectedDate)
        {
            ShowValidation("Choose the first day of your last period.");
            return;
        }

        if (selectedDate.Date > DateTimeOffset.Now.Date)
        {
            ShowValidation("The first day of your last period cannot be in the future.");
            return;
        }

        if (!TryGetWholeNumber(CycleLengthBox.Value, 21, 45, out var cycleLength))
        {
            ShowValidation("Enter an average cycle length from 21 to 45 days.");
            return;
        }

        if (!TryGetWholeNumber(PeriodLengthBox.Value, 1, 10, out var periodLength))
        {
            ShowValidation("Enter a period length from 1 to 10 days.");
            return;
        }

        if (periodLength >= cycleLength)
        {
            ShowValidation("Period length must be shorter than the cycle length.");
            return;
        }

        var lastPeriodStart = selectedDate.Date;
        var today = DateTimeOffset.Now.Date;
        var nextPeriodStart = lastPeriodStart.AddDays(cycleLength);

        while (nextPeriodStart <= today)
        {
            nextPeriodStart = nextPeriodStart.AddDays(cycleLength);
        }

        var periodEnd = nextPeriodStart.AddDays(periodLength - 1);
        var ovulationDate = nextPeriodStart.AddDays(-14);
        var fertileWindowStart = ovulationDate.AddDays(-5);
        var fertileWindowEnd = ovulationDate.AddDays(1);

        NextPeriodText.Text = FormatDate(nextPeriodStart);
        PeriodEndText.Text = FormatDate(periodEnd);
        OvulationText.Text = FormatDate(ovulationDate);
        FertileWindowText.Text = $"{FormatDate(fertileWindowStart)} - {FormatDate(fertileWindowEnd)}";
    }

    private void ShowValidation(string message)
    {
        ValidationBar.Title = "Check the inputs";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }

    private static bool TryGetWholeNumber(double value, int minimum, int maximum, out int result)
    {
        result = 0;
        if (!double.IsFinite(value) || value < minimum || value > maximum || value != Math.Truncate(value))
        {
            return false;
        }

        result = (int)value;
        return true;
    }

    private static string FormatDate(DateTimeOffset date) =>
        date.ToString("ddd, MMM d, yyyy", System.Globalization.CultureInfo.CurrentCulture);
}
