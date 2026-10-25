using Microsoft.UI.Xaml.Controls;

namespace SreToolbox.Views;

public sealed partial class CidrPage : Page
{
    public CidrPage()
    {
        this.InitializeComponent();
        var rows = new List<string>
        {
            "Prefix   Netmask             Addresses        Usable hosts",
        };
        for (int p = 32; p >= 8; p--)
        {
            uint mask = p == 0 ? 0u : uint.MaxValue << (32 - p);
            ulong total = 1UL << (32 - p);
            ulong usable = total > 2 ? total - 2 : 0;
            rows.Add($"/{p,-7} {ToIp(mask),-18} {total,16:N0} {usable,16:N0}");
        }
        RowsList.ItemsSource = rows;
    }

    private static string ToIp(uint v) =>
        $"{(v >> 24) & 0xFF}.{(v >> 16) & 0xFF}.{(v >> 8) & 0xFF}.{v & 0xFF}";
}
