using System.Buffers.Binary;

namespace Framework.Util;

public static class NetworkUtility
{
    public static ushort EndianConvert(ushort value)
    {
        return BinaryPrimitives.ReverseEndianness(value);
    }
}
