using System.Globalization;

namespace Tools365.Technology.CidrCalculator;

public sealed record CidrCalculation(
    string Address,
    int PrefixLength,
    string NetworkAddress,
    string BroadcastAddress,
    string SubnetMask,
    string WildcardMask,
    string FirstUsableAddress,
    string LastUsableAddress,
    ulong TotalAddresses,
    ulong UsableHosts);

public static class CidrCalculator
{
    public static bool TryCalculate(string input, out CidrCalculation? calculation, out string error)
    {
        calculation = null;
        var parts = input.Trim().Split('/');
        if (parts.Length != 2 || !TryParseIpv4(parts[0], out var address) ||
            !int.TryParse(parts[1], NumberStyles.None, CultureInfo.InvariantCulture, out var prefixLength) ||
            prefixLength is < 0 or > 32)
        {
            error = "Enter an IPv4 address and prefix length in CIDR notation, for example 192.168.1.10/24.";
            return false;
        }

        var mask = prefixLength == 0 ? 0u : uint.MaxValue << (32 - prefixLength);
        var network = address & mask;
        var broadcast = network | ~mask;
        var totalAddresses = 1UL << (32 - prefixLength);
        var usableHosts = prefixLength switch
        {
            <= 30 => totalAddresses - 2,
            31 => 2UL,
            _ => 1UL
        };

        var firstUsable = prefixLength <= 30 ? network + 1 : network;
        var lastUsable = prefixLength <= 30 ? broadcast - 1 : broadcast;
        calculation = new CidrCalculation(
            FormatIpv4(address),
            prefixLength,
            FormatIpv4(network),
            FormatIpv4(broadcast),
            FormatIpv4(mask),
            FormatIpv4(~mask),
            FormatIpv4(firstUsable),
            FormatIpv4(lastUsable),
            totalAddresses,
            usableHosts);
        error = string.Empty;
        return true;
    }

    private static bool TryParseIpv4(string value, out uint address)
    {
        address = 0;
        var octets = value.Split('.');
        if (octets.Length != 4)
        {
            return false;
        }

        foreach (var octet in octets)
        {
            if (!byte.TryParse(octet, NumberStyles.None, CultureInfo.InvariantCulture, out var valueByte))
            {
                address = 0;
                return false;
            }

            address = (address << 8) | valueByte;
        }

        return true;
    }

    private static string FormatIpv4(uint address) =>
        $"{address >> 24}.{(address >> 16) & 0xFF}.{(address >> 8) & 0xFF}.{address & 0xFF}";
}
