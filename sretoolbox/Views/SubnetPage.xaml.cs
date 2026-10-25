using Microsoft.UI.Xaml;
using Microsoft.UI.Xaml.Controls;

namespace SreToolbox.Views;

public sealed partial class SubnetPage : Page
{
    public SubnetPage()
    {
        this.InitializeComponent();
    }

    private void CalcButton_Click(object sender, RoutedEventArgs e)
    {
        ErrorText.Text = string.Empty;
        ResultsText.Text = string.Empty;

        if (!TryParseIPv4(IpBox.Text.Trim(), out uint ip))
        {
            ErrorText.Text = "Enter a valid IPv4 address, e.g. 192.168.1.10.";
            return;
        }
        if (!int.TryParse(PrefixBox.Text.Trim(), out int prefix) || prefix < 0 || prefix > 32)
        {
            ErrorText.Text = "Prefix length must be a number from 0 to 32.";
            return;
        }

        uint mask = prefix == 0 ? 0u : uint.MaxValue << (32 - prefix);
        uint network = ip & mask;
        uint broadcast = network | ~mask;
        ulong total = 1UL << (32 - prefix);
        ulong usable = total > 2 ? total - 2 : 0;
        uint first = total > 2 ? network + 1 : network;
        uint last = total > 2 ? broadcast - 1 : broadcast;

        var lines = new[]
        {
            $"Address:          {ToIp(ip)}",
            $"Netmask:          {ToIp(mask)}",
            $"Wildcard mask:    {ToIp(~mask)}",
            $"Network:          {ToIp(network)}/{prefix}",
            $"Broadcast:        {ToIp(broadcast)}",
            $"First host:       {ToIp(first)}",
            $"Last host:        {ToIp(last)}",
            $"Total addresses:  {total:N0}",
            $"Usable hosts:     {usable:N0}",
        };
        ResultsText.Text = string.Join(Environment.NewLine, lines);
    }

    private static bool TryParseIPv4(string text, out uint value)
    {
        value = 0;
        var parts = text.Split('.');
        if (parts.Length != 4) return false;
        uint result = 0;
        foreach (var part in parts)
        {
            if (!byte.TryParse(part, out byte octet)) return false;
            result = (result << 8) | octet;
        }
        value = result;
        return true;
    }

    private static string ToIp(uint v) =>
        $"{(v >> 24) & 0xFF}.{(v >> 16) & 0xFF}.{(v >> 8) & 0xFF}.{v & 0xFF}";
}
