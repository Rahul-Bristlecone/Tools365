using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Education.NeetScoreCalculator;

public sealed partial class NeetScoreCalculatorWindow : Window
{
    private readonly SubjectInputs[] _subjects;

    public NeetScoreCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(680, 700));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

        _subjects =
        [
            new("Physics", PhysicsCorrectBox, PhysicsIncorrectBox, PhysicsUnattemptedText),
            new("Chemistry", ChemistryCorrectBox, ChemistryIncorrectBox, ChemistryUnattemptedText),
            new("Botany", BotanyCorrectBox, BotanyIncorrectBox, BotanyUnattemptedText),
            new("Zoology", ZoologyCorrectBox, ZoologyIncorrectBox, ZoologyUnattemptedText)
        ];
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        var totalCorrect = 0;
        var totalIncorrect = 0;
        var totalAttempted = 0;
        var totalScore = 0;
        var unattemptedCounts = new int[_subjects.Length];

        for (var index = 0; index < _subjects.Length; index++)
        {
            var subject = _subjects[index];
            if (!TryParseCount(subject.CorrectBox.Text, out var correct) ||
                !TryParseCount(subject.IncorrectBox.Text, out var incorrect))
            {
                ShowValidation($"Enter whole-number counts from 0 to 45 for {subject.Name}.");
                return;
            }

            if (correct + incorrect > 45)
            {
                ShowValidation($"{subject.Name}: correct and incorrect answers cannot total more than 45.");
                return;
            }

            var unattempted = 45 - correct - incorrect;
            unattemptedCounts[index] = unattempted;
            totalCorrect += correct;
            totalIncorrect += incorrect;
            totalAttempted += correct + incorrect;
            totalScore += correct * 4 - incorrect;
        }

        for (var index = 0; index < _subjects.Length; index++)
        {
            _subjects[index].UnattemptedText.Text = unattemptedCounts[index].ToString(CultureInfo.CurrentCulture);
        }

        TotalScoreText.Text = totalScore.ToString(CultureInfo.CurrentCulture);
        SummaryText.Text = $"{totalCorrect} · {totalIncorrect} · {totalAttempted}";
        ValidationBar.IsOpen = false;
    }

    private static bool TryParseCount(string text, out int count)
    {
        return int.TryParse(text, NumberStyles.None, CultureInfo.CurrentCulture, out count) &&
            count is >= 0 and <= 45;
    }

    private void ShowValidation(string message)
    {
        TotalScoreText.Text = "—";
        SummaryText.Text = "—";
        ValidationBar.Title = "Check the answer counts";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }

    private sealed record SubjectInputs(
        string Name,
        TextBox CorrectBox,
        TextBox IncorrectBox,
        TextBlock UnattemptedText);
}
