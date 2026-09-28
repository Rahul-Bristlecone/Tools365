using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using Microsoft.UI.Xaml.Controls.Primitives;
using Microsoft.UI.Xaml.Media;
using Tools365.Pages;

// To learn more about WinUI, the WinUI project structure,
// and more about our project templates, see: http://aka.ms/winui-project-info.

namespace Tools365;

public sealed partial class MainWindow : Window
{
    private readonly Dictionary<string, NavigationBrushSet> _navigationBrushes = new();

    public MainWindow()
    {
        InitializeComponent();
        InitializeNavigationBrushes();

        ExtendsContentIntoTitleBar = true;
        SetTitleBar(AppTitleBar);
        AppWindow.TitleBar.PreferredHeightOption = TitleBarHeightOption.Tall;
        AppWindow.SetIcon("Assets/AppIcon.ico");

        SetActiveNavigationButton(FamilyButton);
        NavFrame.Navigate(typeof(CategoryPage), "Family");
    }

    private void TitleBar_BackRequested(TitleBar sender, object args)
    {
        NavFrame.GoBack();
    }

    private void CategoryButton_Click(object sender, RoutedEventArgs e)
    {
        if (sender is ToggleButton button && button.Tag is string category)
        {
            SetActiveNavigationButton(button);
            NavFrame.Navigate(typeof(CategoryPage), category);
        }
    }

    private void CategoryButton_PointerEntered(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is ToggleButton button)
        {
            SetNavigationButtonVisual(button, button.IsChecked == true, isHovered: true);
        }
    }

    private void CategoryButton_PointerExited(object sender, Microsoft.UI.Xaml.Input.PointerRoutedEventArgs e)
    {
        if (sender is ToggleButton button)
        {
            SetNavigationButtonVisual(button, button.IsChecked == true, isHovered: false);
        }
    }

    private void SetActiveNavigationButton(ToggleButton activeButton)
    {
        var buttons = new[]
        {
            FamilyButton,
            HealthButton,
            FinanceButton,
            UtilityButton,
            EducationButton,
            TechnologyButton,
            GenericButton
        };

        foreach (var button in buttons)
        {
            var isActive = button == activeButton;
            button.IsChecked = isActive;
            SetNavigationButtonVisual(button, isActive, button.IsPointerOver);
        }
    }

    private void SetNavigationButtonVisual(ToggleButton button, bool isActive, bool isHovered)
    {
        var brushSet = _navigationBrushes[button.Tag as string ?? string.Empty];
        button.Background = isActive
                ? isHovered ? brushSet.ActiveHover : brushSet.Active
                : isHovered
                    ? brushSet.InactiveHover
                    : GetNavigationBrush("NavigationPillInactiveBrush");
            button.Foreground = isActive
                ? GetNavigationBrush("NavigationPillActiveForegroundBrush")
                : GetNavigationBrush("NavigationPillInactiveForegroundBrush");
    }

    private Microsoft.UI.Xaml.Media.Brush GetNavigationBrush(string key)
    {
        return (Microsoft.UI.Xaml.Media.Brush)RootGrid.Resources[key];
    }

    private void InitializeNavigationBrushes()
    {
        foreach (var category in new[] { "Family", "Health", "Finance", "Utility", "Education", "Technology", "Generic" })
        {
            var color = CategoryColorPalette.GetColor(category);
            _navigationBrushes[category] = new NavigationBrushSet(
                new SolidColorBrush(color),
                new SolidColorBrush(CategoryColorPalette.Lighten(color, 0.15)),
                new SolidColorBrush(CategoryColorPalette.Lighten(color, 0.78)));
        }
    }

    private sealed record NavigationBrushSet(
        SolidColorBrush Active,
        SolidColorBrush ActiveHover,
        SolidColorBrush InactiveHover);
}
