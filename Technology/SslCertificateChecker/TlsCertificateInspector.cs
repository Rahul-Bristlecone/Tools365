using System.Net;
using System.Net.Security;
using System.Net.Sockets;
using System.Security.Authentication;
using System.Security.Cryptography.X509Certificates;

namespace Tools365.Technology.SslCertificateChecker;

internal sealed record TlsCertificateCheckResult(
    string Subject,
    string SubjectAlternativeName,
    string Issuer,
    DateTime NotBefore,
    DateTime NotAfter,
    SslProtocols Protocol,
    SslPolicyErrors PolicyErrors,
    IReadOnlyList<string> ChainErrors);

internal static class TlsCertificateInspector
{
    private static readonly TimeSpan Timeout = TimeSpan.FromSeconds(15);

    public static async Task<TlsCertificateCheckResult> CheckAsync(
        string host,
        int port,
        CancellationToken cancellationToken)
    {
        using var timeout = CancellationTokenSource.CreateLinkedTokenSource(cancellationToken);
        timeout.CancelAfter(Timeout);

        using var client = new TcpClient();
        await client.ConnectAsync(host, port, timeout.Token);

        var policyErrors = SslPolicyErrors.None;
        string[] chainErrors = [];
        using var tls = new SslStream(
            client.GetStream(),
            leaveInnerStreamOpen: false,
            (_, _, chain, errors) =>
            {
                policyErrors = errors;
                chainErrors = chain?.ChainStatus
                    .Select(status => status.StatusInformation.Trim())
                    .Where(status => status.Length > 0)
                    .Distinct(StringComparer.Ordinal)
                    .ToArray() ?? [];
                return true;
            });

        await tls.AuthenticateAsClientAsync(
            new SslClientAuthenticationOptions { TargetHost = host },
            timeout.Token);

        if (tls.RemoteCertificate is null)
        {
            throw new AuthenticationException("The host did not present a TLS certificate.");
        }

        using var certificate = X509CertificateLoader.LoadCertificate(
            tls.RemoteCertificate.Export(X509ContentType.Cert));

        return new TlsCertificateCheckResult(
            certificate.Subject,
            certificate.GetNameInfo(X509NameType.DnsName, forIssuer: false),
            certificate.Issuer,
            certificate.NotBefore,
            certificate.NotAfter,
            tls.SslProtocol,
            policyErrors,
            chainErrors);
    }

    public static bool IsValidHost(string host)
    {
        if (string.IsNullOrWhiteSpace(host) || host.Any(char.IsWhiteSpace))
        {
            return false;
        }

        return Uri.CheckHostName(host) is UriHostNameType.Dns or UriHostNameType.IPv4 or UriHostNameType.IPv6 ||
            IPAddress.TryParse(host, out _);
    }
}
