using Microsoft.UI.Windowing;
using Microsoft.UI.Xaml;
using Tools365.Pages;
using Windows.Graphics;

namespace Tools365.Pages;

public sealed partial class PinnedToolWidgetWindow : Window
{
    public PinnedToolWidgetWindow(ToolCard card)
    {
        InitializeComponent();

        Title = $"Tools365 - {card.Title}";
        TitleText.Text = card.Title;
        DescriptionText.Text = card.Description;
        AppWindow.Resize(new SizeInt32(320, 220));

        if (AppWindow.Presenter is OverlappedPresenter presenter)
        {
            presenter.IsAlwaysOnTop = true;
            presenter.IsResizable = false;
            presenter.IsMaximizable = false;
            presenter.IsMinimizable = true;
        }
    }

    private void UnpinButton_Click(object sender, RoutedEventArgs e)
    {
        Close();
    }
}