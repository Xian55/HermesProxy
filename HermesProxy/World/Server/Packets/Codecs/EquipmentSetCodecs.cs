using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;

namespace HermesProxy.World.Server.Packets;

// Equipment-set CMSG codecs.

public static class SaveEquipmentSetCodec
{
    /// <remarks>
    /// Reads into the shared <see cref="EquipmentSetData"/>, which still owns this layout because
    /// its Write half serves the outbound LoadEquipmentSet. The bit lengths sit after every fixed
    /// field and before the optional spec index, so the two strings are read last.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SaveEquipmentSet packet)
    {
        var set = new EquipmentSetData();
        set.Type = r.ReadInt32();
        set.Guid = r.ReadUInt64();
        set.SetID = r.ReadUInt32();
        set.IgnoreMask = r.ReadUInt32();

        for (int i = 0; i < LoadEquipmentSet.SlotCount; i++)
        {
            set.Pieces[i] = r.ReadPackedGuid128();
            set.Appearances[i] = r.ReadInt32();
        }

        set.Enchants[0] = r.ReadInt32();
        set.Enchants[1] = r.ReadInt32();
        set.SecondaryShoulderApparanceID = r.ReadInt32();
        set.SecondaryShoulderSlot = r.ReadInt32();
        set.SecondaryWeaponAppearanceID = r.ReadInt32();
        set.SecondaryWeaponSlot = r.ReadInt32();

        bool hasSpec = r.ReadBit();
        uint nameLen = r.ReadBits<uint>(8);
        uint iconLen = r.ReadBits<uint>(9);
        r.ResetBitPos();

        if (hasSpec)
            set.AssignedSpecIndex = r.ReadInt32();

        set.SetName = r.ReadString(nameLen);
        set.SetIcon = r.ReadString(iconLen);

        packet = new SaveEquipmentSet(set);
    }
}

public static class DeleteEquipmentSetCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out DeleteEquipmentSet packet)
        => packet = new DeleteEquipmentSet(r.ReadUInt64());
}

public static class UseEquipmentSetCodec
{
    /// <remarks>
    /// The leading 2-bit count describes a variable block of inventory slot pairs that the original
    /// body read and discarded; the nineteen real slots follow at a fixed size, then the set GUID.
    /// The discard has to happen or every slot after it is misaligned.
    /// </remarks>
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out UseEquipmentSet packet)
    {
        uint invCount = r.ReadBits<uint>(2);
        r.ResetBitPos();
        for (uint i = 0; i < invCount; i++)
        {
            r.ReadUInt8();
            r.ReadUInt8();
        }

        EquipmentSetItems items = default;
        for (int i = 0; i < LoadEquipmentSet.SlotCount; i++)
        {
            items[i].Item = r.ReadPackedGuid128();
            items[i].ContainerSlot = r.ReadUInt8();
            items[i].Slot = r.ReadUInt8();
        }

        packet = new UseEquipmentSet(items, r.ReadUInt64());
    }
}
