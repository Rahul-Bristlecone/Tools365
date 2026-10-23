using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Tools365.Technology.JwtDecoder;

public sealed partial class JwtDecoderWindow : Window
{
    public JwtDecoderWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(960, 900));
        TokenBox.TextChanged += TokenBox_TextChanged;
    }

    private void DecodeButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var result = JwtTokenDecoder.Decode(TokenBox.Text);
            HeaderBox.Text = result.Header;
            PayloadBox.Text = result.Payload;
            SignatureBox.Text = result.Signature;
            ValidationBar.IsOpen = false;
        }
        catch (FormatException exception)
        {
            ClearDecodedParts();
            ValidationBar.Title = "Could not decode token";
            ValidationBar.Message = exception.Message;
            ValidationBar.IsOpen = true;
        }
    }

    private void ClearButton_Click(object sender, RoutedEventArgs e)
    {
        TokenBox.Text = string.Empty;
        ClearDecodedParts();
        ValidationBar.IsOpen = false;
    }

    private void ClearDecodedParts()
    {
        HeaderBox.Text = string.Empty;
        PayloadBox.Text = string.Empty;
        SignatureBox.Text = string.Empty;
    }

    private void TokenBox_TextChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e)
    {
        ClearDecodedParts();
        ValidationBar.IsOpen = false;
    }
}
