using Windows.UI;

namespace Tools365;

public static class CategoryColorPalette
{
    public static Color GetColor(string category) => category switch
    {
        "Family" => Color.FromArgb(255, 40, 96, 118),
        "Health" => Color.FromArgb(255, 163, 98, 93),
        "Finance" => Color.FromArgb(255, 127, 121, 147),
        "Utility" => Color.FromArgb(255, 192, 87, 128),
        "Education" => Color.FromArgb(255, 98, 134, 108),
        "Technology" => Color.FromArgb(255, 177, 168, 134),
        _ => Color.FromArgb(255, 128, 0, 32)
    };

    public static Color Lighten(Color color, double amount) => Color.FromArgb(
        255,
        (byte)(color.R + (255 - color.R) * amount),
        (byte)(color.G + (255 - color.G) * amount),
        (byte)(color.B + (255 - color.B) * amount));

    public static Color Darken(Color color, double amount) => Color.FromArgb(
        255,
        (byte)(color.R * (1 - amount)),
        (byte)(color.G * (1 - amount)),
        (byte)(color.B * (1 - amount)));
}