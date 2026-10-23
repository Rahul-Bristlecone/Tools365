using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Numerics;
using Windows.Graphics;

namespace Tools365.Education.RadixConverter;

public sealed partial class RadixConverterWindow : Window
{
    private const string Digits = "0123456789ABCDEFGHIJKLMNOPQRSTUVWXYZ";

    public RadixConverterWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(560, 620));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
        }

        var bases = Enumerable.Range(2, 35)
            .Select(radix => $"Base {radix}")
            .ToArray();
        FromBaseComboBox.ItemsSource = bases;
        ToBaseComboBox.ItemsSource = bases;
        FromBaseComboBox.SelectedIndex = 0;
        ToBaseComboBox.SelectedIndex = 8;
        ValueBox.Text = "1010";
        ConvertValue();
    }

    private void ValueBox_TextChanged(object sender, TextChangedEventArgs e) => ConvertValue();

    private void BaseComboBox_SelectionChanged(object sender, SelectionChangedEventArgs e) => ConvertValue();

    private void SwapBases_Click(object sender, RoutedEventArgs e)
    {
        var fromIndex = FromBaseComboBox.SelectedIndex;
        var toIndex = ToBaseComboBox.SelectedIndex;
        FromBaseComboBox.SelectedIndex = toIndex;
        ToBaseComboBox.SelectedIndex = fromIndex;
    }

    private void ConvertValue()
    {
        if (FromBaseComboBox.SelectedIndex < 0 || ToBaseComboBox.SelectedIndex < 0)
        {
            return;
        }

        var sourceBase = FromBaseComboBox.SelectedIndex + 2;
        var targetBase = ToBaseComboBox.SelectedIndex + 2;
        var text = ValueBox.Text.Trim();

        if (text.Length == 0)
        {
            ShowValidation("Enter a number to convert.");
            return;
        }

        if (!TryParseValue(text, sourceBase, out var value, out var errorMessage))
        {
            ShowValidation(errorMessage);
            return;
        }

        ResultText.Text = $"{FormatValue(value, targetBase)} (base {targetBase})";
        ValidationBar.IsOpen = false;
    }

    private static bool TryParseValue(string text, int radix, out BigInteger value, out string errorMessage)
    {
        value = BigInteger.Zero;
        errorMessage = string.Empty;

        var isNegative = text[0] == '-';
        var startIndex = text[0] is '-' or '+' ? 1 : 0;
        if (startIndex == text.Length)
        {
            errorMessage = "Enter at least one digit after the sign.";
            return false;
        }

        for (var index = startIndex; index < text.Length; index++)
        {
            var digit = Digits.IndexOf(char.ToUpperInvariant(text[index]));
            if (digit < 0 || digit >= radix)
            {
                errorMessage = $"“{text[index]}” is not a valid digit in base {radix}.";
                return false;
            }

            value = value * radix + digit;
        }

        if (isNegative)
        {
            value = -value;
        }

        return true;
    }

    private static string FormatValue(BigInteger value, int radix)
    {
        if (value.IsZero)
        {
            return "0";
        }

        var isNegative = value.Sign < 0;
        value = BigInteger.Abs(value);
        var digits = new List<char>();
        while (value > BigInteger.Zero)
        {
            value = BigInteger.DivRem(value, radix, out var remainder);
            digits.Add(Digits[(int)remainder]);
        }

        if (isNegative)
        {
            digits.Add('-');
        }

        digits.Reverse();
        return new string(digits.ToArray());
    }

    private void ShowValidation(string message)
    {
        ResultText.Text = "—";
        ValidationBar.Title = "Check the number";
        ValidationBar.Message = message;
        ValidationBar.IsOpen = true;
    }
}
