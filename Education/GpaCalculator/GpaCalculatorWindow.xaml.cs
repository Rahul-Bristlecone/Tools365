using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.ObjectModel;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Education.GpaCalculator;

public sealed partial class GpaCalculatorWindow : Window
{
    private static readonly double[] GradePoints =
    {
        4.0, 3.7, 3.3, 3.0, 2.7, 2.3, 2.0, 1.7, 1.3, 1.0, 0.0
    };

    private int _nextCourseNumber = 1;

    public ObservableCollection<GpaCourse> Courses { get; } = new();

    public GpaCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(760, 700));
        Courses.CollectionChanged += (_, _) => UpdateGpa();
        UpdateGpa();
    }

    private void AddCourse_Click(object sender, RoutedEventArgs e)
    {
        Courses.Add(new GpaCourse($"Course {_nextCourseNumber++}"));
        EmptyCoursesText.Visibility = Visibility.Collapsed;
    }

    private void RemoveCourse_Click(object sender, RoutedEventArgs e)
    {
        if (sender is Button { DataContext: GpaCourse course })
        {
            Courses.Remove(course);
            EmptyCoursesText.Visibility = Courses.Count == 0 ? Visibility.Visible : Visibility.Collapsed;
        }
    }

    private void CourseCredits_TextChanged(object sender, TextChangedEventArgs e)
    {
        if (sender is TextBox { DataContext: GpaCourse course } textBox)
        {
            course.Credits = textBox.Text;
        }

        UpdateGpa();
    }

    private void Grade_SelectionChanged(object sender, SelectionChangedEventArgs e)
    {
        if (sender is ComboBox { DataContext: GpaCourse course } comboBox)
        {
            course.GradeIndex = comboBox.SelectedIndex;
        }

        UpdateGpa();
    }

    private void UpdateGpa()
    {
        CourseCountText.Text = Courses.Count == 1 ? "1 course" : $"{Courses.Count} courses";

        if (Courses.Count == 0)
        {
            GpaValueText.Text = "—";
            TotalCreditsText.Text = "0.00 credits";
            ValidationBar.IsOpen = false;
            return;
        }

        var totalCredits = 0.0;
        var totalQualityPoints = 0.0;

        foreach (var course in Courses)
        {
            if (!double.TryParse(
                    course.Credits,
                    NumberStyles.Number,
                    CultureInfo.CurrentCulture,
                    out var credits) ||
                !double.IsFinite(credits) ||
                credits <= 0 ||
                course.GradeIndex < 0 ||
                course.GradeIndex >= GradePoints.Length)
            {
                GpaValueText.Text = "—";
                TotalCreditsText.Text = "—";
                ValidationBar.Title = "Complete each course";
                ValidationBar.Message = "Enter positive credits and select a grade for every course.";
                ValidationBar.IsOpen = true;
                return;
            }

            totalCredits += credits;
            totalQualityPoints += credits * GradePoints[course.GradeIndex];
            if (!double.IsFinite(totalCredits) || !double.IsFinite(totalQualityPoints))
            {
                GpaValueText.Text = "—";
                TotalCreditsText.Text = "—";
                ValidationBar.Title = "Credit total is too large";
                ValidationBar.Message = "Reduce the course credits to calculate a GPA.";
                ValidationBar.IsOpen = true;
                return;
            }
        }

        var gpa = totalQualityPoints / totalCredits;
        GpaValueText.Text = gpa.ToString("0.00", CultureInfo.CurrentCulture);
        TotalCreditsText.Text = $"{totalCredits:0.##} credits";
        ValidationBar.IsOpen = false;
    }
}

public sealed class GpaCourse
{
    public string CourseName { get; }

    public string Credits { get; set; } = string.Empty;

    public int GradeIndex { get; set; } = -1;

    public GpaCourse(string courseName)
    {
        CourseName = courseName;
    }
}
