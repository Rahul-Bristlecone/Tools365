using Microsoft.UI.Xaml;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Technology.TlsCertificateGenerator;

public sealed partial class TlsCertificateGeneratorWindow : Window
{
    public TlsCertificateGeneratorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(1040, 1040));
        PrimaryNameBox.TextChanged += SettingsChanged;
        AdditionalNamesBox.TextChanged += SettingsChanged;
        ValidityDaysBox.ValueChanged += SettingsChanged;
        KeyPasswordBox.PasswordChanged += SettingsChanged;
    }

    private void GenerateButton_Click(object sender, RoutedEventArgs e)
    {
        try
        {
            var certificate = SelfSignedTlsCertificateFactory.Generate(
                PrimaryNameBox.Text,
                AdditionalNamesBox.Text,
                ValidityDaysBox.Value,
                KeyPasswordBox.Password);

            CertificateOutputBox.Text = certificate.CertificatePem;
            PrivateKeyOutputBox.Text = certificate.EncryptedPrivateKeyPem;
            ExpiryText.Text = $"Certificate expires {certificate.NotAfter.ToLocalTime():f}. The private key is encrypted; securely store its password.";
            CopyCertificateButton.IsEnabled = true;
            CopyPrivateKeyButton.IsEnabled = true;
            ValidationBar.IsOpen = false;
        }
        catch (ArgumentException exception)
        {
            ClearOutputs();
            ValidationBar.Title = "Check the certificate settings";
            ValidationBar.Message = exception.Message;
            ValidationBar.IsOpen = true;
        }
        catch (System.Security.Cryptography.CryptographicException exception)
        {
            ClearOutputs();
            ValidationBar.Title = "Certificate generation failed";
            ValidationBar.Message = exception.Message;
            ValidationBar.IsOpen = true;
        }
    }

    private void CopyCertificateButton_Click(object sender, RoutedEventArgs e) =>
        CopyText(CertificateOutputBox.Text);

    private void CopyPrivateKeyButton_Click(object sender, RoutedEventArgs e) =>
        CopyText(PrivateKeyOutputBox.Text);

    private static void CopyText(string text)
    {
        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
    }

    private void SettingsChanged(object sender, Microsoft.UI.Xaml.Controls.TextChangedEventArgs e) =>
        ClearOutputsAndValidation();

    private void SettingsChanged(Microsoft.UI.Xaml.Controls.NumberBox sender, Microsoft.UI.Xaml.Controls.NumberBoxValueChangedEventArgs args) =>
        ClearOutputsAndValidation();

    private void SettingsChanged(object sender, RoutedEventArgs e) => ClearOutputsAndValidation();

    private void ClearOutputsAndValidation()
    {
        ClearOutputs();
        ValidationBar.IsOpen = false;
    }

    private void ClearOutputs()
    {
        CertificateOutputBox.Text = string.Empty;
        PrivateKeyOutputBox.Text = string.Empty;
        ExpiryText.Text = string.Empty;
        CopyCertificateButton.IsEnabled = false;
        CopyPrivateKeyButton.IsEnabled = false;
    }
}
