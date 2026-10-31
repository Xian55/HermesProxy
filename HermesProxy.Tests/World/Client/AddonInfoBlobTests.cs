using System;
using System.Buffers.Binary;
using System.IO;
using System.IO.Compression;
using HermesProxy.World.Client;
using Xunit;

namespace HermesProxy.Tests.World.Client;

/// <summary>
/// The addon list in CMSG_AUTH_SESSION ends with a uint32 after the addons, and a server reads it
/// whether there are addons or not. Without it the read runs past the end: AzerothCore logged
/// "Addon packet read error" on every login through the proxy, and a core with no guard around
/// that read has no such way out (#346).
/// </summary>
public sealed class AddonInfoBlobTests
{
    [Fact]
    public void EmptyAddonInfoBlob_ReadAsAServerDoes_HoldsNoAddonsThenTheTime()
    {
        byte[] blob = WorldClient.EmptyAddonInfoBlob;

        uint size = BinaryPrimitives.ReadUInt32LittleEndian(blob);
        byte[] list = new byte[size];
        using (var inflater = new ZLibStream(new MemoryStream(blob, sizeof(uint), blob.Length - sizeof(uint)), CompressionMode.Decompress))
            inflater.ReadExactly(list);

        Assert.Equal(0u, BinaryPrimitives.ReadUInt32LittleEndian(list));
        Assert.Equal(2 * sizeof(uint), list.Length);
    }
}
