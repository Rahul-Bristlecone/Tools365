using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.IO;
using System.Text.Json;
using Windows.Graphics;
using Windows.Storage;

namespace Tools365.Family.VaccinationReminder;

public sealed partial class VaccinationReminderWindow : Window
{
    private static readonly JsonSerializerOptions JsonOptions = new() { WriteIndented = true };
    private readonly string _statePath = Path.Combine(
        ApplicationData.Current.LocalFolder.Path,
        "vaccination-reminder-india.json");
    private readonly VaccinationReminderState _state = new();
    private bool _isLoading = true;
    private bool _isUpdatingSchedule;
    private bool _storageReady;

    public DateTimeOffset Today => DateTimeOffset.Now.Date;

    public VaccinationReminderWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(820, 860));
    }

    private async void Window_Loaded(object sender, RoutedEventArgs e)
    {
        try
        {
            if (File.Exists(_statePath))
            {
                var json = await File.ReadAllTextAsync(_statePath);
                var savedState = JsonSerializer.Deserialize<VaccinationReminderState>(json, JsonOptions)
                    ?? throw new JsonException("The saved vaccination profile is empty.");
                _state.ChildDateOfBirth = savedState.ChildDateOfBirth;
                _state.IsJeEndemicArea = savedState.IsJeEndemicArea;
                _state.AdministeredDates = savedState.AdministeredDates ?? new Dictionary<string, DateOnly>();
            }

            _storageReady = true;
            _isLoading = true;
            ChildDateOfBirthPicker.Date = _state.ChildDateOfBirth is null
                ? null
                : new DateTimeOffset(_state.ChildDateOfBirth.Value.ToDateTime(TimeOnly.MinValue));
            JeEndemicAreaSwitch.IsOn = _state.IsJeEndemicArea;
            _isLoading = false;
            RefreshSchedule();
        }
        catch (IOException ex)
        {
            ShowStorageError(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowStorageError(ex);
        }
        catch (JsonException ex)
        {
            ShowStorageError(ex);
        }
    }

    private async void ChildDateOfBirthPicker_DateChanged(
        CalendarDatePicker sender,
        CalendarDatePickerDateChangedEventArgs args)
    {
        if (_isLoading || !_storageReady)
        {
            return;
        }

        _state.ChildDateOfBirth = args.NewDate is null
            ? null
            : DateOnly.FromDateTime(args.NewDate.Value.LocalDateTime);
        RefreshSchedule();
        await SaveStateAsync();
    }

    private async void JeEndemicAreaSwitch_Toggled(object sender, RoutedEventArgs e)
    {
        if (_isLoading || !_storageReady)
        {
            return;
        }

        _state.IsJeEndemicArea = JeEndemicAreaSwitch.IsOn;
        RefreshSchedule();
        await SaveStateAsync();
    }

    private async void AdministeredDatePicker_DateChanged(
        CalendarDatePicker sender,
        CalendarDatePickerDateChangedEventArgs args)
    {
        if (_isLoading || _isUpdatingSchedule || !_storageReady || sender.Tag is not string doseId)
        {
            return;
        }

        DateOnly? newAdministeredDate = args.NewDate is null
            ? null
            : DateOnly.FromDateTime(args.NewDate.Value.LocalDateTime);
        if (_state.AdministeredDates.TryGetValue(doseId, out var existingDate) &&
            newAdministeredDate == existingDate)
        {
            return;
        }

        if (newAdministeredDate is null && !_state.AdministeredDates.ContainsKey(doseId))
        {
            return;
        }

        if (newAdministeredDate is null)
        {
            _state.AdministeredDates.Remove(doseId);
        }
        else
        {
            _state.AdministeredDates[doseId] = newAdministeredDate.Value;
        }

        RefreshSchedule();
        await SaveStateAsync();
    }

    private async void ClearAdministeredDate_Click(object sender, RoutedEventArgs e)
    {
        if (!_storageReady || sender is not Button { Tag: string doseId })
        {
            return;
        }

        _state.AdministeredDates.Remove(doseId);
        RefreshSchedule();
        await SaveStateAsync();
    }

    private void RefreshSchedule()
    {
        if (_state.ChildDateOfBirth is null)
        {
            SchedulePanel.Visibility = Visibility.Collapsed;
            StatusBar.Title = "Add the child's date of birth";
            StatusBar.Message = "Choose a date of birth to see estimated dose dates and reminders.";
            StatusBar.Severity = InfoBarSeverity.Informational;
            StatusBar.IsOpen = true;
            return;
        }

        var doses = VaccinationSchedule.GetDoses(_state.IsJeEndemicArea)
            .Select(dose =>
            {
                DateOnly? administeredDate = _state.AdministeredDates.TryGetValue(dose.Id, out var recordedDate)
                    ? recordedDate
                    : null;
                return new VaccinationDoseViewModel(
                    dose,
                    dose.GetStartDate(_state.ChildDateOfBirth.Value),
                    dose.GetEndDate(_state.ChildDateOfBirth.Value),
                    administeredDate);
            })
            .ToArray();

        var today = DateOnly.FromDateTime(DateTime.Today);
        var overdueCount = doses.Count(dose =>
            !dose.HasAdministeredDate && today > dose.DueWindowEnd);
        var dueCount = doses.Count(dose =>
            !dose.HasAdministeredDate && today >= dose.DueDate && today <= dose.DueWindowEnd);
        var upcomingCount = doses.Count(dose =>
            !dose.HasAdministeredDate && today < dose.DueDate);
        var recordedCount = doses.Count(dose => dose.HasAdministeredDate);

        SummaryText.Text = $"{recordedCount} recorded · {dueCount} due · {overdueCount} overdue · {upcomingCount} upcoming";
        _isUpdatingSchedule = true;
        try
        {
            DosesList.ItemsSource = doses;
        }
        finally
        {
            _isUpdatingSchedule = false;
        }
        SchedulePanel.Visibility = Visibility.Visible;
        StatusBar.IsOpen = false;
    }

    private async Task SaveStateAsync()
    {
        try
        {
            var json = JsonSerializer.Serialize(_state, JsonOptions);
            await File.WriteAllTextAsync(_statePath, json);
            StatusBar.Title = "Saved on this device";
            StatusBar.Message = "The vaccination profile and dose records were saved locally.";
            StatusBar.Severity = InfoBarSeverity.Success;
            StatusBar.IsOpen = true;
        }
        catch (IOException ex)
        {
            ShowStorageError(ex);
        }
        catch (UnauthorizedAccessException ex)
        {
            ShowStorageError(ex);
        }
    }

    private void ShowStorageError(Exception exception)
    {
        _storageReady = false;
        _isLoading = false;
        ChildDateOfBirthPicker.IsEnabled = false;
        JeEndemicAreaSwitch.IsEnabled = false;
        SchedulePanel.Visibility = Visibility.Collapsed;
        StatusBar.Title = "Unable to load or save vaccination records";
        StatusBar.Message = exception.Message;
        StatusBar.Severity = InfoBarSeverity.Error;
        StatusBar.IsOpen = true;
    }
}
