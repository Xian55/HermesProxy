using System;
using System.Runtime.CompilerServices;
using Framework.IO;

namespace HermesProxy.World.Server.Packets;

internal static class CodecHelpers
{
    /// <summary>
    /// Capacity to pre-size a wire-counted collection with.
    /// </summary>
    /// <remarks>
    /// The count is read from the packet, so pre-sizing on it directly hands a corrupt or hostile
    /// count a multi-gigabyte allocation that happens <em>before</em> the first element read can
    /// fail. The old ClientPacket readers started empty and grew, so they hit the end of the buffer
    /// and threw long before allocating anything large; pre-sizing is the optimisation that removes
    /// that accident.
    ///
    /// No element occupies fewer than <paramref name="minElementBytes"/> bytes, so what is left in
    /// the buffer is a hard ceiling on how many can actually follow. Clamping to it keeps the
    /// pre-size for every real packet and costs a divide. A count above the ceiling is not rejected
    /// here — the element loop still runs and still throws on underflow, which is where a truncated
    /// packet should be reported.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    internal static int WireCountCapacity(uint count, in SpanPacketReader r, int minElementBytes)
        => (int)Math.Min(count, (uint)(r.Remaining / minElementBytes));
}
