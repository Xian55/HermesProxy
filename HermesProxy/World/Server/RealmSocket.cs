using System.Net.Sockets;
using HermesProxy.Configuration.Options;
using Microsoft.Extensions.Options;

namespace HermesProxy.World.Server;

public sealed class RealmSocket : WorldSocket
{
    public RealmSocket(Socket socket, IOptions<ProxyNetworkOptions> networkOptions)
        : base(socket, networkOptions)
    {
    }
}
