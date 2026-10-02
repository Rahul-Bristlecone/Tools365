using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Tools365.Utility.AgeCalculator;

public sealed partial class AgeCalculatorWindow : Window
{
    public AgeCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(560, 560));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }

        DateOfBirthPicker.MaxDate = new DateTimeOffset(DateTime.Today);
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        ValidationBar.IsOpen = false;

        if (DateOfBirthPicker.Date is not DateTimeOffset selectedDate)
        {
            ShowValidation("Select your date of birth.");
            return;
        }

        var dateOfBirth = DateOnly.FromDateTime(selectedDate.DateTime);
        var today = DateOnly.FromDateTime(DateTime.Today);
        if (dateOfBirth > today)
        {
            ShowValidation("Date of birth cannot be in the future.");
            return;
        }

        var years = today.Year - dateOfBirth.Year;
        if (dateOfBirth.AddYears(years) > today)
        {
            years--;
        }

        var lastBirthday = dateOfBirth.AddYears(years);
        var months = 0;
        while (months < 11 && lastBirthday.AddMonths(months + 1) <= today)
        {
            months++;
        }

        var days = today.DayNumber - lastBirthday.AddMonths(months).DayNumber;
        var nextBirthday = dateOfBirth.AddYears(today.Year - dateOfBirth.Year);
        if (nextBirthday <= today)
        {
            nextBirthday = dateOfBirth.AddYears(today.Year - dateOfBirth.Year + 1);
        }

        AgeText.Text = $"{years} {Pluralize(years, "year")}, {months} {Pluralize(months, "month")}, {days} {Pluralize(days, "day")}";
        NextBirthdayText.Text = $"Next birthday: {nextBirthday:MMMM d, yyyy} ({nextBirthday.DayNumber - today.DayNumber} days away).";
        DaysLivedText.Text = $"Days lived: {today.DayNumber - dateOfBirth.DayNumber:N0}.";
        ResultPanel.Visibility = Visibility.Visible;
    }

    private void ShowValidation(string message)
    {
        ResultPanel.Visibility = Visibility.Collapsed;
        ValidationBar.Title = "Check the date";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }

    private static string Pluralize(int count, string unit) =>
        count == 1 ? unit : $"{unit}s";
}