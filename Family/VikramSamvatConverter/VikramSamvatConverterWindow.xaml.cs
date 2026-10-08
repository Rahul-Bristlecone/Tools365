using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Family.VikramSamvatConverter;

public sealed partial class VikramSamvatConverterWindow : Window
{
    private static readonly string[] VikramMonths =
    [
        "Chaitra",
        "Vaishakh",
        "Jyeshtha",
        "Ashadh",
        "Shravan",
        "Bhadrapad",
        "Ashwin",
        "Kartik",
        "Margashirsha",
        "Paush",
        "Magh",
        "Falgun"
    ];

    public VikramSamvatConverterWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(780, 700));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }

        DirectionComboBox.ItemsSource = new[]
        {
            "Gregorian → Vikram Samvat",
            "Vikram Samvat → Gregorian"
        };
        DirectionComboBox.SelectedIndex = 0;

        VsMonthComboBox.ItemsSource = VikramMonths;
        VsMonthComboBox.SelectedIndex = 0;
        GregorianDatePicker.Date = DateTime.Today;

        UpdateInputVisibility();
        UpdateResultLabels();
        ConvertDate();
    }

    private void DirectionComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateInputVisibility();

    private void ConvertButton_Click(object sender, RoutedEventArgs e) =>
        ConvertDate();

    private void UpdateInputVisibility()
    {
        var isGregorianToVikram = DirectionComboBox.SelectedIndex == 0;
        GregorianInputPanel.Visibility = isGregorianToVikram ? Visibility.Visible : Visibility.Collapsed;
        VikramInputPanel.Visibility = isGregorianToVikram ? Visibility.Collapsed : Visibility.Visible;
        UpdateResultLabels();
    }

    private void UpdateResultLabels()
    {
        var isGregorianToVikram = DirectionComboBox.SelectedIndex == 0;
        SourceLabel.Text = isGregorianToVikram ? "Gregorian date" : "Vikram Samvat date";
        TargetLabel.Text = isGregorianToVikram ? "Vikram Samvat" : "Gregorian date";
    }

    private void ConvertDate()
    {
        ValidationBar.IsOpen = false;

        if (DirectionComboBox.SelectedIndex == 0)
        {
            var gregorianInputDate = GregorianDatePicker.Date.DateTime;
            var vikramYear = gregorianInputDate.Year + 57;
            if (gregorianInputDate.Month < 3 || (gregorianInputDate.Month == 3 && gregorianInputDate.Day < 22))
            {
                vikramYear--;
            }

            var vikramMonthIndex = ((gregorianInputDate.Month - 3 + 12) % 12) + 1;
            var vikramDay = gregorianInputDate.Day;
            var monthName = VikramMonths[vikramMonthIndex - 1];

            SourceValueText.Text = gregorianInputDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
            TargetValueText.Text = $"{vikramYear} {monthName} {vikramDay}";
            return;
        }

        var year = (int)VsYearBox.Value;
        var monthIndex = VsMonthComboBox.SelectedIndex + 1;
        var day = (int)VsDayBox.Value;

        if (year <= 0)
        {
            ShowValidation("Enter a valid Vikram Samvat year.");
            return;
        }

        if (day <= 0 || day > 30)
        {
            ShowValidation("Vikram Samvat day must be between 1 and 30.");
            return;
        }

        var gregorianYear = year - 57;
        if (monthIndex >= 10)
        {
            gregorianYear++;
        }

        var gregorianMonth = ((monthIndex + 2) % 12) + 1;
        var maxDay = DateTime.DaysInMonth(gregorianYear, gregorianMonth);
        var safeDay = Math.Min(day, maxDay);
        var convertedGregorianDate = new DateTime(gregorianYear, gregorianMonth, safeDay);

        SourceValueText.Text = $"{year} {VikramMonths[monthIndex - 1]} {day}";
        TargetValueText.Text = convertedGregorianDate.ToString("dd MMM yyyy", CultureInfo.InvariantCulture);
    }

    private void ShowValidation(string message)
    {
        ValidationBar.Title = "Check the inputs";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }
}
