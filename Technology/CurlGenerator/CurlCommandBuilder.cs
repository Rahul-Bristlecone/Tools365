namespace Tools365.Technology.CurlGenerator;

internal sealed record CurlCommandResult(string Command, string ErrorMessage = "")
{
    public bool IsValid => ErrorMessage.Length == 0;
}

internal static class CurlCommandBuilder
{
    private static readonly HashSet<string> SupportedMethods = new(StringComparer.Ordinal)
    {
        "GET",
        "POST",
        "PUT",
        "PATCH",
        "DELETE",
        "HEAD",
        "OPTIONS"
    };

    public static CurlCommandResult Build(
        string method,
        string url,
        string headers,
        string body,
        bool followRedirects,
        bool failOnHttpError)
    {
        method = method.Trim().ToUpperInvariant();
        if (!SupportedMethods.Contains(method))
        {
            return Invalid("Choose a supported HTTP method.");
        }

        url = url.Trim();
        if (!Uri.TryCreate(url, UriKind.Absolute, out var parsedUrl) ||
            parsedUrl.Scheme is not ("http" or "https"))
        {
            return Invalid("Enter a valid absolute HTTP or HTTPS URL.");
        }

        var parsedHeaders = new List<string>();
        foreach (var rawHeader in headers.Split(['\r', '\n'], StringSplitOptions.RemoveEmptyEntries))
        {
            var header = rawHeader.Trim();
            var separator = header.IndexOf(':');
            if (separator <= 0)
            {
                return Invalid($"Header must use the “Name: value” format: {header}");
            }

            var name = header[..separator].Trim();
            var value = header[(separator + 1)..].Trim();
            if (name.Length == 0 || name.Any(character => !IsHeaderNameCharacter(character)))
            {
                return Invalid($"“{name}” is not a valid HTTP header name.");
            }

            if (value.Contains('\r') || value.Contains('\n'))
            {
                return Invalid($"Header “{name}” contains an invalid line break.");
            }

            parsedHeaders.Add($"{name}: {value}");
        }

        var arguments = new List<string> { "curl" };
        if (failOnHttpError)
        {
            arguments.Add("--fail-with-body");
        }

        if (followRedirects)
        {
            arguments.Add("--location");
        }

        arguments.Add("--request");
        arguments.Add(QuoteForPosixShell(method));
        arguments.Add("--url");
        arguments.Add(QuoteForPosixShell(url));
        foreach (var header in parsedHeaders)
        {
            arguments.Add("--header");
            arguments.Add(QuoteForPosixShell(header));
        }

        if (body.Length > 0)
        {
            arguments.Add("--data-raw");
            arguments.Add(QuoteForPosixShell(body));
        }

        return new CurlCommandResult(string.Join(' ', arguments));
    }

    private static bool IsHeaderNameCharacter(char character) =>
        char.IsAsciiLetterOrDigit(character) || "!#$%&'*+-.^_`|~".Contains(character);

    private static string QuoteForPosixShell(string value) =>
        $"'{value.Replace("'", "'\"'\"'", StringComparison.Ordinal)}'";

    private static CurlCommandResult Invalid(string message) =>
        new(string.Empty, message);
}
