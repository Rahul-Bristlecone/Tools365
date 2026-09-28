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
    public MainWindow()
    {
        InitializeComponent();

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
            TechnologyButton
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
        button.Background = isActive
                ? new SolidColorBrush(CategoryColorPalette.Lighten(CategoryColorPalette.GetColor(button.Tag as string ?? string.Empty), isHovered ? 0.15 : 0))
                : isHovered
                    ? new SolidColorBrush(CategoryColorPalette.Lighten(CategoryColorPalette.GetColor(button.Tag as string ?? string.Empty), 0.78))
                    : GetNavigationBrush("NavigationPillInactiveBrush");
            button.Foreground = isActive
                ? GetNavigationBrush("NavigationPillActiveForegroundBrush")
                : GetNavigationBrush("NavigationPillInactiveForegroundBrush");
    }

    private Microsoft.UI.Xaml.Media.Brush GetNavigationBrush(string key)
    {
        return (Microsoft.UI.Xaml.Media.Brush)RootGrid.Resources[key];
    }
}
