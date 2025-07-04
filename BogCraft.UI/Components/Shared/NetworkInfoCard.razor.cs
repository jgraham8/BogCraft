using System.Net;
using Microsoft.AspNetCore.Components;

namespace BogCraft.UI.Components.Shared;

public partial class NetworkInfoCard : ComponentBase
{
    private string GetLocalIP()
    {
        try
        {
            var host = Dns.GetHostEntry(Dns.GetHostName());
            var localIP = host.AddressList
                .FirstOrDefault(ip => ip.AddressFamily == System.Net.Sockets.AddressFamily.InterNetwork && !IPAddress.IsLoopback(ip));
            return localIP?.ToString() ?? "localhost";
        }
        catch
        {
            return "localhost";
        }
    }
}