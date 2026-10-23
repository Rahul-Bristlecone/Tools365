using Microsoft.UI.Xaml;
using Windows.Graphics;

namespace Tools365.Business.FormatValidator;

public sealed partial class BusinessFormatValidatorWindow : Window
{
    public BusinessFormatValidatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(640, 800));
    }

    private void ValidateButton_Click(object sender, RoutedEventArgs e)
    {
        var gstin = BusinessIdentifierValidator.ValidateGstin(GstinBox.Text);
        var pan = BusinessIdentifierValidator.ValidatePan(PanBox.Text);
        var aadhaar = BusinessIdentifierValidator.ValidateAadhaar(AadhaarBox.Password);

        GstinResultText.Text = $"GSTIN: {gstin.Message}";
        PanResultText.Text = $"PAN: {pan.Message}";
        AadhaarResultText.Text = $"Aadhaar: {aadhaar.Message}";
    }
}
