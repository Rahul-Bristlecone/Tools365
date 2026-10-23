using System.Net;
using System.Security.Cryptography;
using System.Security.Cryptography.X509Certificates;
using System.Globalization;

namespace Tools365.Technology.TlsCertificateGenerator;

internal sealed record GeneratedTlsCertificate(
    string CertificatePem,
    string EncryptedPrivateKeyPem,
    DateTimeOffset NotAfter);

internal static class SelfSignedTlsCertificateFactory
{
    public static GeneratedTlsCertificate Generate(
        string primaryName,
        string additionalNames,
        double validityDays,
        string password)
    {
        if (string.IsNullOrWhiteSpace(primaryName))
        {
            throw new ArgumentException("Enter the primary DNS name.", nameof(primaryName));
        }

        if (!double.IsFinite(validityDays) ||
            validityDays < 1 ||
            validityDays > 397 ||
            validityDays != Math.Truncate(validityDays))
        {
            throw new ArgumentException("Validity must be a whole number from 1 to 397 days.", nameof(validityDays));
        }

        if (password.Length < 12)
        {
            throw new ArgumentException("Use a private-key password with at least 12 characters.", nameof(password));
        }

        var normalizedPrimaryName = NormalizeDnsName(primaryName.Trim());
        var dnsNames = new HashSet<string>(StringComparer.OrdinalIgnoreCase) { normalizedPrimaryName };
        foreach (var name in additionalNames.Split(',', StringSplitOptions.TrimEntries | StringSplitOptions.RemoveEmptyEntries))
        {
            dnsNames.Add(NormalizeDnsName(name));
        }

        using var rsa = RSA.Create(2048);
        var subject = new X500DistinguishedName($"CN={EscapeDistinguishedNameValue(normalizedPrimaryName)}");
        var request = new CertificateRequest(subject, rsa, HashAlgorithmName.SHA256, RSASignaturePadding.Pkcs1);
        request.CertificateExtensions.Add(new X509BasicConstraintsExtension(false, false, 0, true));
        request.CertificateExtensions.Add(new X509KeyUsageExtension(
            X509KeyUsageFlags.DigitalSignature | X509KeyUsageFlags.KeyEncipherment,
            true));
        request.CertificateExtensions.Add(new X509EnhancedKeyUsageExtension(
            new OidCollection { new("1.3.6.1.5.5.7.3.1") },
            true));

        var subjectAlternativeNames = new SubjectAlternativeNameBuilder();
        foreach (var name in dnsNames)
        {
            if (IPAddress.TryParse(name, out var address))
            {
                subjectAlternativeNames.AddIpAddress(address);
            }
            else
            {
                subjectAlternativeNames.AddDnsName(name);
            }
        }

        request.CertificateExtensions.Add(subjectAlternativeNames.Build());

        var notBefore = DateTimeOffset.UtcNow.AddMinutes(-5);
        var notAfter = notBefore.AddDays(validityDays);
        using var certificate = request.CreateSelfSigned(notBefore, notAfter);
        var encryptedKey = rsa.ExportEncryptedPkcs8PrivateKeyPem(
            password,
            new PbeParameters(PbeEncryptionAlgorithm.Aes256Cbc, HashAlgorithmName.SHA256, 100_000));

        return new GeneratedTlsCertificate(certificate.ExportCertificatePem(), encryptedKey, notAfter);
    }

    private static string NormalizeDnsName(string name)
    {
        if (IPAddress.TryParse(name, out var address))
        {
            return address.ToString();
        }

        var wildcardPrefix = name.StartsWith("*.", StringComparison.Ordinal);
        var candidate = wildcardPrefix ? name[2..] : name;
        if (candidate.Length == 0 || candidate.Any(char.IsWhiteSpace))
        {
            throw new ArgumentException($"“{name}” is not a valid DNS name.");
        }

        string asciiName;
        try
        {
            asciiName = new IdnMapping().GetAscii(candidate);
        }
        catch (ArgumentException exception)
        {
            throw new ArgumentException($"“{name}” is not a valid DNS name.", exception);
        }

        if (Uri.CheckHostName(asciiName) != UriHostNameType.Dns)
        {
            throw new ArgumentException($"“{name}” is not a valid DNS name.");
        }

        return wildcardPrefix ? $"*.{asciiName}" : asciiName;
    }

    private static string EscapeDistinguishedNameValue(string value) =>
        value.Replace("\\", "\\\\", StringComparison.Ordinal)
            .Replace(",", "\\,", StringComparison.Ordinal)
            .Replace("+", "\\+", StringComparison.Ordinal)
            .Replace("\"", "\\\"", StringComparison.Ordinal)
            .Replace("<", "\\<", StringComparison.Ordinal)
            .Replace(">", "\\>", StringComparison.Ordinal)
            .Replace(";", "\\;", StringComparison.Ordinal)
            .Replace("=", "\\=", StringComparison.Ordinal);
}
