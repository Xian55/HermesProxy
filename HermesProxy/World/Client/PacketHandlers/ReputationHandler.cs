using Framework;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    // Handlers for SMSG opcodes coming the legacy world server
    [HandlesSmsg(Opcode.SMSG_INITIALIZE_FACTIONS)]
    internal void HandleInitializeFactions(WorldPacket packet)
    {
        if (!GetSession().GameState.IsFirstEnterWorld)
            return;

        InitializeFactions factions = new InitializeFactions();
        uint count = packet.ReadUInt32();
        for (uint i = 0; i < count; i ++)
        {
            factions.FactionFlags[i] = (ReputationFlags)packet.ReadUInt8();
            factions.FactionStandings[i] = packet.ReadInt32();
        }
        SendPacketToClient(factions);

        // This packet does not exist in Vanilla, but it must be sent for client to be able to move.
        if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V2_0_1_6180))
            SendPacketToClient(new TimeSyncRequest());
    }

    [HandlesSmsg(Opcode.SMSG_SET_FACTION_STANDING)]
    internal void HandleSetFactionStanding(WorldPacket packet)
    {
        SetFactionStanding standing = new();
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_4_0_8089))
            packet.ReadFloat(); // Reputation loss

        bool showVisual = true;
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            showVisual = packet.ReadBool();
        standing.ShowVisual = showVisual;

        var count = packet.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            FactionStandingData faction = new();
            faction.Index = packet.ReadInt32();
            faction.Standing = packet.ReadInt32();
            standing.Factions.Add(faction);
        }
        SendPacketToClient(standing);
    }

    [HandlesSmsg(Opcode.SMSG_SET_FORCED_REACTIONS)]
    internal void HandleSetForcedReaction(WorldPacket packet)
    {
        // The packet cannot even be made for a client that has no number for it.
        if (ModernVersion.GetCurrentOpcode(Opcode.SMSG_SET_FORCED_REACTIONS) == 0)
        {
            SetForcedReactionsInPlayerObject(packet);
            return;
        }

        SetForcedReactions reactions = new();
        var count = packet.ReadInt32();
        for (var i = 0; i < count; i++)
        {
            ForcedReaction reaction = new();
            reaction.Faction = packet.ReadInt32();
            reaction.Reaction = packet.ReadInt32();
            reactions.Reactions.Add(reaction);
        }
        SendPacketToClient(reactions);
    }

    // A client without the packet (4.4.2) keeps forced reactions in its player object. The
    // server's list is the whole of them every time: the slots are brought in line with it, and
    // what changed goes out as a change of the player. A player the client has not been given yet
    // is made with them (the server sends the list ahead of the player at login).
    private void SetForcedReactionsInPlayerObject(WorldPacket packet)
    {
        int count = packet.ReadInt32();
        GameSessionData state = GetSession().GameState;
        if (state.ForcedReactionSlots == null && count == 0)
            return;

        Span<ForcedReaction> wanted = count <= 64 ? stackalloc ForcedReaction[64] : new ForcedReaction[count];
        wanted = wanted[..count];
        for (int i = 0; i < count; i++)
        {
            wanted[i].Faction = packet.ReadInt32();
            wanted[i].Reaction = packet.ReadInt32();
        }

        ForcedReaction[] slots = state.ForcedReactionSlots ??= new ForcedReaction[ForcedReactionSlots.Count];
        uint changed = ForcedReactionSlots.Apply(slots, wanted);
        if (changed == 0 || !state.ClientHasPlayerObject)
            return;

        ObjectUpdate updateData = new ObjectUpdate(state.CurrentPlayerGuid, UpdateTypeModern.Values, GetSession());
        ForcedReaction?[] sent = updateData.EnsurePlayerData().EnsureForcedReactions();
        for (int i = 0; i < slots.Length; i++)
        {
            if ((changed & (1u << i)) != 0)
                sent[i] = slots[i];
        }
        UpdateObject updatePacket = new UpdateObject(state);
        updatePacket.ObjectUpdates.Add(updateData);
        SendPacketToClient(updatePacket);
    }

    [HandlesSmsg(Opcode.SMSG_SET_FACTION_VISIBLE)]
    internal void HandleSetFactionVisible(WorldPacket packet)
    {
        SetFactionVisible faction = new(true);
        faction.FactionIndex = packet.ReadUInt32();
        SendPacketToClient(faction);
    }
    [HandlesSmsg(Opcode.SMSG_SET_FACTION_NOT_VISIBLE)]
    internal void HandleSetFactionNotVisible(WorldPacket packet)
    {
        if (!IsCataLegacy)
            return;

        SetFactionVisible faction = new(false);
        faction.FactionIndex = packet.ReadUInt32();
        SendPacketToClient(faction);
    }
}
