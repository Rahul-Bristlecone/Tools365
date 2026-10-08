using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Collections.Generic;
using System.Linq;
using Windows.Graphics;

namespace Tools365.Family.CookingRandomiser;

public sealed partial class CookingRandomiserWindow : Window
{
    private readonly Random _random = new();
    private readonly Dictionary<string, string[]> _mealIdeas = new()
    {
        ["Quick & light"] = ["Masala oats", "Veggie wraps", "Greek salad bowl", "Tomato soup & toast", "Cucumber rice bowl"],
        ["Comfort food"] = ["Paneer tikka bowl", "Creamy pasta", "Cheese garlic toast", "Makhani dal", "Vegetable khichdi"],
        ["High protein"] = ["Soy chilla", "Egg bhurji wrap", "Grilled tofu bowl", "Chicken salad platter", "Paneer quinoa bowl"],
        ["Family feast"] = ["Veg biryani", "Paneer butter masala", "Tandoori platter", "Dal fry with rice", "Vegetable pulao"],
        ["Treat yourself"] = ["Loaded garlic naan", "Cheese burst toast", "Schezwan noodles", "Mushroom pasta", "Loaded fries with dip"]
    };

    public CookingRandomiserWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(760, 680));

        if (AppWindow.Presenter is Microsoft.UI.Windowing.OverlappedPresenter presenter)
        {
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
        }

        MoodComboBox.ItemsSource = _mealIdeas.Keys.ToList();
        MoodComboBox.SelectedIndex = 0;

        TimeComboBox.ItemsSource = new[] { "15 min", "30 min", "45 min", "1 hour+" };
        TimeComboBox.SelectedIndex = 0;

        IngredientComboBox.ItemsSource = new[] { "Vegetarian", "Paneer", "Egg", "Chicken", "Tofu", "Mixed" };
        IngredientComboBox.SelectedIndex = 0;

        MethodComboBox.ItemsSource = new[] { "Stovetop", "Air fry", "Bake", "Steam", "No-cook" };
        MethodComboBox.SelectedIndex = 0;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        var mood = MoodComboBox.SelectedItem as string ?? _mealIdeas.Keys.First();
        var time = (TimeComboBox.SelectedItem as string) ?? "15 min";
        var ingredient = (IngredientComboBox.SelectedItem as string) ?? "Vegetarian";
        var method = (MethodComboBox.SelectedItem as string) ?? "Stovetop";

        var availableMeals = _mealIdeas.TryGetValue(mood, out var meals)
            ? meals.ToList()
            : _mealIdeas.Values.SelectMany(x => x).ToList();

        var meal = availableMeals[_random.Next(availableMeals.Count)];
        MealNameText.Text = meal;
        MealDetailsText.Text = $"{meal} • {ingredient} • {time} • {method}";
    }
}
