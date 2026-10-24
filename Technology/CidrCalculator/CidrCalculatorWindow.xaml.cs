using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;
using System.Globalization;
using System.Text;
using Windows.ApplicationModel.DataTransfer;
using Windows.Graphics;

namespace Tools365.Technology.CidrCalculator;

public sealed partial class CidrCalculatorWindow : Window
{
    private CidrCalculation? _calculation;

    public CidrCalculatorWindow()
    {
        InitializeComponent();
        AppWindow.Resize(new SizeInt32(620, 700));
        Calculate();
    }

    private void CidrInputBox_TextChanged(object sender, TextChangedEventArgs e) => Calculate();

    private void Calculate()
    {
        if (!CidrCalculator.TryCalculate(CidrInputBox.Text, out var calculation, out var error))
        {
            _calculation = null;
            ResultsPanel.Visibility = Visibility.Collapsed;
            CopyButton.IsEnabled = false;
            ValidationBar.Title = "Enter a valid IPv4 CIDR";
            ValidationBar.Message = error;
            ValidationBar.IsOpen = true;
            return;
        }

        _calculation = calculation;
        ValidationBar.IsOpen = false;
        ResultsPanel.Visibility = Visibility.Visible;
        NetworkAddressText.Text = $"{calculation!.NetworkAddress}/{calculation.PrefixLength}";
        BroadcastAddressText.Text = calculation.BroadcastAddress;
        SubnetMaskText.Text = calculation.SubnetMask;
        WildcardMaskText.Text = calculation.WildcardMask;
        FirstUsableAddressText.Text = calculation.FirstUsableAddress;
        LastUsableAddressText.Text = calculation.LastUsableAddress;
        TotalAddressesText.Text = calculation.TotalAddresses.ToString("N0", CultureInfo.InvariantCulture);
        UsableHostsText.Text = calculation.UsableHosts.ToString("N0", CultureInfo.InvariantCulture);
        CopyButton.IsEnabled = true;
    }

    private void CopyButton_Click(object sender, RoutedEventArgs e)
    {
        if (_calculation is null)
        {
            return;
        }

        var text = new StringBuilder()
            .AppendLine($"Address: {_calculation.Address}/{_calculation.PrefixLength}")
            .AppendLine($"Network address: {_calculation.NetworkAddress}")
            .AppendLine($"Broadcast address: {_calculation.BroadcastAddress}")
            .AppendLine($"Subnet mask: {_calculation.SubnetMask}")
            .AppendLine($"Wildcard mask: {_calculation.WildcardMask}")
            .AppendLine($"First usable address: {_calculation.FirstUsableAddress}")
            .AppendLine($"Last usable address: {_calculation.LastUsableAddress}")
            .AppendLine($"Total addresses: {_calculation.TotalAddresses}")
            .Append($"Usable hosts: {_calculation.UsableHosts}")
            .ToString();

        var package = new DataPackage();
        package.SetText(text);
        Clipboard.SetContent(package);
    }
}
