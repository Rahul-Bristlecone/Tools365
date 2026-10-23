using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Net.Security;
using System.Security.Authentication;
using System.Net.Sockets;
using System.Globalization;
using Windows.Graphics;

namespace Tools365.Technology.SslCertificateChecker;

public sealed partial class SslCertificateCheckerWindow : Window
{
    private CancellationTokenSource? _checkCancellation;

    public SslCertificateCheckerWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(760, 760));
        HostBox.TextChanged += SettingsChanged;
        PortBox.ValueChanged += PortChanged;
        Closed += (_, _) => _checkCancellation?.Cancel();
    }

    private async void CheckButton_Click(object sender, RoutedEventArgs e)
    {
        var host = HostBox.Text.Trim();
        if (!TlsCertificateInspector.IsValidHost(host))
        {
            ShowError("Enter a valid host name or IP address without a URL scheme or path.");
            return;
        }

        if (!double.IsFinite(PortBox.Value) ||
            PortBox.Value < 1 ||
            PortBox.Value > 65535 ||
            PortBox.Value != Math.Truncate(PortBox.Value))
        {
            ShowError("Port must be a whole number from 1 to 65535.");
            return;
        }

        _checkCancellation?.Cancel();
        using var cancellation = new CancellationTokenSource();
        _checkCancellation = cancellation;
        CheckButton.IsEnabled = false;
        StatusBar.Severity = InfoBarSeverity.Informational;
        StatusBar.Title = "Checking certificate";
        StatusBar.Message = $"Connecting to {host}:{PortBox.Value:0} for a TLS handshake...";
        StatusBar.IsOpen = true;
        ClearCertificateDetails();

        try
        {
            var result = await TlsCertificateInspector.CheckAsync(host, (int)PortBox.Value, cancellation.Token);
            DisplayResult(result);
        }
        catch (OperationCanceledException)
        {
            ShowError("The certificate check timed out or was cancelled.");
        }
        catch (SocketException exception)
        {
            ShowError($"Could not connect to {host}:{PortBox.Value:0}: {exception.Message}");
        }
        catch (AuthenticationException exception)
        {
            ShowError($"TLS negotiation failed: {exception.Message}");
        }
        catch (IOException exception)
        {
            ShowError($"The connection ended before the certificate could be checked: {exception.Message}");
        }
        catch (ArgumentException exception)
        {
            ShowError($"Could not check the certificate: {exception.Message}");
        }
        finally
        {
            CheckButton.IsEnabled = true;
            if (ReferenceEquals(_checkCancellation, cancellation))
            {
                _checkCancellation = null;
            }
        }
    }

    private void DisplayResult(TlsCertificateCheckResult result)
    {
        SubjectText.Text = result.Subject;
        DnsNameText.Text = string.IsNullOrWhiteSpace(result.SubjectAlternativeName)
            ? "No DNS name reported"
            : result.SubjectAlternativeName;
        IssuerText.Text = result.Issuer;
        NotBeforeText.Text = result.NotBefore.ToString("f", CultureInfo.CurrentCulture);
        NotAfterText.Text = result.NotAfter.ToString("f", CultureInfo.CurrentCulture);
        ProtocolText.Text = result.Protocol.ToString();

        var trustMessages = new List<string>();
        if (result.PolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNameMismatch))
        {
            trustMessages.Add("The certificate name does not match the requested host.");
        }

        if (result.PolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateChainErrors))
        {
            trustMessages.Add("The certificate chain could not be trusted.");
        }

        if (result.PolicyErrors.HasFlag(SslPolicyErrors.RemoteCertificateNotAvailable))
        {
            trustMessages.Add("The host did not provide a certificate.");
        }

        trustMessages.AddRange(result.ChainErrors);
        var isTrusted = result.PolicyErrors == SslPolicyErrors.None;
        TrustDetailsText.Text = isTrusted
            ? "The certificate chain and host name passed system validation."
            : string.Join(Environment.NewLine, trustMessages.Distinct(StringComparer.Ordinal));
        StatusBar.Severity = isTrusted ? InfoBarSeverity.Success : InfoBarSeverity.Warning;
        StatusBar.Title = isTrusted ? "Certificate trusted" : "Certificate presented; validation warnings found";
        StatusBar.Message = isTrusted
            ? "The host completed a TLS handshake and the certificate passed system validation."
            : "The handshake completed so the certificate can be inspected, but validation reported the warnings below. No application data was sent.";
        StatusBar.IsOpen = true;
    }

    private void ShowError(string message)
    {
        ClearCertificateDetails();
        StatusBar.Severity = InfoBarSeverity.Error;
        StatusBar.Title = "Certificate check failed";
        StatusBar.Message = message;
        StatusBar.IsOpen = true;
    }

    private void ClearCertificateDetails()
    {
        SubjectText.Text = string.Empty;
        DnsNameText.Text = string.Empty;
        IssuerText.Text = string.Empty;
        NotBeforeText.Text = string.Empty;
        NotAfterText.Text = string.Empty;
        ProtocolText.Text = string.Empty;
        TrustDetailsText.Text = string.Empty;
    }

    private void SettingsChanged(object sender, TextChangedEventArgs e) => ClearPreviousCheck();

    private void PortChanged(NumberBox sender, NumberBoxValueChangedEventArgs args) => ClearPreviousCheck();

    private void ClearPreviousCheck()
    {
        _checkCancellation?.Cancel();
        ClearCertificateDetails();
        StatusBar.IsOpen = false;
    }
}
