using System.Collections.Generic;
using System.Runtime.CompilerServices;
using Framework.IO;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;

namespace HermesProxy.World.Server.Packets;

// Character-management CMSG codecs: the character list, creation and deletion, login and logout,
// appearance, action buttons and inspection.
//
// Nearly all of these fire once per session or once per UI action, so the heap-carrying ones keep
// their reference fields — CharacterCreateInfo nests a customization list, and the two name-query
// and reorder packets are count-driven with a client-chosen length. The structs cost nothing; the
// single allocation each survives until the outbound conversion.

public static class ReorderCharactersCodec
{
    public static void Read(ref SpanPacketReader r, out ReorderCharacters packet)
    {
        uint count = r.ReadBits<uint>(9);
        var entries = new ReorderInfo[count];
        for (uint i = 0; i < count; i++)
        {
            WowGuid128 playerGuid = r.ReadPackedGuid128();
            byte newPosition = r.ReadUInt8();
            entries[i] = new ReorderInfo(playerGuid, newPosition);
        }
        packet = new ReorderCharacters(entries);
    }
}

public static class GetAccountCharacterListRequestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GetAccountCharacterListRequest packet)
        => packet = new GetAccountCharacterListRequest(r.ReadUInt32());
}

public static class GenerateRandomCharacterNameRequestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out GenerateRandomCharacterNameRequest packet)
    {
        var race = (Race)r.ReadUInt8();
        var sex = (Gender)r.ReadUInt8();
        packet = new GenerateRandomCharacterNameRequest(race, sex);
    }
}

public static class CreateCharacterCodec
{
    public static void Read(ref SpanPacketReader r, out CreateCharacter packet)
    {
        var createInfo = new CharacterCreateInfo();
        uint nameLength = r.ReadBits<uint>(6);
        bool hasTemplateSet = r.HasBit();
        createInfo.IsTrialBoost = r.HasBit();
        createInfo.UseNPE = r.HasBit();

        createInfo.RaceId = (Race)r.ReadUInt8();
        createInfo.ClassId = (Class)r.ReadUInt8();
        createInfo.Sex = (Gender)r.ReadUInt8();
        var customizationCount = r.ReadUInt32();

        createInfo.Name = r.ReadString(nameLength);
        if (hasTemplateSet)
            createInfo.TemplateSet = r.ReadUInt32();

        for (var i = 0; i < customizationCount; ++i)
        {
            createInfo.Customizations.Add(new ChrCustomizationChoice(r.ReadUInt32(), r.ReadUInt32()));
        }

        createInfo.Customizations.Sort();
        packet = new CreateCharacter(createInfo);
    }
}

public static class CharDeleteCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CharDelete packet)
        => packet = new CharDelete(r.ReadPackedGuid128());
}

public static class LoadingScreenNotifyCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LoadingScreenNotify packet)
    {
        uint mapId = r.ReadUInt32();
        bool showing = r.HasBit();
        packet = new LoadingScreenNotify(mapId, showing);
    }
}

// ---- name queries ----

public static class QueryPlayerNameCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out QueryPlayerName packet)
        => packet = new QueryPlayerName(r.ReadPackedGuid128());
}

public static class QueryPlayerNamesCodec
{
    public static void Read(ref SpanPacketReader r, out QueryPlayerNames packet)
    {
        uint count = r.ReadUInt32();
        var players = new List<WowGuid128>((int)count);
        for (uint i = 0; i < count; i++)
            players.Add(r.ReadPackedGuid128());
        packet = new QueryPlayerNames(players);
    }
}

// ---- session lifecycle ----

public static class PlayerLoginCodec
{
    /// <remarks>
    /// The trailing bit is read on Vanilla and TBC and absent from WotLK Classic onward. This stays
    /// one codec with the branch inline rather than becoming a ranged pair, because the predicate is
    /// an <em>expansion</em> comparison and <c>AddedIn</c>/<c>RemovedIn</c> can only express a build
    /// boundary. Encoding it as <c>RemovedIn = V3_4_3_54261</c> would happen to agree over the
    /// currently supported modern builds and quietly disagree the moment another one is added, which
    /// is the precise failure mode ranged codecs exist to prevent. <c>ExpansionVersion</c> is
    /// <see langword="static readonly"/>, so the JIT folds this to a constant either way.
    /// </remarks>
    public static void Read(ref SpanPacketReader r, out PlayerLogin packet)
    {
        WowGuid128 guid = r.ReadPackedGuid128();
        float farClip = r.ReadFloat();
        // 3.4.3 client doesn't send the trailing bit — packet is exactly Guid+FarClip.
        // Per WPP V3_4_0_45166 SessionHandler.cs:143, gated on V3_4_3_51505+.
        bool unkBit = false;
        if (ModernVersion.ExpansionVersion < 3)
            unkBit = r.HasBit();
        packet = new PlayerLogin(guid, farClip, unkBit);
    }
}

public static class LogoutRequestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out LogoutRequest packet)
        => packet = new LogoutRequest(r.HasBit());
}

public static class RequestPlayedTimeCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out RequestPlayedTime packet)
        => packet = new RequestPlayedTime(r.HasBit());
}

// ---- appearance and titles ----

public static class SetTitleCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetTitle packet)
        => packet = new SetTitle(r.ReadInt32());
}

public static class AlterAppearanceCodec
{
    public static void Read(ref SpanPacketReader r, out AlterAppearance packet)
    {
        var customizationCount = r.ReadUInt32();
        var newSexId = (Gender)r.ReadUInt8();
        var customizedRace = (Race)r.ReadUInt32();
        uint customizedChrModelId = r.ReadUInt32();

        var customizations = new List<ChrCustomizationChoice>(8);
        for (var i = 0; i < customizationCount; ++i)
            customizations.Add(new ChrCustomizationChoice(r.ReadUInt32(), r.ReadUInt32()));

        customizations.Sort();
        packet = new AlterAppearance(newSexId, customizedRace, customizedChrModelId, customizations);
    }
}

public static class SetPvPCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetPvP packet)
        => packet = new SetPvP(r.HasBit());
}

public static class PlayerShowingHelmOrCloakCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out PlayerShowingHelmOrCloak packet)
    {
        r.ResetBitPos();
        packet = new PlayerShowingHelmOrCloak(r.HasBit());
    }
}

// ---- action bar ----

public static class SetActionButtonCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetActionButton packet)
    {
        ushort action = r.ReadUInt16();
        ushort type = r.ReadUInt16();
        byte index = r.ReadUInt8();
        packet = new SetActionButton(action, type, index);
    }
}

public static class SetActionBarTogglesCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out SetActionBarToggles packet)
        => packet = new SetActionBarToggles(r.ReadUInt8());
}

public static class UnlearnSkillCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out UnlearnSkill packet)
        => packet = new UnlearnSkill(r.ReadUInt32());
}

// ---- inspection ----

public static class InspectCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out Inspect packet)
        => packet = new Inspect(r.ReadPackedGuid128());
}

public static class CharacterRenameRequestCodec
{
    [MethodImpl(MethodImplOptions.AggressiveInlining)]
    public static void Read(ref SpanPacketReader r, out CharacterRenameRequest packet)
    {
        WowGuid128 guid = r.ReadPackedGuid128();
        string newName = r.ReadString(r.ReadBits<uint>(6));
        packet = new CharacterRenameRequest(guid, newName);
    }
}
