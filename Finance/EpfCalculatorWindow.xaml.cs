using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Windows.Graphics;

namespace Tools365.Finance;

public sealed partial class EpfCalculatorWindow : Window
{
    public EpfCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(560, 470));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

        EmployeeContributionBox.Text = "12";
        EmployerContributionBox.Text = "12";
        AnnualReturnBox.Text = "8.5";
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        if (!double.TryParse(MonthlySalaryBox.Text, out var monthlySalary) || monthlySalary <= 0)
        {
            ShowResult("Enter a valid monthly salary", "Monthly salary must be greater than zero.", InfoBarSeverity.Warning);
            return;
        }

        if (!double.TryParse(YearsBox.Text, out var years) || years <= 0)
        {
            ShowResult("Enter a valid tenure", "Years must be greater than zero.", InfoBarSeverity.Warning);
            return;
        }

        if (!double.TryParse(EmployeeContributionBox.Text, out var employeeRate) || employeeRate < 0)
        {
            ShowResult("Enter a valid employee rate", "Rate must be zero or greater.", InfoBarSeverity.Warning);
            return;
        }

        if (!double.TryParse(EmployerContributionBox.Text, out var employerRate) || employerRate < 0)
        {
            ShowResult("Enter a valid employer rate", "Rate must be zero or greater.", InfoBarSeverity.Warning);
            return;
        }

        if (!double.TryParse(AnnualReturnBox.Text, out var annualReturn) || annualReturn < 0)
        {
            ShowResult("Enter a valid annual return", "Annual return must be zero or greater.", InfoBarSeverity.Warning);
            return;
        }

        var employeeMonthly = monthlySalary * employeeRate / 100.0;
        var employerMonthly = monthlySalary * employerRate / 100.0;
        var totalMonthly = employeeMonthly + employerMonthly;
        var months = years * 12;
        var monthlyRate = annualReturn / 12.0 / 100.0;

        var maturity = 0.0;
        if (monthlyRate > 0)
        {
            maturity = totalMonthly * ((Math.Pow(1 + monthlyRate, months) - 1) / monthlyRate) * (1 + monthlyRate);
        }
        else
        {
            maturity = totalMonthly * months;
        }

        var totalContribution = totalMonthly * months;
        ShowResult(
            $"Estimated corpus: ₹{maturity:N0}",
            $"Monthly contribution: ₹{totalMonthly:N0}; total contributed: ₹{totalContribution:N0}; employee share: ₹{employeeMonthly:N0}; employer share: ₹{employerMonthly:N0}.",
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
