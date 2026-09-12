using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's guild- and arena-charter CMSGs.
/// </summary>
/// <remarks>
/// <see cref="HandlePetitionBuy"/> is the one worth reading: the legacy packet carries a long tail
/// of fields the modern client no longer sends, so most of it is literal zeroes, laid out
/// differently either side of V3_0_2. The zeroes are load-bearing padding, not placeholders to be
/// tidied — the server reads positionally.
/// </remarks>
public static class PetitionSystem
{
    [HandlesCmsg(Opcode.CMSG_PETITION_BUY)]
    public static void HandlePetitionBuy(in PetitionBuy petition, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PETITION_BUY);
        packet.WriteGuid(petition.Unit.To64());
        packet.WriteUInt32(0);
        packet.WriteUInt64(0);
        packet.WriteCString(petition.Title);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteCString("");

        packet.WriteUInt32(0);
        packet.WriteUInt32(0);
        packet.WriteUInt32(0);
        packet.WriteUInt32(0);
        packet.WriteUInt32(0);
        packet.WriteUInt32(0);
        packet.WriteUInt32(0);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
            packet.WriteUInt16(0);

        packet.WriteUInt32(0);
        packet.WriteUInt32(0);
        packet.WriteUInt32(0);

        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V3_0_2_9056))
        {
            for (var i = 0; i < 10; i++)
                packet.WriteCString("");
        }
        else
        {
            packet.WriteUInt16(0);
            packet.WriteUInt8(0);
        }

        packet.WriteUInt32(petition.Index);
        packet.WriteUInt32(0);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PETITION_SHOW_SIGNATURES)]
    public static void HandlePetitionShowSignatures(in PetitionShowSignatures petition, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_PETITION_SHOW_SIGNATURES);
        packet.WriteGuid(petition.Item.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_QUERY_PETITION)]
    public static void HandleQueryPetition(in QueryPetition petition, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_QUERY_PETITION);
        packet.WriteUInt32(petition.PetitionID);
        packet.WriteGuid(petition.ItemGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_PETITION_RENAME_GUILD)]
    public static void HandlePetitionRenameGuild(in PetitionRenameGuild petition, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_PETITION_RENAME);
        packet.WriteGuid(petition.PetitionGuid.To64());
        packet.WriteCString(petition.NewGuildName);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_OFFER_PETITION)]
    public static void HandleOfferPetition(in OfferPetition petition, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_OFFER_PETITION);
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
            packet.WriteUInt32(petition.UnkInt);
        packet.WriteGuid(petition.ItemGUID.To64());
        packet.WriteGuid(petition.TargetPlayer.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_DECLINE_PETITION)]
    public static void HandleDeclinePetition(in DeclinePetition petition, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.MSG_PETITION_DECLINE);
        packet.WriteGuid(petition.PetitionGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_SIGN_PETITION)]
    public static void HandleSignPetition(in SignPetition petition, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_SIGN_PETITION);
        packet.WriteGuid(petition.PetitionGUID.To64());
        packet.WriteUInt8(petition.Choice);
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_TURN_IN_PETITION)]
    public static void HandleTurnInPetition(in TurnInPetition petition, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(Opcode.CMSG_TURN_IN_PETITION);
        packet.WriteGuid(petition.Item.To64());
        if (LegacyVersion.AddedInVersion(ClientVersionBuild.V2_0_1_6180))
        {
            packet.WriteUInt32(petition.BackgroundColor);
            packet.WriteUInt32(petition.EmblemStyle);
            packet.WriteUInt32(petition.EmblemColor);
            packet.WriteUInt32(petition.BorderStyle);
            packet.WriteUInt32(petition.BorderColor);
        }
        ctx.SendPacketToServer(packet);
    }
}
