using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Technology.CronExpressionGenerator;

public sealed partial class CronExpressionGeneratorWindow : Window
{
    public CronExpressionGeneratorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(640, 820));
        ScheduleComboBox.SelectionChanged += ScheduleComboBox_SelectionChanged;
        WeekdayComboBox.SelectionChanged += ScheduleValueChanged;
        IntervalBox.ValueChanged += ScheduleValueChanged;
        HourBox.ValueChanged += ScheduleValueChanged;
        MinuteBox.ValueChanged += ScheduleValueChanged;
        MonthDayBox.ValueChanged += ScheduleValueChanged;
        UpdateFieldVisibility();
        UpdateSchedule();
    }

    private void ScheduleComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        UpdateFieldVisibility();
        UpdateSchedule();
    }

    private void ScheduleValueChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) =>
        UpdateSchedule();

    private void ScheduleValueChanged(object sender, SelectionChangedEventArgs e) =>
        UpdateSchedule();

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        var package = new DataPackage();
        package.SetText(ExpressionTextBox.Text);
        Clipboard.SetContent(package);
    }

    private void UpdateFieldVisibility()
    {
        IntervalPanel.Visibility = ScheduleComboBox.SelectedIndex == 1 ? Visibility.Visible : Visibility.Collapsed;
        TimePanel.Visibility = ScheduleComboBox.SelectedIndex is >= 2 and <= 6
            ? Visibility.Visible
            : Visibility.Collapsed;
        WeekdayPanel.Visibility = ScheduleComboBox.SelectedIndex == 5 ? Visibility.Visible : Visibility.Collapsed;
        MonthDayPanel.Visibility = ScheduleComboBox.SelectedIndex == 6 ? Visibility.Visible : Visibility.Collapsed;
    }

    private void UpdateSchedule()
    {
        if (ScheduleComboBox.SelectedIndex < 0)
        {
            return;
        }

        var schedule = CronScheduleBuilder.Build(
            ScheduleComboBox.SelectedIndex,
            IntervalBox.Value,
            HourBox.Value,
            MinuteBox.Value,
            WeekdayComboBox.SelectedIndex,
            MonthDayBox.Value);

        if (!schedule.IsValid)
        {
            ExpressionTextBox.Text = string.Empty;
            DescriptionText.Text = string.Empty;
            CopyButton.IsEnabled = false;
            ValidationBar.Title = "Check the schedule values";
            ValidationBar.Message = schedule.ErrorMessage;
            ValidationBar.IsOpen = true;
            return;
        }

        ValidationBar.IsOpen = false;
        ExpressionTextBox.Text = schedule.Expression;
        DescriptionText.Text = schedule.Description;
        CopyButton.IsEnabled = true;
    }
}
