using System.Net;
using System.Net.Sockets;

namespace Framework.Networking;

public static class NetworkUtils
{
    /// Forces IPv4 result or exception
    public static IPAddress ResolveOrDirectIPv4(string hostOrIpaddress)
    {
        if (IPAddress.TryParse(hostOrIpaddress, out IPAddress? result) && result.AddressFamily == AddressFamily.InterNetwork)
        {
            if (IPAddress.IsLoopback(result))
                return IPAddress.Loopback;

            return result;
        }

        return Dns.GetHostAddresses(hostOrIpaddress, AddressFamily.InterNetwork)[0];
    }

    /// Forces IPv4 or IPv6 result or exception
    public static IPAddress ResolveOrDirectIPv64(string hostOrIpaddress)
    {
        if (IPAddress.TryParse(hostOrIpaddress, out IPAddress? result))
        {
            if (IPAddress.IsLoopback(result))
                return IPAddress.Loopback;

            return result;
        }

        return Dns.GetHostAddresses(hostOrIpaddress)[0];
    }
}
