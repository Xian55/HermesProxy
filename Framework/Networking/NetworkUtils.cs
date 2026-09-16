using System;
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

    /// <summary>
    /// Ask the OS to probe an idle connection, so a peer whose machine went away surfaces as a
    /// closed socket instead of leaving a read pending until TCP retransmits give up (minutes).
    /// Best effort: the per-probe options are not available on every platform, and a connection
    /// without them still works, it just notices a dead peer later.
    /// </summary>
    public static void EnableKeepAlive(Socket socket, int idleSeconds, int intervalSeconds, int retryCount)
    {
        socket.SetSocketOption(SocketOptionLevel.Socket, SocketOptionName.KeepAlive, true);
        TrySet(SocketOptionName.TcpKeepAliveTime, idleSeconds);
        TrySet(SocketOptionName.TcpKeepAliveInterval, intervalSeconds);
        TrySet(SocketOptionName.TcpKeepAliveRetryCount, retryCount);

        void TrySet(SocketOptionName option, int value)
        {
            try
            {
                socket.SetSocketOption(SocketOptionLevel.Tcp, option, value);
            }
            catch (SocketException) { }
            catch (PlatformNotSupportedException) { }
        }
    }
}
