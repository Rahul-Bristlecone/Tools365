using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Tools365.Family.NakshatraCalculator;

public sealed partial class NakshatraCalculatorWindow : Window
{
    private static readonly string[] NakshatraNames =
    {
        "Ashwini", "Bharani", "Krittika", "Rohini", "Mrigashira", "Ardra", "Punarvasu",
        "Pushya", "Ashlesha", "Magha", "Purva Phalguni", "Uttara Phalguni", "Hasta",
        "Chitra", "Swati", "Vishakha", "Anuradha", "Jyeshtha", "Mula", "Purva Ashadha",
        "Uttara Ashadha", "Shravana", "Dhanishta", "Shatabhisha", "Purva Bhadrapada",
        "Uttara Bhadrapada", "Revati"
    };

    private static readonly string[] NakshatraSymbols =
    {
        "Horse's head", "Yoni", "Flame", "Chariot", "Deer's head", "Tears", "Good dawn",
        "Nourishment", "Serpent", "Royal throne", "Bed of flowers", "Second bed of flowers", "Hand",
        "Bright jewel", "Sword", "Branch", "Success", "Oldest", "Root", "First victory",
        "Second victory", "Listening", "Wealthy", "Hundred physicians", "First blessing",
        "Second blessing", "Prosperity"
    };

    private static readonly string[] NakshatraDeities =
    {
        "Ashwini Kumaras", "Yama", "Agni", "Brahma", "Chandra", "Rudra", "Aditi",
        "Brihaspati", "Sarpa", "Pitris", "Bhaga", "Aryaman", "Savitar",
        "Vishvakarma", "Vayu", "Indra", "Mitra", "Indra", "Nirriti", "Apah",
        "Vishvadevas", "Hari", "Vasu", "Varuna", "Ahirbudhnya", "Aja Ekapada", "Pushan"
    };

    public NakshatraCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(760, 620));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }

        BirthDatePicker.Date = DateTime.Today.AddYears(-28);
        BirthTimePicker.Time = new TimeSpan(9, 30, 0);
        CalculateButton_Click(this, null!);
    }

    private void CalculateButton_Click(object sender, RoutedEventArgs e)
    {
        var selectedDate = BirthDatePicker.Date.DateTime.Date;
        var selectedTime = BirthTimePicker.Time;

        if (selectedDate == default)
        {
            selectedDate = DateTime.Today;
        }

        var moonLongitude = CalculateMoonLongitude(selectedDate, selectedTime);
        var nakshatraIndex = GetNakshatraIndex(moonLongitude);
        var pada = GetPada(moonLongitude);

        var nakshatraName = NakshatraNames[nakshatraIndex];
        var symbol = NakshatraSymbols[nakshatraIndex];
        var deity = NakshatraDeities[nakshatraIndex];
        var dateLabel = selectedDate.ToString("dd MMM yyyy");
        var timeLabel = selectedTime.ToString();

        NakshatraNameText.Text = nakshatraName;
        NakshatraDetailsText.Text = $"{dateLabel} at {timeLabel} • Pada {pada} • {symbol} • Ruling deity: {deity}";
        NakshatraSummaryText.Text = $"This reading is an approximation of the moon's nakshatra position at birth. {nakshatraName} is the {nakshatraIndex + 1}th nakshatra in the lunar zodiac cycle and is associated with {symbol.ToLowerInvariant()}.";
    }

    private static double CalculateMoonLongitude(DateTime birthDate, TimeSpan birthTime)
    {
        var referenceNewMoon = new DateTime(2000, 1, 6, 18, 14, 0, DateTimeKind.Utc);
        var birthUtc = new DateTime(birthDate.Year, birthDate.Month, birthDate.Day, birthTime.Hours, birthTime.Minutes, birthTime.Seconds, DateTimeKind.Local).ToUniversalTime();
        var totalDays = (birthUtc - referenceNewMoon).TotalDays;
        var synodicMonth = 29.530588853;
        var moonAge = ((totalDays % synodicMonth) + synodicMonth) % synodicMonth;
        var longitude = (moonAge / synodicMonth) * 360.0;
        var adjustedLongitude = (longitude + 90.0 + 360.0) % 360.0;
        return adjustedLongitude;
    }

    private static int GetNakshatraIndex(double longitude)
    {
        var degreesPerNakshatra = 360.0 / 27.0;
        var index = (int)(longitude / degreesPerNakshatra);
        return index % 27;
    }

    private static int GetPada(double longitude)
    {
        var degreesPerNakshatra = 360.0 / 27.0;
        var segment = longitude % degreesPerNakshatra;
        var pada = (int)(segment / (degreesPerNakshatra / 4.0)) + 1;
        return Math.Min(pada, 4);
    }
}
