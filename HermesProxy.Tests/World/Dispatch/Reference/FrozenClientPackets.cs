using HermesProxy.World;

namespace HermesProxy.Tests.World.Dispatch.Reference;

/// <summary>
/// Byte-for-byte copies of the <c>ClientPacket.Read()</c> bodies as they were immediately before
/// each packet was converted to a <c>readonly record struct</c> + codec.
/// </summary>
/// <remarks>
/// <para>
/// <b>Do not "fix" anything in this file.</b> Its entire value is that it does not change. These
/// are the oracle the codecs are proven against; editing one to match a codec would turn the
/// equivalence test into a tautology.
/// </para>
/// <para>
/// A wire regression on this path does not throw — the client silently drops or mis-renders the
/// packet — so the only way to know a conversion preserved the layout is to run both readers over
/// the same bytes and compare fields. That is what these exist for.
/// </para>
/// <para>
/// Field initializers are copied too, not just the reads. A positional record struct defaults to
/// all-zero, so a field that the original defaulted to something else and <c>Read</c> skips on some
/// path is exactly the regression that would otherwise ship silently.
/// </para>
/// </remarks>
internal static class FrozenClientPackets
{
    /// Frozen from <c>CombatPackets.cs</c> AttackSwing.
    internal sealed class AttackSwing
    {
        public WowGuid128 Victim;

        public void Read(WorldPacket p)
        {
            Victim = p.ReadPackedGuid128();
        }
    }

    /// Frozen from <c>CombatPackets.cs</c> AttackStop — an empty payload.
    internal sealed class AttackStop
    {
        public void Read(WorldPacket p) { }
    }

    /// Frozen from <c>CombatPackets.cs</c> SetSheathed. Note <c>Animate</c> defaulted to true.
    internal sealed class SetSheathed
    {
        public int SheathState;
        public bool Animate = true;

        public void Read(WorldPacket p)
        {
            SheathState = p.ReadInt32();
            Animate = p.HasBit();
        }
    }

    /// Frozen from <c>ItemPackets.cs</c> BuyBackItem.
    internal sealed class BuyBackItem
    {
        public WowGuid128 VendorGUID;
        public uint Slot;

        public void Read(WorldPacket p)
        {
            VendorGUID = p.ReadPackedGuid128();
            Slot = p.ReadUInt32();
        }
    }
}
