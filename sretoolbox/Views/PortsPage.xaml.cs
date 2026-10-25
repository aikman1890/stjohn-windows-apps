using Microsoft.UI.Xaml.Controls;
using SreToolbox.Models;

namespace SreToolbox.Views;

public sealed partial class PortsPage : Page
{
    private static readonly PortEntry[] AllPorts =
    {
        new() { Port = 20, Transport = "tcp", Service = "FTP (data)", Description = "File Transfer Protocol - data channel" },
        new() { Port = 21, Transport = "tcp", Service = "FTP (control)", Description = "File Transfer Protocol - command channel" },
        new() { Port = 22, Transport = "tcp", Service = "SSH", Description = "Secure shell - remote login, SFTP, SCP, tunneling" },
        new() { Port = 23, Transport = "tcp", Service = "Telnet", Description = "Unencrypted remote login - legacy, avoid" },
        new() { Port = 25, Transport = "tcp", Service = "SMTP", Description = "Mail transfer between servers" },
        new() { Port = 53, Transport = "tcp/udp", Service = "DNS", Description = "Domain name resolution" },
        new() { Port = 67, Transport = "udp", Service = "DHCP (server)", Description = "Dynamic host configuration - server side" },
        new() { Port = 68, Transport = "udp", Service = "DHCP (client)", Description = "Dynamic host configuration - client side" },
        new() { Port = 80, Transport = "tcp", Service = "HTTP", Description = "Unencrypted web traffic" },
        new() { Port = 110, Transport = "tcp", Service = "POP3", Description = "Post Office Protocol - mail retrieval" },
        new() { Port = 111, Transport = "tcp/udp", Service = "RPCbind", Description = "ONC RPC portmapper, used by NFS" },
        new() { Port = 143, Transport = "tcp", Service = "IMAP", Description = "Internet Message Access Protocol - mail retrieval" },
        new() { Port = 161, Transport = "udp", Service = "SNMP", Description = "Simple Network Management Protocol - monitoring" },
        new() { Port = 389, Transport = "tcp/udp", Service = "LDAP", Description = "Directory access (Active Directory, OpenLDAP)" },
        new() { Port = 443, Transport = "tcp", Service = "HTTPS", Description = "Encrypted web traffic (TLS)" },
        new() { Port = 445, Transport = "tcp", Service = "SMB", Description = "Windows file sharing (Server Message Block)" },
        new() { Port = 514, Transport = "udp", Service = "Syslog", Description = "Remote log collection" },
        new() { Port = 587, Transport = "tcp", Service = "SMTP submission", Description = "Mail submission from clients (STARTTLS)" },
        new() { Port = 636, Transport = "tcp", Service = "LDAPS", Description = "LDAP over TLS" },
        new() { Port = 873, Transport = "tcp", Service = "rsync", Description = "File synchronization daemon" },
        new() { Port = 993, Transport = "tcp", Service = "IMAPS", Description = "IMAP over TLS" },
        new() { Port = 995, Transport = "tcp", Service = "POP3S", Description = "POP3 over TLS" },
        new() { Port = 1194, Transport = "udp", Service = "OpenVPN", Description = "OpenVPN tunnel endpoint" },
        new() { Port = 1723, Transport = "tcp", Service = "PPTP", Description = "Point-to-Point Tunneling Protocol - legacy VPN" },
        new() { Port = 2049, Transport = "tcp/udp", Service = "NFS", Description = "Network File System" },
        new() { Port = 3306, Transport = "tcp", Service = "MySQL", Description = "MySQL / MariaDB database" },
        new() { Port = 3389, Transport = "tcp/udp", Service = "RDP", Description = "Remote Desktop Protocol (Windows)" },
        new() { Port = 5060, Transport = "tcp/udp", Service = "SIP", Description = "Session Initiation Protocol - VoIP signaling" },
        new() { Port = 5432, Transport = "tcp", Service = "PostgreSQL", Description = "PostgreSQL database" },
        new() { Port = 5900, Transport = "tcp", Service = "VNC", Description = "Virtual Network Computing - remote display" },
        new() { Port = 6379, Transport = "tcp", Service = "Redis", Description = "Redis key-value store" },
        new() { Port = 8080, Transport = "tcp", Service = "HTTP (alternate)", Description = "Alternate / proxy HTTP port" },
        new() { Port = 8443, Transport = "tcp", Service = "HTTPS (alternate)", Description = "Alternate HTTPS port" },
        new() { Port = 9100, Transport = "tcp", Service = "Node Exporter / printing", Description = "Prometheus node_exporter metrics; also raw printer port (JetDirect)" },
        new() { Port = 27017, Transport = "tcp", Service = "MongoDB", Description = "MongoDB database" },
    };

    public PortsPage()
    {
        this.InitializeComponent();
        PortsList.ItemsSource = AllPorts;
    }

    private void SearchBox_TextChanged(object sender, TextChangedEventArgs e)
    {
        var q = SearchBox.Text.Trim();
        if (q.Length == 0)
        {
            PortsList.ItemsSource = AllPorts;
            return;
        }
        PortsList.ItemsSource = AllPorts.Where(p =>
            p.Port.ToString().Contains(q) ||
            p.Service.Contains(q, StringComparison.OrdinalIgnoreCase) ||
            p.Description.Contains(q, StringComparison.OrdinalIgnoreCase)).ToList();
    }
}
