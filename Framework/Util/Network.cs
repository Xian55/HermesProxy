using System.Buffers.Binary;

namespace Framework.Util
{
    public static class NetworkUtility
    {
        public static uint EndianConvert(uint value)
        {
            return BinaryPrimitives.ReverseEndianness(value);
        }
        public static ushort EndianConvert(ushort value)
        {
            return BinaryPrimitives.ReverseEndianness(value);
        }
    }
}
