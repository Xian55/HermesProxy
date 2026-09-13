using System;
using System.Collections.Generic;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.Tests.World.Dispatch.Reference;

/// <summary>
/// Frozen copies of <c>ClientPacket.Read()</c> bodies, lifted verbatim from the packet classes
/// immediately before each was converted to a <c>readonly record struct</c> + codec.
/// </summary>
/// <remarks>
/// <para>
/// <b>Do not edit.</b> These are the oracle the codecs are proven against; changing one to match a
/// codec turns the equivalence test into a tautology. They were extracted mechanically by
/// <c>freeze-oracles.py</c> rather than transcribed, because a typo here would make the test agree
/// with a bug — worse than having no test.
/// </para>
/// <para>
/// Field initializers are preserved deliberately. A positional record struct defaults to all-zero,
/// so a field the original defaulted to something else and <c>Read</c> skips on some path is exactly
/// the regression that would otherwise ship silently.
/// </para>
/// </remarks>
internal static class FrozenPackets
{
    /// Frozen verbatim from <c>QueryPackets.cs</c>.
    internal sealed class ItemTextQuery
    {
        public void Read(WorldPacket p)
        {
            Id = p.ReadPackedGuid128();
        }

        public WowGuid128 Id = WowGuid128.Empty;
    }

    /// Frozen verbatim from <c>QueryPackets.cs</c>.
    internal sealed class QueryQuestInfo
    {
        public void Read(WorldPacket p)
        {
            QuestID = p.ReadUInt32();
            QuestGiver = p.ReadPackedGuid128();
        }

        public WowGuid128 QuestGiver;
        public uint QuestID;
    }

    /// Frozen verbatim from <c>QueryPackets.cs</c>.
    internal sealed class QueryCreature
    {
        public void Read(WorldPacket p)
        {
            CreatureID = p.ReadUInt32();
        }

        public uint CreatureID;
    }

    /// Frozen verbatim from <c>QueryPackets.cs</c>.
    internal sealed class QueryGameObject
    {
        public void Read(WorldPacket p)
        {
            GameObjectID = p.ReadUInt32();
            Guid = p.ReadPackedGuid128();
        }

        public uint GameObjectID;
        public WowGuid128 Guid;
    }

    /// Frozen verbatim from <c>QueryPackets.cs</c>.
    internal sealed class QueryPageText
    {
        public void Read(WorldPacket p)
        {
            PageTextID = p.ReadUInt32();
            ItemGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 ItemGUID;
        public uint PageTextID;
    }

    /// Frozen verbatim from <c>QueryPackets.cs</c>.
    internal sealed class QueryNPCText
    {
        public void Read(WorldPacket p)
        {
            TextID = p.ReadUInt32();
            Guid = p.ReadPackedGuid128();
        }

        public WowGuid128 Guid;
        public uint TextID;
    }

    /// Frozen verbatim from <c>QueryPackets.cs</c>.
    internal sealed class QueryPetName
    {
        public void Read(WorldPacket p)
        {
            UnitGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 UnitGUID;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class TimeSyncResponse
    {
        public void Read(WorldPacket p)
        {
            SequenceIndex = p.ReadUInt32();
            ClientTime = p.ReadUInt32();
        }

        public uint ClientTime; // Client ticks in ms
        public uint SequenceIndex; // Same index as in request
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class AreaTriggerPkt
    {
        public void Read(WorldPacket p)
        {
            AreaTriggerID = p.ReadUInt32();
            Entered = p.HasBit();
            FromClient = p.HasBit();
        }

        public uint AreaTriggerID;
        public bool Entered;
        public bool FromClient;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class SetSelection
    {
        public void Read(WorldPacket p)
        {
            TargetGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 TargetGUID;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class RepopRequest
    {
        public void Read(WorldPacket p)
        {
            CheckInstance = p.HasBit();
        }

        public bool CheckInstance;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class QueryCorpseLocationFromClient
    {
        public void Read(WorldPacket p)
        {
            Player = p.ReadPackedGuid128();
        }

        public WowGuid128 Player;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class ReclaimCorpse
    {
        public void Read(WorldPacket p)
        {
            CorpseGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 CorpseGUID;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class StandStateChange
    {
        public void Read(WorldPacket p)
        {
            StandState = p.ReadUInt32();
        }

        public uint StandState;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class ClientCinematicPkt
    {
        public void Read(WorldPacket p) { }
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class FarSight
    {
        public void Read(WorldPacket p)
        {
            Enable = p.HasBit();
        }

        public bool Enable;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class TutorialSetFlag
    {
        public void Read(WorldPacket p)
        {
            Action = (TutorialAction)p.ReadBits<byte>(2);
            if (Action == TutorialAction.Update)
                TutorialBit = p.ReadUInt32();
        }

        public TutorialAction Action;
        public uint TutorialBit;
    }

    /// Frozen verbatim from <c>UpdatePackets.cs</c>.
    internal sealed class ObjectUpdateFailed
    {
        public void Read(WorldPacket p)
        {
            ObjectGuid = p.ReadPackedGuid128();
        }

        public WowGuid128 ObjectGuid;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class SetDungeonDifficulty
    {
        public void Read(WorldPacket p)
        {
            DifficultyID = p.ReadUInt32();
        }

        public uint DifficultyID;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class SetRaidDifficulty
    {
        public void Read(WorldPacket p)
        {
            DifficultyID = p.ReadInt32();
            if (p.CanRead())
                Legacy = p.ReadUInt8();
        }

        public int DifficultyID;
        public byte Legacy;
    }

    /// Frozen verbatim from <c>ChatPackets.cs</c>.
    internal sealed class ChatMessage
    {
        public void Read(WorldPacket p)
        {
            Language = p.ReadUInt32();

            // V3_4_3 widened the length to 11 bits and added a trailing IsSecure
            // bit AFTER the length (per WPP V3_4_0_45166 ChatHandler.cs:233-238).
            // The pre-V3_4_3 modern clients used a 9-bit length with no IsSecure.
            // Reading the wrong layout leaves the packet bytes short and the text
            // shifted, so HandleChatMessage sees garbage and chat / GM commands
            // silently fail.
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                uint len = p.ReadBits<uint>(11);
                IsSecure = p.HasBit();
                Text = p.ReadString(len);
            }
            else
            {
                uint len = p.ReadBits<uint>(9);
                Text = p.ReadString(len);
            }
        }

        public string Text = string.Empty;
        public uint Language;
        public bool IsSecure;
    }

    /// Frozen verbatim from <c>ChatPackets.cs</c>.
    internal sealed class ChatMessageAFK
    {
        public void Read(WorldPacket p)
        {
            // V3_4_3 widened the length to 11 bits (per WPP V3_4_0_45166
            // ChatHandler.cs:86-93). Pre-V3_4_3 modern clients used 9 bits.
            uint len = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
                ? p.ReadBits<uint>(11)
                : p.ReadBits<uint>(9);
            Text = p.ReadString(len);
        }

        public string Text = string.Empty;
    }

    /// Frozen verbatim from <c>ChatPackets.cs</c>.
    internal sealed class ChatMessageDND
    {
        public void Read(WorldPacket p)
        {
            uint len = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
                ? p.ReadBits<uint>(11)
                : p.ReadBits<uint>(9);
            Text = p.ReadString(len);
        }

        public string Text = string.Empty;
    }

    /// Frozen verbatim from <c>ChatPackets.cs</c>.
    internal sealed class ChatMessageEmote
    {
        public void Read(WorldPacket p)
        {
            // V3_4_3 widened the length to 11 bits — same DND/EMOTE/AFK group
            // (WPP V3_4_0_45166 ChatHandler.cs:86-93). Reading 9 bits truncates
            // the text and leaves the bit cursor mid-byte, so the legacy server
            // sees a malformed CMSG_MESSAGECHAT and replies "unknown language".
            uint len = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
                ? p.ReadBits<uint>(11)
                : p.ReadBits<uint>(9);
            Text = p.ReadString(len);
        }

        public string Text = string.Empty;
    }

    /// Frozen verbatim from <c>ChatPackets.cs</c>.
    internal sealed class ChatMessageWhisper
    {
        public void Read(WorldPacket p)
        {
            Language = p.ReadUInt32();

            // V3_4_3 has no TargetGUID (V3_4_4+). Text length is 11 bits, same as SAY.
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                uint targetLen = p.ReadBits<uint>(9);
                uint textLen = p.ReadBits<uint>(11);
                Target = p.ReadString(targetLen);
                Text = p.ReadString(textLen);
            }
            else
            {
                uint targetLen = p.ReadBits<uint>(9);
                uint textLen = p.ReadBits<uint>(9);
                Target = p.ReadString(targetLen);
                Text = p.ReadString(textLen);
            }
        }

        public uint Language = 0;
        public string Text = string.Empty;
        public string Target = string.Empty;
    }

    /// Frozen verbatim from <c>ChatPackets.cs</c>.
    internal sealed class ChatMessageChannel
    {
        public void Read(WorldPacket p)
        {
            Language = p.ReadUInt32();
            ChannelGUID = p.ReadPackedGuid128();

            // V3_4_3 writes the text length as 11 bits, not 9, and follows the two lengths
            // with a conditional secure-flag bit pair (TC 3.4.3 ChatMessageChannel::Read).
            // Bit reads are MSB-first, so reading 9 bits of an 11-bit length yields len >> 2 —
            // "gooday" (6) came through as 1 and the client posted "g". Messages of 1-3
            // characters read as length 0 and post nothing at all. Same hazard as the one
            // already handled in ChatMessage and ChatMessageWhisper above; this packet was
            // missed when those were fixed. Issue #177.
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                uint targetLen343 = p.ReadBits<uint>(9);
                uint textLen343 = p.ReadBits<uint>(11);
                if (p.HasBit())
                    IsSecure = p.HasBit();
                Target = p.ReadString(targetLen343);
                Text = p.ReadString(textLen343);
                return;
            }

            uint targetLen = p.ReadBits<uint>(9);
            uint textLen = p.ReadBits<uint>(9);
            Target = p.ReadString(targetLen);
            Text = p.ReadString(textLen);
        }

        public uint Language;
        public WowGuid128 ChannelGUID;
        public string Text = string.Empty;
        public string Target = string.Empty;
        public bool IsSecure;
    }

    /// Frozen verbatim from <c>ChatPackets.cs</c>.
    internal sealed class JoinChannel
    {
        public void Read(WorldPacket p)
        {
            ChatChannelId = p.ReadInt32();
            uint channelLength = p.ReadBits<uint>(7);
            uint passwordLength = p.ReadBits<uint>(7);
            p.ResetBitPos();
            ChannelName = p.ReadString(channelLength);
            Password = p.ReadString(passwordLength);
        }

        public string Password = string.Empty;
        public string ChannelName = string.Empty;
        public int ChatChannelId;
    }

    /// Frozen verbatim from <c>ChatPackets.cs</c>.
    internal sealed class LeaveChannel
    {
        public void Read(WorldPacket p)
        {
            ZoneChannelID = p.ReadInt32();
            ChannelName = p.ReadString(p.ReadBits<uint>(7));
        }

        public int ZoneChannelID;
        public string ChannelName = string.Empty;
    }

    /// Frozen verbatim from <c>ChatPackets.cs</c>.
    internal sealed class ChannelCommand
    {
        public void Read(WorldPacket p)
        {
            ChannelName = p.ReadString(p.ReadBits<uint>(7));
        }

        public string ChannelName = string.Empty;
    }

    /// Frozen copy of <c>MovementAck.Read</c>, which the conversion deleted once its holders
    /// moved to codecs. The oracle has to keep reading the pre-conversion way.
    internal struct MovementAck
    {
        public void Read(WorldPacket p)
        {
            MoveInfo = new();
            MoveInfo.ReadMovementInfoModern(p);
            MoveCounter = p.ReadUInt32();
        }

        public MovementInfo MoveInfo;
        public uint MoveCounter;
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class ClientPlayerMovement
    {
        public void Read(WorldPacket p)
        {
            Guid = p.ReadPackedGuid128(); ;
            MoveInfo = new MovementInfo();
            MoveInfo.ReadMovementInfoModern(p);
        }

        public WowGuid128 Guid;
        public MovementInfo MoveInfo = null!;
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class MoveTeleportAck
    {
        public void Read(WorldPacket p)
        {
            MoverGUID = p.ReadPackedGuid128();
            MoveCounter = p.ReadUInt32();
            MoveTime = p.ReadUInt32();
        }

        public WowGuid128 MoverGUID;
        public uint MoveCounter;
        public uint MoveTime;
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class WorldPortResponse
    {
        public void Read(WorldPacket p) { }
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class MovementSpeedAck
    {
        public void Read(WorldPacket p)
        {
            MoverGUID = p.ReadPackedGuid128();
            Ack.Read(p);
            Speed = p.ReadFloat();
        }

        public WowGuid128 MoverGUID;
        public MovementAck Ack;
        public float Speed;
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class MovementAckMessage
    {
        public void Read(WorldPacket p)
        {
            MoverGUID = p.ReadPackedGuid128();
            Ack.Read(p);
        }

        public WowGuid128 MoverGUID;
        public MovementAck Ack;
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class MoveSetCollisionHeightAck
    {
        public void Read(WorldPacket p)
        {
            MoverGUID = p.ReadPackedGuid128();
            Ack.Read(p);
            Height = p.ReadFloat();
            MountDisplayID = p.ReadUInt32();
            Reason = p.ReadUInt8();
        }

        public WowGuid128 MoverGUID;
        public MovementAck Ack;
        public float Height;
        public uint MountDisplayID;
        public byte Reason;
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class SetActiveMover
    {
        public void Read(WorldPacket p)
        {
            MoverGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 MoverGUID;
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class InitActiveMoverComplete
    {
        public void Read(WorldPacket p)
        {
            Ticks = p.ReadUInt32();
        }

        public uint Ticks;
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class MoveSplineDone
    {
        public void Read(WorldPacket p)
        {
            Guid = p.ReadPackedGuid128();
            MoveInfo = new();
            MoveInfo.ReadMovementInfoModern(p);
            SplineID = p.ReadInt32();
        }

        public WowGuid128 Guid;
        public MovementInfo MoveInfo = null!;
        public int SplineID;
    }

    /// Frozen verbatim from <c>MovementPackets.cs</c>.
    internal sealed class MoveTimeSkipped
    {
        public void Read(WorldPacket p)
        {
            MoverGUID = p.ReadPackedGuid128();
            TimeSkipped = p.ReadUInt32();
        }

        public WowGuid128 MoverGUID;
        public uint TimeSkipped;
    }

    /// Frozen verbatim from <c>MiscPackets.cs</c>.
    internal sealed class RequestVehicleSeatChange
    {
        public void Read(WorldPacket p) { }
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class QueryGuildInfo
    {
        public void Read(WorldPacket p)
        {
            GuildGuid = p.ReadPackedGuid128();
            PlayerGuid = p.ReadPackedGuid128();
        }

        public WowGuid128 GuildGuid;
        public WowGuid128 PlayerGuid;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildUpdateMotdText
    {
        public void Read(WorldPacket p)
        {
            uint textLen = p.ReadBits<uint>(11);
            MotdText = p.ReadString(textLen);
        }

        public string MotdText = string.Empty;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildUpdateInfoText
    {
        public void Read(WorldPacket p)
        {
            uint textLen = p.ReadBits<uint>(11);
            InfoText = p.ReadString(textLen);
        }

        public string InfoText = string.Empty;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildSetMemberNote
    {
        public void Read(WorldPacket p)
        {
            NoteeGUID = p.ReadPackedGuid128();

            uint noteLen = p.ReadBits<uint>(8);
            IsPublic = p.HasBit();

            Note = p.ReadString(noteLen);
        }

        public WowGuid128 NoteeGUID;
        public bool IsPublic;          // 0 == Officer, 1 == Public
        public string Note = string.Empty;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildPromoteMember
    {
        public void Read(WorldPacket p)
        {
            Promotee = p.ReadPackedGuid128();
        }

        public WowGuid128 Promotee;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildDemoteMember
    {
        public void Read(WorldPacket p)
        {
            Demotee = p.ReadPackedGuid128();
        }

        public WowGuid128 Demotee;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildOfficerRemoveMember
    {
        public void Read(WorldPacket p)
        {
            Removee = p.ReadPackedGuid128();
        }

        public WowGuid128 Removee;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildInviteByName
    {
        public void Read(WorldPacket p)
        {
            uint nameLen = p.ReadBits<uint>(9);
            bool isArena = p.HasBit();

            Name = p.ReadString(nameLen);

            if (isArena)
                ArenaTeamId = p.ReadUInt32();
        }

        public string Name = string.Empty;
        public uint ArenaTeamId;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildSetRankPermissions
    {
        public void Read(WorldPacket p)
        {
            RankID = p.ReadUInt32();
            RankOrder = p.ReadUInt32();
            Flags = p.ReadUInt32();
            WithdrawGoldLimit = p.ReadInt32();

            for (byte i = 0; i < GuildConst.MaxBankTabs; i++)
            {
                TabFlags[i] = p.ReadUInt32();
                TabWithdrawItemLimit[i] = p.ReadUInt32();
            }

            OldFlags = p.ReadUInt32();

            p.ResetBitPos();
            uint rankNameLen = p.ReadBits<uint>(7);
            RankName = p.ReadString(rankNameLen);
        }

        public uint RankID;
        public uint RankOrder;
        public int WithdrawGoldLimit;
        public uint Flags;
        public uint OldFlags;
        public uint[] TabFlags = new uint[GuildConst.MaxBankTabs];
        public uint[] TabWithdrawItemLimit = new uint[GuildConst.MaxBankTabs];
        public string RankName = string.Empty;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildAddRank
    {
        public void Read(WorldPacket p)
        {
            uint nameLen = p.ReadBits<uint>(7);
            p.ResetBitPos();

            RankOrder = p.ReadInt32();
            Name = p.ReadString(nameLen);
        }

        public string Name = string.Empty;
        public int RankOrder;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildDeleteRank
    {
        public void Read(WorldPacket p)
        {
            RankOrder = p.ReadInt32();
        }

        public int RankOrder;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildSetGuildMaster
    {
        public void Read(WorldPacket p)
        {
            uint nameLen = p.ReadBits<uint>(9);
            NewMasterName = p.ReadString(nameLen);
        }

        public string NewMasterName = string.Empty;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class SaveGuildEmblem
    {
        public void Read(WorldPacket p)
        {
            DesignerGUID = p.ReadPackedGuid128();
            EmblemStyle = p.ReadUInt32();
            EmblemColor = p.ReadUInt32();
            BorderStyle = p.ReadUInt32();
            BorderColor = p.ReadUInt32();
            BackgroundColor = p.ReadUInt32();
        }

        public WowGuid128 DesignerGUID;
        public uint EmblemStyle;
        public uint EmblemColor;
        public uint BorderStyle;
        public uint BorderColor;
        public uint BackgroundColor;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class SetAutoDeclineGuildInvites
    {
        public void Read(WorldPacket p)
        {
            GuildInvitesShouldGetBlocked = p.ReadBool();
        }

        public bool GuildInvitesShouldGetBlocked;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildBankAtivate
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            FullUpdate = p.HasBit();
        }

        public WowGuid128 BankGuid;
        public bool FullUpdate;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildBankQueryTab
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            Tab = p.ReadUInt8();

            FullUpdate = p.HasBit();
        }

        public WowGuid128 BankGuid;
        public byte Tab;
        public bool FullUpdate;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildBankDepositMoney
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            Money = p.ReadUInt64();
        }

        public WowGuid128 BankGuid;
        public ulong Money;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildBankTextQuery
    {
        public void Read(WorldPacket p)
        {
            Tab = p.ReadInt32();
        }

        public int Tab;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildBankUpdateTab
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            BankTab = p.ReadUInt8();

            p.ResetBitPos();
            uint nameLen = p.ReadBits<uint>(7);
            uint iconLen = p.ReadBits<uint>(9);

            Name = p.ReadString(nameLen);
            Icon = p.ReadString(iconLen);
        }

        public WowGuid128 BankGuid;
        public byte BankTab;
        public string Name = string.Empty;
        public string Icon = string.Empty;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildBankLogQuery
    {
        public void Read(WorldPacket p)
        {
            Tab = p.ReadInt32();
        }

        public int Tab;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildBankSetTabText
    {
        public void Read(WorldPacket p)
        {
            Tab = p.ReadInt32();
            TabText = p.ReadString(p.ReadBits<uint>(14));
        }

        public int Tab;
        public string TabText = string.Empty;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildBankBuyTab
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            BankTab = p.ReadUInt8();
        }

        public WowGuid128 BankGuid;
        public byte BankTab;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class GuildBankWithdrawMoney
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            Money = p.ReadUInt64();
        }

        public WowGuid128 BankGuid;
        public ulong Money;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class AutoGuildBankItem
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            BankTab = p.ReadUInt8();
            BankSlot = p.ReadUInt8(); ;
            ContainerItemSlot = p.ReadUInt8();

            if (p.HasBit())
                ContainerSlot = p.ReadUInt8();
        }

        public WowGuid128 BankGuid;
        public byte BankTab;
        public byte BankSlot;
        public byte? ContainerSlot;
        public byte ContainerItemSlot;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class SplitItemToGuildBank
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            BankTab = p.ReadUInt8();
            BankSlot = p.ReadUInt8(); ;
            ContainerItemSlot = p.ReadUInt8();
            StackCount = p.ReadUInt32();

            if (p.HasBit())
                ContainerSlot = p.ReadUInt8();
        }

        public WowGuid128 BankGuid;
        public byte BankTab;
        public byte BankSlot;
        public byte? ContainerSlot;
        public byte ContainerItemSlot;
        public uint StackCount;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class AutoStoreGuildBankItem
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            BankTab = p.ReadUInt8();
            BankSlot = p.ReadUInt8();
        }

        public WowGuid128 BankGuid;
        public byte BankTab;
        public byte BankSlot;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class MoveGuildBankItem
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            BankTab1 = p.ReadUInt8();
            BankSlot1 = p.ReadUInt8();
            BankTab2 = p.ReadUInt8();
            BankSlot2 = p.ReadUInt8();
        }

        public WowGuid128 BankGuid;
        public byte BankTab1;
        public byte BankSlot1;
        public byte BankTab2;
        public byte BankSlot2;
    }

    /// Frozen verbatim from <c>GuildPackets.cs</c>.
    internal sealed class SplitGuildBankItem
    {
        public void Read(WorldPacket p)
        {
            BankGuid = p.ReadPackedGuid128();
            BankTab1 = p.ReadUInt8();
            BankSlot1 = p.ReadUInt8();
            BankTab2 = p.ReadUInt8();
            BankSlot2 = p.ReadUInt8();
            StackCount = p.ReadUInt32();
        }

        public WowGuid128 BankGuid;
        public byte BankTab1;
        public byte BankSlot1;
        public byte BankTab2;
        public byte BankSlot2;
        public uint StackCount;
    }

    /// Frozen verbatim from <c>NPCPackets.cs</c>.
    internal sealed class InteractWithNPC
    {
        public void Read(WorldPacket p)
        {
            CreatureGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 CreatureGUID;
    }

    /// Frozen verbatim from <c>NPCPackets.cs</c>.
    internal sealed class GossipSelectOption
    {
        public void Read(WorldPacket p)
        {
            GossipUnit = p.ReadPackedGuid128();
            GossipID = p.ReadUInt32();
            GossipIndex = p.ReadUInt32();

            uint length = p.ReadBits<uint>(8);
            PromotionCode = p.ReadString(length);
        }

        public WowGuid128 GossipUnit;
        public uint GossipIndex;
        public uint GossipID;
        public string PromotionCode = string.Empty;
    }

    /// Frozen verbatim from <c>NPCPackets.cs</c>.
    internal sealed class BuyBankSlot
    {
        public void Read(WorldPacket p)
        {
            Guid = p.ReadPackedGuid128();
        }

        public WowGuid128 Guid;
    }

    /// Frozen verbatim from <c>NPCPackets.cs</c>.
    internal sealed class TrainerBuySpell
    {
        public void Read(WorldPacket p)
        {
            TrainerGUID = p.ReadPackedGuid128();
            TrainerID = p.ReadUInt32();
            SpellID = p.ReadUInt32();
        }

        public WowGuid128 TrainerGUID;
        public uint TrainerID;
        public uint SpellID;
    }

    /// Frozen verbatim from <c>NPCPackets.cs</c>.
    internal sealed class ConfirmRespecWipe
    {
        public void Read(WorldPacket p)
        {
            TrainerGUID = p.ReadPackedGuid128();
            RespecType = (SpecResetType)p.ReadUInt8();
        }

        public WowGuid128 TrainerGUID;
        public SpecResetType RespecType;
    }

    /// Frozen verbatim from <c>TaxiPackets.cs</c>.
    internal sealed class ActivateTaxi
    {
        public void Read(WorldPacket p)
        {
            FlightMaster = p.ReadPackedGuid128();
            Node = p.ReadUInt32();
            GroundMountID = p.ReadUInt32();
            FlyingMountID = p.ReadUInt32();
        }

        public WowGuid128 FlightMaster;
        public uint Node;
        public uint GroundMountID;
        public uint FlyingMountID;
    }

    /// Frozen verbatim from <c>AuctionPackets.cs</c>.
    internal sealed class AuctionListOwnerItems
    {
        public void Read(WorldPacket p)
        {
            Auctioneer = p.ReadPackedGuid128();
            Offset = p.ReadUInt32();
        }

        public WowGuid128 Auctioneer;
        public uint Offset;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestGiverQueryQuest
    {
        public void Read(WorldPacket p)
        {
            QuestGiverGUID = p.ReadPackedGuid128();
            QuestID = p.ReadUInt32();
            RespondToGiver = p.HasBit();
        }

        public WowGuid128 QuestGiverGUID;
        public uint QuestID;
        public bool RespondToGiver;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestGiverAcceptQuest
    {
        public void Read(WorldPacket p)
        {
            QuestGiverGUID = p.ReadPackedGuid128();
            QuestID = p.ReadUInt32();
            StartCheat = p.HasBit();
        }

        public WowGuid128 QuestGiverGUID;
        public uint QuestID;
        public bool StartCheat;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestLogRemoveQuest
    {
        public void Read(WorldPacket p)
        {
            Slot = p.ReadUInt8();
        }

        public byte Slot;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestGiverStatusQuery
    {
        public void Read(WorldPacket p)
        {
            QuestGiverGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 QuestGiverGUID;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestGiverHello
    {
        public void Read(WorldPacket p)
        {
            QuestGiverGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 QuestGiverGUID;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestGiverCloseQuest
    {
        public void Read(WorldPacket p)
        {
            QuestID = p.ReadInt32();
        }

        public int QuestID;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class CloseInteraction
    {
        public void Read(WorldPacket p)
        {
            Guid = p.ReadPackedGuid128();
        }

        public WowGuid128 Guid;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestPOIQuery
    {
        public void Read(WorldPacket p)
        {
            // Wire: int32 count, int32[count] questIds. CypherCore over-allocates a
            // 175-slot array but only reads `count` ints from the stream — only the
            // populated prefix is on the wire.
            int count = p.ReadInt32();
            MissingQuestPOIs = new int[count];
            for (int i = 0; i < count; i++)
                MissingQuestPOIs[i] = p.ReadInt32();
        }

        public int[] MissingQuestPOIs = Array.Empty<int>();
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestGiverRequestReward
    {
        public void Read(WorldPacket p)
        {
            QuestGiverGUID = p.ReadPackedGuid128();
            QuestID = p.ReadUInt32();
        }

        public WowGuid128 QuestGiverGUID;
        public uint QuestID;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestGiverChooseReward
    {
        public void Read(WorldPacket p)
        {
            QuestGiverGUID = p.ReadPackedGuid128();
            QuestID = p.ReadUInt32();
            Choice.Read(p);
        }

        public WowGuid128 QuestGiverGUID;
        public uint QuestID;
        public QuestChoiceItem Choice = new();
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestGiverCompleteQuest
    {
        public void Read(WorldPacket p)
        {
            QuestGiverGUID = p.ReadPackedGuid128();
            QuestID = p.ReadUInt32();
            FromScript = p.HasBit();
        }

        public WowGuid128 QuestGiverGUID; // NPC / GameObject guid for normal quest completion. Player guid for self-completed quests
        public uint QuestID;
        public bool FromScript; // 0 - standart complete quest mode with npc, 1 - auto-complete mode
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestConfirmAcceptResponse
    {
        public void Read(WorldPacket p)
        {
            QuestID = p.ReadUInt32();
        }

        public uint QuestID;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class PushQuestToParty
    {
        public void Read(WorldPacket p)
        {
            QuestID = p.ReadUInt32();
        }

        public uint QuestID;
    }

    /// Frozen verbatim from <c>QuestPackets.cs</c>.
    internal sealed class QuestPushResultResponse
    {
        public void Read(WorldPacket p)
        {
            SenderGUID = p.ReadPackedGuid128();
            QuestID = p.ReadUInt32();
            Result = (QuestPushReason)p.ReadUInt8();
        }

        public WowGuid128 SenderGUID;
        public uint QuestID;
        public QuestPushReason Result;
    }

    /// <summary>
    /// Frozen verbatim from <c>ItemPackets.cs</c>, where <c>InvUpdate</c> was a struct holding a
    /// <c>List&lt;InvItem&gt;</c> before the item slice made it an inline array.
    /// </summary>
    /// <remarks>
    /// The oracles below construct this one, not the production type. An oracle that referenced
    /// the converted type would be validating the new reader against itself.
    /// </remarks>
    internal struct InvUpdate
    {
        public InvUpdate(WorldPacket data)
        {
            Items = new List<InvItem>();
            int size = data.ReadBits<int>(2);
            data.ResetBitPos();
            for (int i = 0; i < size; ++i)
            {
                var item = new InvItem
                {
                    ContainerSlot = data.ReadUInt8(),
                    Slot = data.ReadUInt8()
                };
                Items.Add(item);
            }
        }

        public List<InvItem> Items;

        public struct InvItem
        {
            public byte ContainerSlot;
            public byte Slot;
        }
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class BuyItem
    {
        public BuyItem()
        {
            Item = new ItemInstance();
        }

        public void Read(WorldPacket p)
        {
            VendorGUID = p.ReadPackedGuid128();
            ContainerGUID = p.ReadPackedGuid128();
            Quantity = p.ReadUInt32();

            // V3_4_3 (WotLK Classic) reordered the trailing fields and inserted MuID
            // (the 1-based vendor slot index returned in SMSG_VENDOR_INVENTORY).
            // Reading the older (pre-WotLK) layout against this packet shifts every
            // following field, so the proxy forwarded a garbage Slot to the legacy
            // server and the buy was silently rejected. Layout mirrors fork
            // HermesProxy-WOTLK Server/Packets/BuyItem.cs:Read for ExpansionVersion>=3.
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                MuID = p.ReadUInt32();
                Slot = p.ReadUInt32();
                ItemType = (ItemVendorType)p.ReadInt32();
                Item.Read(p);
            }
            else
            {
                Slot = p.ReadUInt32();
                BagSlot = p.ReadUInt32();
                Item.Read(p);
                ItemType = (ItemVendorType)p.ReadBits<int>(3);
            }
        }

        public WowGuid128 VendorGUID;
        public ItemInstance Item;
        public uint MuID;
        public uint Slot;
        public uint BagSlot;
        public ItemVendorType ItemType;
        public uint Quantity;
        public WowGuid128 ContainerGUID;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class SellItem
    {
        public void Read(WorldPacket p)
        {
            VendorGUID = p.ReadPackedGuid128();
            ItemGUID = p.ReadPackedGuid128();
            Amount = p.ReadUInt32();
        }

        public WowGuid128 VendorGUID;
        public WowGuid128 ItemGUID;
        public uint Amount;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class SplitItem
    {
        public void Read(WorldPacket p)
        {
            Inv = new InvUpdate(p);
            FromPackSlot = p.ReadUInt8();
            FromSlot = p.ReadUInt8();
            ToPackSlot = p.ReadUInt8();
            ToSlot = p.ReadUInt8();
            Quantity = p.ReadInt32();
        }

        public byte ToSlot;
        public byte ToPackSlot;
        public byte FromPackSlot;
        public int Quantity;
        public InvUpdate Inv;
        public byte FromSlot;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class SwapInvItem
    {
        public void Read(WorldPacket p)
        {
            Inv = new InvUpdate(p);
            Slot2 = p.ReadUInt8();
            Slot1 = p.ReadUInt8();
        }

        public InvUpdate Inv;
        public byte Slot1; // Source Slot
        public byte Slot2; // Destination Slot
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class SwapItem
    {
        public void Read(WorldPacket p)
        {
            Inv = new InvUpdate(p);
            ContainerSlotB = p.ReadUInt8();
            ContainerSlotA = p.ReadUInt8();
            SlotB = p.ReadUInt8();
            SlotA = p.ReadUInt8();
        }

        public InvUpdate Inv;
        public byte SlotA;
        public byte ContainerSlotB;
        public byte SlotB;
        public byte ContainerSlotA;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class DestroyItem
    {
        public void Read(WorldPacket p)
        {
            Count = p.ReadUInt32();
            ContainerId = p.ReadUInt8();
            SlotNum = p.ReadUInt8();
        }

        public uint Count;
        public byte SlotNum;
        public byte ContainerId;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class AutoStoreBagItem
    {
        public void Read(WorldPacket p)
        {
            Inv = new InvUpdate(p);
            ContainerSlotA = p.ReadUInt8();
            ContainerSlotB = p.ReadUInt8();
            SlotA = p.ReadUInt8();
        }

        public InvUpdate Inv;
        public byte ContainerSlotA;
        public byte ContainerSlotB;
        public byte SlotA;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class AutoEquipItem
    {
        public void Read(WorldPacket p)
        {
            Inv = new InvUpdate(p);
            PackSlot = p.ReadUInt8();
            Slot = p.ReadUInt8();
        }

        public byte Slot;
        public InvUpdate Inv;
        public byte PackSlot;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class AutoEquipItemSlot
    {
        public void Read(WorldPacket p)
        {
            Inv = new InvUpdate(p);
            Item = p.ReadPackedGuid128();
            ItemDstSlot = p.ReadUInt8();
        }

        public WowGuid128 Item;
        public byte ItemDstSlot;
        public InvUpdate Inv;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class ReadItem
    {
        public void Read(WorldPacket p)
        {
            PackSlot = p.ReadUInt8();
            Slot = p.ReadUInt8();
        }

        public byte PackSlot;
        public byte Slot;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class RepairItem
    {
        public void Read(WorldPacket p)
        {
            VendorGUID = p.ReadPackedGuid128();
            ItemGUID = p.ReadPackedGuid128();
            UseGuildBank = p.HasBit();
        }

        public WowGuid128 VendorGUID;
        public WowGuid128 ItemGUID;
        public bool UseGuildBank;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class SocketGems
    {
        public void Read(WorldPacket p)
        {
            ItemGuid = p.ReadPackedGuid128();
            for (int i = 0; i < ItemConst.MaxGemSockets; ++i)
                Gems[i] = p.ReadPackedGuid128();
        }

        public WowGuid128 ItemGuid;
        public WowGuid128[] Gems = new WowGuid128[ItemConst.MaxGemSockets];
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class OpenItem
    {
        public void Read(WorldPacket p)
        {
            PackSlot = p.ReadUInt8();
            Slot = p.ReadUInt8();
        }

        public byte PackSlot;
        public byte Slot;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class SetAmmo
    {
        public void Read(WorldPacket p)
        {
            ItemId = p.ReadUInt32();
        }

        public uint ItemId;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class CancelTempEnchantment
    {
        public void Read(WorldPacket p)
        {
            EnchantmentSlot = p.ReadUInt32();
        }

        public uint EnchantmentSlot;
    }

    /// Frozen verbatim from <c>ItemPackets.cs</c>.
    internal sealed class WrapItem
    {
        public void Read(WorldPacket p)
        {
            _ = p.ReadUInt8(); // Unknown Value. Usually 128
            GiftBag = p.ReadUInt8();
            GiftSlot = p.ReadUInt8();
            ItemBag = p.ReadUInt8();
            ItemSlot = p.ReadUInt8();
        }

        public byte GiftBag;
        public byte GiftSlot;
        public byte ItemBag;
        public byte ItemSlot;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class PartyInviteClient
    {
        public void Read(WorldPacket p)
        {
            PartyIndex = p.ReadUInt8();

            uint targetNameLen = p.ReadBits<uint>(9);
            uint targetRealmLen = p.ReadBits<uint>(9);

            VirtualRealmAddress = p.ReadUInt32();
            TargetGUID = p.ReadPackedGuid128();

            TargetName = p.ReadString(targetNameLen);
            TargetRealm = p.ReadString(targetRealmLen);
        }

        public byte PartyIndex;
        public uint VirtualRealmAddress;
        public WowGuid128 TargetGUID;
        public string TargetName = string.Empty;
        public string TargetRealm = string.Empty;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class PartyInviteResponse
    {
        public void Read(WorldPacket p)
        {
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                // V3_4_3 wire layout: 3 header bits first, then optional bytes.
                // /reload emits this packet with all flags=0 (size=1) as a state flush.
                bool hasPartyIndex = p.HasBit();
                Accept = p.HasBit();
                bool hasRolesDesiredV343 = p.HasBit();

                if (hasPartyIndex)
                    PartyIndex = p.ReadUInt8();
                if (hasRolesDesiredV343)
                    RolesDesired = p.ReadUInt8();
                return;
            }

            PartyIndex = p.ReadUInt8();

            Accept = p.HasBit();

            bool hasRolesDesired = p.HasBit();
            if (hasRolesDesired)
                RolesDesired = p.ReadUInt32();
        }

        public byte PartyIndex;
        public bool Accept;
        public uint? RolesDesired;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class LeaveGroup
    {
        public void Read(WorldPacket p)
        {
            PartyIndex = p.ReadInt8();
        }

        public sbyte PartyIndex;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class PartyUninvite
    {
        public void Read(WorldPacket p)
        {
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                // V3_4_3 wire layout: bits first, then GUID, then optional PartyIndex byte.
                bool hasPartyIndex = p.HasBit();
                byte reasonLen = p.ReadBits<byte>(8);
                TargetGUID = p.ReadPackedGuid128();
                if (hasPartyIndex)
                    PartyIndex = p.ReadUInt8();
                Reason = p.ReadString(reasonLen);
                return;
            }

            PartyIndex = p.ReadUInt8();
            TargetGUID = p.ReadPackedGuid128();

            byte legacyReasonLen = p.ReadBits<byte>(8);
            Reason = p.ReadString(legacyReasonLen);
        }

        public byte PartyIndex;
        public WowGuid128 TargetGUID;
        public string Reason = string.Empty;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class SetAssistantLeader
    {
        public void Read(WorldPacket p)
        {
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                // V3_4_3 wire layout: 2 header bits, then GUID, then optional PartyIndex byte.
                // Mirrors CypherCore WorldPackets::Party::SetAssistantLeader::Read.
                bool hasPartyIndex = p.HasBit();
                Apply = p.HasBit();
                TargetGUID = p.ReadPackedGuid128();
                if (hasPartyIndex)
                    PartyIndex = p.ReadUInt8();
                return;
            }

            PartyIndex = p.ReadUInt8();
            TargetGUID = p.ReadPackedGuid128();
            Apply = p.HasBit();
        }

        public byte PartyIndex;
        public WowGuid128 TargetGUID;
        public bool Apply;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class SetEveryoneIsAssistant
    {
        public void Read(WorldPacket p)
        {
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                // V3_4_3 wire layout: 2 header bits, then optional PartyIndex byte.
                // Mirrors CypherCore WorldPackets::Party::SetEveryoneIsAssistant::Read.
                bool hasPartyIndex = p.HasBit();
                Apply = p.HasBit();
                if (hasPartyIndex)
                    PartyIndex = p.ReadUInt8();
                return;
            }

            PartyIndex = p.ReadUInt8();
            Apply = p.HasBit();
        }

        public byte PartyIndex;
        public bool Apply;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class SetPartyLeader
    {
        public void Read(WorldPacket p)
        {
            PartyIndex = p.ReadInt8();
            TargetGUID = p.ReadPackedGuid128();
        }

        public sbyte PartyIndex;
        public WowGuid128 TargetGUID;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class ConvertRaid
    {
        public void Read(WorldPacket p)
        {
            Raid = p.HasBit();
        }

        public bool Raid;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class DoReadyCheck
    {
        public void Read(WorldPacket p)
        {
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                // V3_4_3 wire layout: a HasPartyIndex bit first, then the optional byte.
                bool hasPartyIndex = p.HasBit();
                if (hasPartyIndex)
                    PartyIndex = p.ReadInt8();
                return;
            }

            PartyIndex = p.ReadInt8();
        }

        public sbyte PartyIndex;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class ReadyCheckResponseClient
    {
        public void Read(WorldPacket p)
        {
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                // V3_4_3 wire layout: a HasPartyIndex bit, then IsReady, then the optional
                // PartyIndex byte - the same bits-first shape as PartyInviteResponse above.
                // Reading PartyIndex first consumed the bit byte and then took the MSB of the
                // (always zero) index byte as IsReady, so every answer reached the group as
                // "not ready". Observed bytes: Ready = C0 00, Not Ready = 80 00. WPP's
                // V3_4_0 parser orders these bits the other way round, but it is registered
                // at V3_4_4_59817 and does not hold for 54261.
                bool hasPartyIndex = p.HasBit();
                IsReady = p.HasBit();
                if (hasPartyIndex)
                    PartyIndex = p.ReadUInt8();
                return;
            }

            PartyIndex = p.ReadUInt8();
            IsReady = p.HasBit();
        }

        public byte PartyIndex;
        public bool IsReady;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class UpdateRaidTarget
    {
        public void Read(WorldPacket p)
        {
            PartyIndex = p.ReadInt8();
            Target = p.ReadPackedGuid128();
            Symbol = p.ReadInt8();
        }

        public sbyte PartyIndex;
        public WowGuid128 Target;
        public sbyte Symbol;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class SummonResponse
    {
        public void Read(WorldPacket p)
        {
            SummonerGUID = p.ReadPackedGuid128();
            Accept = p.HasBit();
        }

        public WowGuid128 SummonerGUID;
        public bool Accept;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class MinimapPingClient
    {
        public void Read(WorldPacket p)
        {
            Position = p.ReadVector2();
            PartyIndex = p.ReadInt8();
        }

        public Vector2 Position;
        public sbyte PartyIndex;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class RandomRollClient
    {
        public void Read(WorldPacket p)
        {
            Min = p.ReadInt32();
            Max = p.ReadInt32();
            PartyIndex = p.ReadUInt8();
        }

        public int Min;
        public int Max;
        public byte PartyIndex;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class RequestPartyMemberStats
    {
        public void Read(WorldPacket p)
        {
            PartyIndex = p.ReadUInt8();
            TargetGUID = p.ReadPackedGuid128();
        }

        public byte PartyIndex;
        public WowGuid128 TargetGUID;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class ChangeSubGroup
    {
        public void Read(WorldPacket p)
        {
            TargetGUID = p.ReadPackedGuid128();
            PartyIndex = p.ReadInt8();
            NewSubGroup = p.ReadUInt8();
        }

        public WowGuid128 TargetGUID;
        public sbyte PartyIndex;
        public byte NewSubGroup;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class SwapSubGroups
    {
        public void Read(WorldPacket p)
        {
            PartyIndex = p.ReadInt8();
            FirstTarget = p.ReadPackedGuid128();
            SecondTarget = p.ReadPackedGuid128();
        }

        public WowGuid128 FirstTarget;
        public WowGuid128 SecondTarget;
        public sbyte PartyIndex;
    }

    /// Frozen verbatim from <c>GroupPackets.cs</c>.
    internal sealed class SetRole
    {
        public void Read(WorldPacket p)
        {
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                bool hasPartyIndex = p.HasBit();
                ChangedUnit = p.ReadPackedGuid128();
                Role = p.ReadUInt8();
                if (hasPartyIndex)
                    PartyIndex = p.ReadUInt8();
                return;
            }

            PartyIndex = (byte)p.ReadInt8();
            ChangedUnit = p.ReadPackedGuid128();
            Role = (byte)p.ReadInt32();
        }

        public byte PartyIndex;
        public WowGuid128 ChangedUnit;
        public byte Role;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class ReorderCharacters
    {
        public void Read(WorldPacket p)
        {
            uint count = p.ReadBits<uint>(9);
            Entries = new ReorderInfo[count];
            for (uint i = 0; i < count; i++)
            {
                Entries[i].PlayerGuid = p.ReadPackedGuid128();
                Entries[i].NewPosition = p.ReadUInt8();
            }
        }

        public ReorderInfo[] Entries = System.Array.Empty<ReorderInfo>();

        public struct ReorderInfo
        {
            public WowGuid128 PlayerGuid;
            public byte NewPosition;
        }
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class GetAccountCharacterListRequest
    {
        public void Read(WorldPacket p)
        {
            Token = p.ReadUInt32();
        }

        public uint Token = 0;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class GenerateRandomCharacterNameRequest
    {
        public void Read(WorldPacket p)
        {
            Race = (Race)p.ReadUInt8();
            Sex = (Gender)p.ReadUInt8();
        }

        public Race Race;
        public Gender Sex;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class CreateCharacter
    {
        public void Read(WorldPacket p)
        {
            CreateInfo = new CharacterCreateInfo();
            uint nameLength = p.ReadBits<uint>(6);
            bool hasTemplateSet = p.HasBit();
            CreateInfo.IsTrialBoost = p.HasBit();
            CreateInfo.UseNPE = p.HasBit();

            CreateInfo.RaceId = (Race)p.ReadUInt8();
            CreateInfo.ClassId = (Class)p.ReadUInt8();
            CreateInfo.Sex = (Gender)p.ReadUInt8();
            var customizationCount = p.ReadUInt32();

            CreateInfo.Name = p.ReadString(nameLength);
            if (hasTemplateSet)
                CreateInfo.TemplateSet = p.ReadUInt32();

            for (var i = 0; i < customizationCount; ++i)
            {
                CreateInfo.Customizations.Add(new ChrCustomizationChoice(p.ReadUInt32(), p.ReadUInt32()));
            }

            CreateInfo.Customizations.Sort();
        }

        public CharacterCreateInfo CreateInfo = null!;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class CharDelete
    {
        public void Read(WorldPacket p)
        {
            Guid = p.ReadPackedGuid128();
        }

        public WowGuid128 Guid; // Guid of the character to delete
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class LoadingScreenNotify
    {
        public void Read(WorldPacket p)
        {
            MapID = p.ReadUInt32();
            Showing = p.HasBit();
        }

        public uint MapID;
        public bool Showing;
    }

    /// Frozen verbatim from <c>QueryPackets.cs</c>.
    internal sealed class QueryPlayerName
    {
        public void Read(WorldPacket p)
        {
            Player = p.ReadPackedGuid128();
        }

        public WowGuid128 Player;
    }

    /// Frozen verbatim from <c>QueryPackets.cs</c>.
    internal sealed class QueryPlayerNames
    {
        public void Read(WorldPacket p)
        {
            uint count = p.ReadUInt32();
            for (uint i = 0; i < count; i++)
                Players.Add(p.ReadPackedGuid128());
        }

        public List<WowGuid128> Players = new List<WowGuid128>();
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class PlayerLogin
    {
        public void Read(WorldPacket p)
        {
            Guid = p.ReadPackedGuid128();
            FarClip = p.ReadFloat();
            // 3.4.3 client doesn't send the trailing bit — packet is exactly Guid+FarClip.
            // Per WPP V3_4_0_45166 SessionHandler.cs:143, gated on V3_4_3_51505+.
            if (ModernVersion.ExpansionVersion < 3)
                UnkBit = p.HasBit();
        }

        public WowGuid128 Guid;      // Guid of the player that is logging in
        public float FarClip;        // Visibility distance (for terrain)
        public bool UnkBit;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class LogoutRequest
    {
        public void Read(WorldPacket p)
        {
            IdleLogout = p.HasBit();
        }

        public bool IdleLogout;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class RequestPlayedTime
    {
        public void Read(WorldPacket p)
        {
            TriggerScriptEvent = p.HasBit();
        }

        public bool TriggerScriptEvent;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class SetTitle
    {
        public int TitleID;

        public void Read(WorldPacket p)
        {
            TitleID = p.ReadInt32();
        }
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class AlterAppearance
    {
        public void Read(WorldPacket p)
        {
            var customizationCount = p.ReadUInt32();
            NewSexId = (Gender)p.ReadUInt8();
            CustomizedRace = (Race)p.ReadUInt32();
            CustomizedChrModelId = p.ReadUInt32();

            for (var i = 0; i < customizationCount; ++i)
                Customizations.Add(new ChrCustomizationChoice(p.ReadUInt32(), p.ReadUInt32()));

            Customizations.Sort();
        }

        public Gender NewSexId;
        public Race CustomizedRace;
        public uint CustomizedChrModelId;
        public List<ChrCustomizationChoice> Customizations = new(8);
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class SetPvP
    {
        public void Read(WorldPacket p)
        {
            Enable = p.HasBit();
        }

        public bool Enable;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class SetActionButton
    {
        public void Read(WorldPacket p)
        {
            Action = p.ReadUInt16();
            Type = p.ReadUInt16();
            Index = p.ReadUInt8();
        }

        public ushort Action;
        public ushort Type;
        public byte Index;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class SetActionBarToggles
    {
        public void Read(WorldPacket p)
        {
            Mask = p.ReadUInt8();
        }

        public byte Mask;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class UnlearnSkill
    {
        public void Read(WorldPacket p)
        {
            SkillLine = p.ReadUInt32();
        }

        public uint SkillLine;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class PlayerShowingHelmOrCloak
    {
        public void Read(WorldPacket p)
        {
            p.ResetBitPos();
            Showing = p.HasBit();
        }

        public bool Showing;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class Inspect
    {
        public void Read(WorldPacket p)
        {
            Target = p.ReadPackedGuid128();
        }

        public WowGuid128 Target;
    }

    /// Frozen verbatim from <c>CharacterPackets.cs</c>.
    internal sealed class CharacterRenameRequest
    {
        public void Read(WorldPacket p)
        {
            Guid = p.ReadPackedGuid128();
            NewName = p.ReadString(p.ReadBits<uint>(6));
        }

        public string NewName = string.Empty;
        public WowGuid128 Guid;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class PetAction
    {
        public void Read(WorldPacket p)
        {
            PetGUID = p.ReadPackedGuid128();

            Action = p.ReadUInt32();
            TargetGUID = p.ReadPackedGuid128();

            ActionPosition = p.ReadVector3();
        }

        public WowGuid128 PetGUID;
        public uint Action;
        public WowGuid128 TargetGUID;
        public Vector3 ActionPosition;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class PetStopAttack
    {
        public void Read(WorldPacket p)
        {
            PetGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 PetGUID;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class PetSetAction
    {
        public void Read(WorldPacket p)
        {
            PetGUID = p.ReadPackedGuid128();

            Index = p.ReadUInt32();
            Action = p.ReadUInt32();
        }

        public WowGuid128 PetGUID;
        public uint Index;
        public uint Action;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class PetRename
    {
        public void Read(WorldPacket p)
        {
            RenameData.PetGUID = p.ReadPackedGuid128();
            RenameData.PetNumber = p.ReadInt32();

            uint nameLen = p.ReadBits<uint>(8);

            RenameData.HasDeclinedNames = p.HasBit();
            if (RenameData.HasDeclinedNames)
            {
                RenameData.DeclinedNames = new DeclinedName();
                uint[] count = new uint[PlayerConst.MaxDeclinedNameCases];
                for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                    count[i] = p.ReadBits<uint>(7);

                for (int i = 0; i < PlayerConst.MaxDeclinedNameCases; i++)
                    RenameData.DeclinedNames.name[i] = p.ReadString(count[i]);
            }

            RenameData.NewName = p.ReadString(nameLen);
        }

        public PetRenameData RenameData;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class RequestStabledPets
    {
        public void Read(WorldPacket p)
        {
            StableMaster = p.ReadPackedGuid128();
        }

        public WowGuid128 StableMaster;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class BuyStableSlot
    {
        public void Read(WorldPacket p)
        {
            StableMaster = p.ReadPackedGuid128();
        }

        public WowGuid128 StableMaster;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class PetAbandon
    {
        public void Read(WorldPacket p)
        {
            PetGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 PetGUID;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class StablePet
    {
        public void Read(WorldPacket p)
        {
            StableMaster = p.ReadPackedGuid128();
        }

        public WowGuid128 StableMaster;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class UnstablePet
    {
        public void Read(WorldPacket p)
        {
            PetNumber = p.ReadUInt32();
            StableMaster = p.ReadPackedGuid128();
        }

        public uint PetNumber;
        public WowGuid128 StableMaster;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class StableSwapPet
    {
        public void Read(WorldPacket p)
        {
            PetNumber = p.ReadUInt32();
            StableMaster = p.ReadPackedGuid128();
        }

        public uint PetNumber;
        public WowGuid128 StableMaster;
    }

    /// Frozen verbatim from <c>PetPackets.cs</c>.
    internal sealed class PetCancelAura
    {
        public void Read(WorldPacket p)
        {
            PetGUID = p.ReadPackedGuid128();
            SpellID = p.ReadUInt32();
        }

        public WowGuid128 PetGUID;
        public uint SpellID;
    }

    /// Frozen verbatim from <c>SpellPackets.cs</c>.
    internal sealed class CancelCast
    {
        public void Read(WorldPacket p)
        {
            CastID = p.ReadPackedGuid128();
            SpellID = p.ReadUInt32();
        }

        public uint SpellID;
        public WowGuid128 CastID;
    }

    /// Frozen verbatim from <c>SpellPackets.cs</c>.
    internal sealed class CancelChannelling
    {
        public void Read(WorldPacket p)
        {
            SpellID = p.ReadInt32();
            Reason = p.ReadInt32();
        }

        public int SpellID;
        public int Reason;       // 40 = /run SpellStopCasting(), 16 = movement/AURA_INTERRUPT_FLAG_MOVE, 41 = turning/AURA_INTERRUPT_FLAG_TURNING
                                 // does not match SpellCastResult enum
    }

    /// Frozen verbatim from <c>SpellPackets.cs</c>.
    internal sealed class CancelAura
    {
        public void Read(WorldPacket p)
        {
            SpellID = p.ReadUInt32();
            CasterGUID = p.ReadPackedGuid128();
        }

        public uint SpellID;
        public WowGuid128 CasterGUID;
    }

    /// Frozen verbatim from <c>SpellPackets.cs</c>.
    internal sealed class LearnTalent
    {
        public void Read(WorldPacket p)
        {
            TalentID = p.ReadUInt32();
            Rank = p.ReadUInt16();
        }

        public uint TalentID;
        public ushort Rank;
    }

    /// Frozen verbatim from <c>SpellPackets.cs</c>.
    internal sealed class ResurrectResponse
    {
        public void Read(WorldPacket p)
        {
            CasterGUID = p.ReadPackedGuid128();
            Response = p.ReadUInt32();
        }

        public WowGuid128 CasterGUID;
        public uint Response;
    }

    /// Frozen verbatim from <c>SpellPackets.cs</c>.
    internal sealed class SelfRes
    {
        public void Read(WorldPacket p)
        {
            SpellId = p.ReadUInt32();
        }

        public uint SpellId;
    }

    /// Frozen verbatim from <c>SpellPackets.cs</c>.
    internal sealed class TotemDestroyed
    {
        public void Read(WorldPacket p)
        {
            Slot = p.ReadUInt8();
            Guid = p.ReadPackedGuid128();
        }
        public byte Slot;
        public WowGuid128 Guid;
    }

    /// Frozen verbatim from <c>LootPackets.cs</c>, where <c>LootRequest</c> was a mutable struct
    /// before the loot slice made it a readonly record struct. The oracles below build it with an
    /// object initializer, which a readonly type cannot express — and pointing them at the
    /// converted type would have them validate the new reader against itself.
    internal struct LootRequest
    {
        public WowGuid128 LootObj;
        public byte LootListID;
    }

/// Frozen verbatim from <c>LootPackets.cs</c>.
    internal sealed class LootRelease
    {
        public void Read(WorldPacket p)
        {
            Owner = p.ReadPackedGuid128();
        }

        public WowGuid128 Owner;
    }

    /// Frozen verbatim from <c>LootPackets.cs</c>.
    internal sealed class LootItemPkt
    {
        public void Read(WorldPacket p)
        {
            uint Count = p.ReadUInt32();

            for (uint i = 0; i < Count; ++i)
            {
                var loot = new LootRequest()
                {
                    LootObj = p.ReadPackedGuid128(),
                    LootListID = p.ReadUInt8()
                };

                Loot.Add(loot);
            }
        }

        public List<LootRequest> Loot = new();
    }

    /// Frozen verbatim from <c>LootPackets.cs</c>.
    internal sealed class LootUnit
    {
        public void Read(WorldPacket p)
        {
            Unit = p.ReadPackedGuid128();
        }

        public WowGuid128 Unit;
    }

    /// Frozen verbatim from <c>LootPackets.cs</c>.
    internal sealed class LootMoney
    {
        public void Read(WorldPacket p) { }
    }

    /// Frozen verbatim from <c>LootPackets.cs</c>.
    internal sealed class SetLootMethod
    {
        public void Read(WorldPacket p)
        {
            PartyIndex = p.ReadInt8();
            LootMethod = (LootMethod)p.ReadUInt8();
            LootMasterGUID = p.ReadPackedGuid128();
            LootThreshold = p.ReadUInt32();
        }

        public sbyte PartyIndex;
        public LootMethod LootMethod;
        public WowGuid128 LootMasterGUID;
        public uint LootThreshold;
    }

    /// Frozen verbatim from <c>LootPackets.cs</c>.
    internal sealed class OptOutOfLoot
    {
        public void Read(WorldPacket p)
        {
            PassOnLoot = p.HasBit();
        }

        public bool PassOnLoot;
    }

    /// Frozen verbatim from <c>LootPackets.cs</c>.
    internal sealed class LootRoll
    {
        public void Read(WorldPacket p)
        {
            LootObj = p.ReadPackedGuid128();
            LootListID = p.ReadUInt8();
            RollType = (RollType)p.ReadUInt8();
        }

        public WowGuid128 LootObj;
        public byte LootListID;
        public RollType RollType;
    }

    /// Frozen verbatim from <c>LootPackets.cs</c>.
    internal sealed class LootMasterGive
    {
        public void Read(WorldPacket p)
        {
            uint Count = p.ReadUInt32();
            TargetGUID = p.ReadPackedGuid128();

            for (int i = 0; i < Count; ++i)
            {
                LootRequest lootRequest = new();
                lootRequest.LootObj = p.ReadPackedGuid128();
                lootRequest.LootListID = p.ReadUInt8();
                Loot.Add(lootRequest);
            }
        }

        public WowGuid128 TargetGUID;
        public List<LootRequest> Loot = new();
    }

    /// Frozen verbatim from <c>TradePackets.cs</c>.
    internal sealed class InitiateTrade
    {
        public void Read(WorldPacket p)
        {
            Guid = p.ReadPackedGuid128();
        }

        public WowGuid128 Guid;
    }

    /// Frozen verbatim from <c>TradePackets.cs</c>.
    internal sealed class SetTradeGold
    {
        public void Read(WorldPacket p)
        {
            Coinage = p.ReadUInt64();
        }

        public ulong Coinage;
    }

    /// Frozen verbatim from <c>TradePackets.cs</c>.
    internal sealed class AcceptTrade
    {
        public void Read(WorldPacket p)
        {
            StateIndex = p.ReadUInt32();
        }

        public uint StateIndex;
    }

    /// Frozen verbatim from <c>TradePackets.cs</c>.
    internal sealed class ClearTradeItem
    {
        public void Read(WorldPacket p)
        {
            TradeSlot = p.ReadUInt8();
        }

        public byte TradeSlot;
    }

    /// Frozen verbatim from <c>TradePackets.cs</c>.
    internal sealed class SetTradeItem
    {
        public void Read(WorldPacket p)
        {
            TradeSlot = p.ReadUInt8();
            PackSlot = p.ReadUInt8();
            ItemSlotInPack = p.ReadUInt8();
        }

        public byte TradeSlot;
        public byte PackSlot;
        public byte ItemSlotInPack;
    }

/// Frozen verbatim from <c>MailPackets.cs</c>.
    internal sealed class MailGetList
    {
        public void Read(WorldPacket p)
        {
            Mailbox = p.ReadPackedGuid128();
        }

        public WowGuid128 Mailbox;
    }

    /// Frozen verbatim from <c>MailPackets.cs</c>.
    internal sealed class MailCreateTextItem
    {
        public void Read(WorldPacket p)
        {
            Mailbox = p.ReadPackedGuid128();
            MailID = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
                ? p.ReadInt64()
                : p.ReadUInt32();
        }

        public WowGuid128 Mailbox;
        public long MailID;
    }

    /// Frozen verbatim from <c>MailPackets.cs</c>.
    internal sealed class MailDelete
    {
        public void Read(WorldPacket p)
        {
            MailID = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
                ? p.ReadInt64()
                : p.ReadUInt32();
            DeleteReason = p.ReadInt32();
        }

        public long MailID;
        public int DeleteReason;
    }

    /// Frozen verbatim from <c>MailPackets.cs</c>.
    internal sealed class MailMarkAsRead
    {
        public void Read(WorldPacket p)
        {
            Mailbox = p.ReadPackedGuid128();
            MailID = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
                ? p.ReadInt64()
                : p.ReadUInt32();
        }

        public WowGuid128 Mailbox;
        public long MailID;
    }

    /// Frozen verbatim from <c>MailPackets.cs</c>.
    internal sealed class MailReturnToSender
    {
        public void Read(WorldPacket p)
        {
            MailID = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
                ? p.ReadInt64()
                : p.ReadUInt32();
            SenderGUID = p.ReadPackedGuid128();
        }

        public long MailID;
        public WowGuid128 SenderGUID;
    }

    /// Frozen verbatim from <c>MailPackets.cs</c>.
    internal sealed class MailTakeItem
    {
        public void Read(WorldPacket p)
        {
            Mailbox = p.ReadPackedGuid128();
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                MailID = p.ReadInt64();
                AttachID = p.ReadInt64();
            }
            else
            {
                MailID = p.ReadUInt32();
                AttachID = p.ReadUInt32();
            }
        }

        public WowGuid128 Mailbox;
        public long MailID;
        public long AttachID;
    }

    /// Frozen verbatim from <c>MailPackets.cs</c>.
    internal sealed class MailTakeMoney
    {
        public void Read(WorldPacket p)
        {
            Mailbox = p.ReadPackedGuid128();
            MailID = ModernVersion.Build == ClientVersionBuild.V3_4_3_54261
                ? p.ReadInt64()
                : p.ReadUInt32();
            Money = p.ReadInt64();
        }

        public WowGuid128 Mailbox;
        public long MailID;
        public long Money;
    }

    /// Frozen verbatim from <c>MailPackets.cs</c>.
    internal sealed class SendMail
    {
        public void Read(WorldPacket p)
        {
            Mailbox = p.ReadPackedGuid128();
            StationeryID = p.ReadInt32();
            SendMoney = p.ReadInt64();
            Cod = p.ReadInt64();

            uint targetLength = p.ReadBits<uint>(9);
            uint subjectLength = p.ReadBits<uint>(9);
            uint bodyLength = p.ReadBits<uint>(11);

            uint count = p.ReadBits<uint>(5);

            Target = p.ReadString(targetLength);
            Subject = p.ReadString(subjectLength);
            Body = p.ReadString(bodyLength);

            for (var i = 0; i < count; ++i)
            {
                var att = new MailAttachment()
                {
                    AttachPosition = p.ReadUInt8(),
                    ItemGUID = p.ReadPackedGuid128()
                };

                Attachments.Add(att);
            }
        }

        public WowGuid128 Mailbox;
        public int StationeryID;
        public long SendMoney;
        public long Cod;
        public string Target = string.Empty;
        public string Subject = string.Empty;
        public string Body = string.Empty;
        public List<MailAttachment> Attachments = new();

        public struct MailAttachment
        {
            public byte AttachPosition;
            public WowGuid128 ItemGUID;
        }
    }

    /// Frozen verbatim from <c>PetitionPackets.cs</c>.
    internal sealed class PetitionBuy
    {
        public void Read(WorldPacket p)
        {
            uint titleLen = p.ReadBits<uint>(7);
            Unit = p.ReadPackedGuid128();
            Index = p.ReadUInt32();
            Title = p.ReadString(titleLen);
        }

        public WowGuid128 Unit;
        public uint Index;
        public string Title = string.Empty;
    }

    /// Frozen verbatim from <c>PetitionPackets.cs</c>.
    internal sealed class PetitionShowSignatures
    {
        public void Read(WorldPacket p)
        {
            Item = p.ReadPackedGuid128();
        }

        public WowGuid128 Item;
    }

    /// Frozen verbatim from <c>PetitionPackets.cs</c>.
    internal sealed class QueryPetition
    {
        public void Read(WorldPacket p)
        {
            PetitionID = p.ReadUInt32();
            ItemGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 ItemGUID;
        public uint PetitionID;
    }

    /// Frozen verbatim from <c>PetitionPackets.cs</c>.
    internal sealed class PetitionRenameGuild
    {
        public void Read(WorldPacket p)
        {
            PetitionGuid = p.ReadPackedGuid128();

            p.ResetBitPos();
            uint nameLen = p.ReadBits<uint>(7);

            NewGuildName = p.ReadString(nameLen);
        }

        public WowGuid128 PetitionGuid;
        public string NewGuildName = string.Empty;
    }

    /// Frozen verbatim from <c>PetitionPackets.cs</c>.
    internal sealed class OfferPetition
    {
        public void Read(WorldPacket p)
        {
            UnkInt = p.ReadUInt32();
            ItemGUID = p.ReadPackedGuid128();
            TargetPlayer = p.ReadPackedGuid128();
        }

        public uint UnkInt;
        public WowGuid128 TargetPlayer;
        public WowGuid128 ItemGUID;
    }

    /// Frozen verbatim from <c>PetitionPackets.cs</c>.
    internal sealed class DeclinePetition
    {
        public void Read(WorldPacket p)
        {
            PetitionGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 PetitionGUID;
    }

    /// Frozen verbatim from <c>PetitionPackets.cs</c>.
    internal sealed class SignPetition
    {
        public void Read(WorldPacket p)
        {
            PetitionGUID = p.ReadPackedGuid128();
            Choice = p.ReadUInt8();
        }

        public WowGuid128 PetitionGUID;
        public byte Choice;
    }

    /// Frozen verbatim from <c>PetitionPackets.cs</c>.
    internal sealed class TurnInPetition
    {
        public void Read(WorldPacket p)
        {
            Item = p.ReadPackedGuid128();

            if (p.CanRead())
            {
                BackgroundColor = p.ReadUInt32();
                EmblemStyle = p.ReadUInt32();
                EmblemColor = p.ReadUInt32();
                BorderStyle = p.ReadUInt32();
                BorderColor = p.ReadUInt32();
            }
        }

        public WowGuid128 Item;
        public uint BackgroundColor;
        public uint EmblemStyle;
        public uint EmblemColor;
        public uint BorderStyle;
        public uint BorderColor;
    }

/// Frozen verbatim from <c>SocialPackets.cs</c>.
    internal sealed class ContactListRequest
    {
        public void Read(WorldPacket p)
        {
            Flags = (SocialFlag)p.ReadUInt32();
        }

        public SocialFlag Flags;
    }

    /// Frozen verbatim from <c>SocialPackets.cs</c>.
    internal sealed class AddFriend
    {
        public void Read(WorldPacket p)
        {
            uint nameLength = p.ReadBits<uint>(9);
            uint noteslength = p.ReadBits<uint>(10);
            Name = p.ReadString(nameLength);
            Note = p.ReadString(noteslength);
        }

        public string Note = string.Empty;
        public string Name = string.Empty;
    }

    /// Frozen verbatim from <c>SocialPackets.cs</c>.
    internal sealed class AddIgnore
    {
        public void Read(WorldPacket p)
        {
            uint nameLength = p.ReadBits<uint>(9);
            if (ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3))
                AccountGuid = p.ReadPackedGuid128();
            Name = p.ReadString(nameLength);
        }

        WowGuid128 AccountGuid;
        public string Name = string.Empty;
    }

    /// Frozen verbatim from <c>SocialPackets.cs</c>.
    internal sealed class DelFriend
    {
        public void Read(WorldPacket p)
        {
            VirtualRealmAddress = p.ReadUInt32();
            Guid = p.ReadPackedGuid128();
        }

        public uint VirtualRealmAddress;
        public WowGuid128 Guid;
    }

    /// Frozen verbatim from <c>SocialPackets.cs</c>.
    internal sealed class SetContactNotes
    {
        public void Read(WorldPacket p)
        {
            VirtualRealmAddress = p.ReadUInt32();
            Guid = p.ReadPackedGuid128();
            Notes = p.ReadString(p.ReadBits<uint>(10));
        }

        public uint VirtualRealmAddress;
        public WowGuid128 Guid;
        public string Notes = string.Empty;
    }

    /// Frozen verbatim from <c>ReputationPackets.cs</c>.
    internal sealed class SetFactionAtWar
    {
        public void Read(WorldPacket p)
        {
            FactionIndex = p.ReadUInt8();
        }

        public byte FactionIndex;
    }

    /// Frozen verbatim from <c>ReputationPackets.cs</c>.
    internal sealed class SetFactionNotAtWar
    {
        public void Read(WorldPacket p)
        {
            FactionIndex = p.ReadUInt8();
        }

        public byte FactionIndex;
    }

    /// Frozen verbatim from <c>ReputationPackets.cs</c>.
    internal sealed class SetFactionInactive
    {
        public void Read(WorldPacket p)
        {
            FactionIndex = p.ReadUInt32();
            State = p.HasBit();
        }

        public uint FactionIndex;
        public bool State;
    }

    /// Frozen verbatim from <c>ReputationPackets.cs</c>.
    internal sealed class SetWatchedFaction
    {
        public void Read(WorldPacket p)
        {
            FactionIndex = p.ReadUInt32();
        }

        public uint FactionIndex;
    }

    /// Frozen verbatim from <c>DuelPackets.cs</c>.
    internal sealed class CanDuel
    {
        public void Read(WorldPacket p)
        {
            TargetGUID = p.ReadPackedGuid128();
        }

        public WowGuid128 TargetGUID;
    }

    /// Frozen verbatim from <c>DuelPackets.cs</c>.
    internal sealed class DuelResponse
    {
        public void Read(WorldPacket p)
        {
            ArbiterGUID = p.ReadPackedGuid128();
            Accepted = p.HasBit();
            Forfeited = p.HasBit();
        }

        public WowGuid128 ArbiterGUID;
        public bool Accepted;
        public bool Forfeited;
    }

    /// Frozen verbatim from <c>CollectionPackets.cs</c>.
    internal sealed class ToyClearFanfare
    {
        public void Read(WorldPacket p)
        {
            ItemID = p.ReadUInt32();
        }

        public uint ItemID;
    }

    /// Frozen verbatim from <c>CollectionPackets.cs</c>.
    internal sealed class AddToy
    {
        public void Read(WorldPacket p)
        {
            Guid = p.ReadPackedGuid128();
        }

        public WowGuid128 Guid = WowGuid128.Empty;
    }

    /// Frozen verbatim from <c>CollectionPackets.cs</c>.
    internal sealed class CollectionItemSetFavorite
    {
        public void Read(WorldPacket p)
        {
            // Wrathion CollectionPackets.cpp: CollectionType is int32, not uint8
            Type = (ItemCollectionType)p.ReadInt32();
            ID = p.ReadUInt32();
            IsFavorite = p.ReadBit();
        }

        public ItemCollectionType Type;
        public uint ID;
        public bool IsFavorite;
    }

/// Frozen verbatim from <c>DFGetSystemInfoPkt.cs</c>.
    internal sealed class DFGetSystemInfoPkt
    {
        public bool Player;

        public void Read(WorldPacket p)
        {
            Player = p.HasBit();
            // optional PartyIndex byte follows — unused
        }
    }

    /// Frozen verbatim from <c>DFGetJoinStatusPkt.cs</c>.
    internal sealed class DFGetJoinStatusPkt
    {
        public void Read(WorldPacket p) { }
    }

    /// Frozen verbatim from <c>DFJoinPkt.cs</c>.
    internal sealed class DFJoinPkt
    {
        public bool QueueAsGroup;
        public byte Roles;
        public uint[] Slots = System.Array.Empty<uint>();

        public void Read(WorldPacket p)
        {
            QueueAsGroup = p.HasBit();
            bool hasPartyIndex = p.HasBit();
            p.HasBit(); // Mercenary
            Roles = p.ReadUInt8();
            uint slotCount = p.ReadUInt32();
            if (hasPartyIndex)
                p.ReadUInt8();
            Slots = new uint[slotCount];
            for (int i = 0; i < slotCount; i++)
                Slots[i] = p.ReadUInt32();
        }
    }

    /// Frozen verbatim from <c>DFLeavePkt.cs</c>.
    internal sealed class DFLeavePkt
    {
        public void Read(WorldPacket p) { }
    }

    /// Frozen verbatim from <c>DFTeleportPkt.cs</c>.
    internal sealed class DFTeleportPkt
    {
        public bool TeleportOut;

        public void Read(WorldPacket p)
        {
            TeleportOut = p.HasBit();
        }
    }

    /// Frozen verbatim from <c>DFSetRolesPkt.cs</c>.
    internal sealed class DFSetRolesPkt
    {
        public byte Roles;

        public void Read(WorldPacket p)
        {
            Roles = p.ReadUInt8();
            // optional PartyIndex byte — unused
        }
    }

    /// Frozen verbatim from <c>DFProposalResponsePkt.cs</c>.
    internal sealed class DFProposalResponsePkt
    {
        public RideTicket Ticket = new();
        public ulong InstanceID;
        public uint ProposalID;
        public bool Accepted;

        public void Read(WorldPacket p)
        {
            // RideTicket.Read owns the V3_4_3 trailing Unknown925 bit and the byte-align that
            // follows it (see BattleGroundPackets.RideTicket). This used to consume a second
            // one here, which over-read the buffer by a byte and made the final Accepted bit
            // throw IndexOutOfRangeException, crashing the proxy on every proposal reply (#103).
            Ticket.Read(p);
            InstanceID = p.ReadUInt64();
            ProposalID = p.ReadUInt32();
            Accepted = p.HasBit();
        }
    }

    /// Frozen verbatim from <c>LFGListGetStatusPkt.cs</c>.
    internal sealed class LFGListGetStatusPkt
    {
        public void Read(WorldPacket p) { }
    }

    /// Frozen verbatim from <c>AuctionPackets.cs</c>: the auction helper types as they were
    /// before the auction slice made them readonly. The oracles below assign their fields one
    /// by one, which a readonly type cannot express — and pointing them at the converted types
    /// would have them validate the new readers against themselves.
    internal class ClassFilter
    {
        public int ItemClass;
        public List<SubClassFilter> SubClassFilters = new();
    }

    internal struct SubClassFilter
    {
        public int ItemSubclass;
        public uint InvTypeMask;
    }

    internal struct AuctionSort
    {
        public byte Type;
        public byte Direction;
    }

    internal struct AuctionItemForSale
    {
        public AuctionItemForSale(WorldPacket data)
        {
            Guid = data.ReadPackedGuid128();
            UseCount = data.ReadUInt32();
        }

        public WowGuid128 Guid;
        public uint UseCount;
    }

    internal class AddOnInfo
    {
        public void Read(WorldPacket data)
        {
            data.ResetBitPos();

            uint nameLength = data.ReadBits<uint>(10);
            uint versionLength = data.ReadBits<uint>(10);
            Loaded = data.HasBit();
            Disabled = data.HasBit();
            if (nameLength > 1)
            {
                Name = data.ReadString(nameLength - 1);
                data.ReadUInt8(); // null terminator
            }
            if (versionLength > 1)
            {
                Version = data.ReadString(versionLength - 1);
                data.ReadUInt8(); // null terminator
            }
        }

        public string Name = string.Empty;
        public string Version = string.Empty;
        public bool Loaded;
        public bool Disabled;
    }

/// Frozen verbatim from <c>AuctionPackets.cs</c>.
    internal sealed class AuctionListBidderItems
    {
        public void Read(WorldPacket p)
        {
            Auctioneer = p.ReadPackedGuid128();
            Offset = p.ReadUInt32();

            uint auctionIDCount = p.ReadBits<uint>(7);
            p.ResetBitPos();

            // Add, not the indexer: the list starts empty and List<T>'s setter requires an index
            // below Count, so assigning through it threw for every client that had an active bid.
            for (var i = 0; i < auctionIDCount; ++i)
                AuctionItemIDs.Add(p.ReadUInt32());
        }

        public WowGuid128 Auctioneer;
        public uint Offset;
        public List<uint> AuctionItemIDs = new();
    }

    /// Frozen verbatim from <c>AuctionPackets.cs</c>.
    internal sealed class AuctionListItems
    {
        public void Read(WorldPacket p)
        {
            if (ModernVersion.Build == ClientVersionBuild.V3_4_3_54261)
            {
                // V3_4_3 CMSG_AUCTION_LIST_ITEMS — wire per CypherCore WotLK-Classic
                // AuctionListItems.Read (the authoritative 54261 layout; TC 3.4.3_Source and WPP's
                // retail handler both mis-handle it). Auctioneer precedes Offset; the pet bytes,
                // class-filter bodies and sort bodies ARE sent — earlier revisions discarded the
                // filter bodies, which silently dropped category filtering (#85).
                Auctioneer = p.ReadPackedGuid128();
                Offset = p.ReadUInt32();
                MinLevel = p.ReadUInt8();
                MaxLevel = p.ReadUInt8();
                Quality = p.ReadInt32();
                int sortsCount = p.ReadUInt8();
                int knownPetSize = p.ReadInt32();
                MaxPetLevel = (byte)p.ReadInt8();

                for (int i = 0; i < knownPetSize; ++i)
                    KnownPets.Add(p.ReadUInt8());

                bool tainted = p.HasBit();
                uint nameLen = p.ReadBits<uint>(8);
                Name = p.ReadString(nameLen);

                p.ResetBitPos();
                uint itemClassFilterCount = p.ReadBits<uint>(3);
                OnlyUsable = p.HasBit();
                ExactMatch = p.HasBit();

                if (tainted)
                {
                    // Consume the AddOnInfo (TaintedBy) body to stay aligned; not forwarded.
                    p.ResetBitPos();
                    uint addonNameLen = p.ReadBits<uint>(10);
                    uint addonVerLen = p.ReadBits<uint>(10);
                    p.HasBit(); // Loaded
                    p.HasBit(); // Disabled
                    if (addonNameLen > 1) { p.ReadString(addonNameLen - 1); p.ReadUInt8(); }
                    if (addonVerLen > 1) { p.ReadString(addonVerLen - 1); p.ReadUInt8(); }
                }

                for (uint i = 0; i < itemClassFilterCount; ++i)
                {
                    ClassFilter classFilter = new ClassFilter();
                    classFilter.ItemClass = p.ReadInt32();
                    uint subClassFilterCount = p.ReadBits<uint>(5);
                    for (uint j = 0; j < subClassFilterCount; ++j)
                    {
                        SubClassFilter filter = new SubClassFilter();
                        filter.InvTypeMask = (uint)p.ReadUInt64();
                        filter.ItemSubclass = p.ReadInt32();
                        classFilter.SubClassFilters.Add(filter);
                    }
                    ClassFilters.Add(classFilter);
                }

                p.ReadInt32(); // sortDataSize
                for (int i = 0; i < sortsCount; ++i)
                {
                    AuctionSort sort = new AuctionSort();
                    p.ResetBitPos();
                    sort.Type = p.ReadUInt8();
                    sort.Direction = p.ReadUInt8();
                    Sorts.Add(sort);
                }
                return;
            }

            Offset = p.ReadUInt32();
            Auctioneer = p.ReadPackedGuid128();

            MinLevel = p.ReadUInt8();
            MaxLevel = p.ReadUInt8();
            Quality = p.ReadInt32();
            var sortCount = p.ReadUInt8();
            var knownPetsCount = p.ReadUInt32();
            MaxPetLevel = p.ReadUInt8();

            for (int i = 0; i < knownPetsCount; ++i)
                KnownPets.Add(p.ReadUInt8());

            uint nameLength = p.ReadBits<uint>(8);
            Name = p.ReadString(nameLength);

            uint classFiltersCount = p.ReadBits<uint>(3);

            OnlyUsable = p.HasBit();
            ExactMatch = p.HasBit();
            p.ResetBitPos();

            for (int i = 0; i < classFiltersCount; ++i)
            {
                ClassFilter classFilter = new ClassFilter();
                classFilter.ItemClass = p.ReadInt32();

                uint subClassFiltersCount = p.ReadBits<uint>(5);
                for (uint j = 0; j < subClassFiltersCount; ++j)
                {
                    SubClassFilter filter = new SubClassFilter();
                    filter.ItemSubclass = p.ReadInt32();
                    filter.InvTypeMask = p.ReadUInt32();
                    classFilter.SubClassFilters.Add(filter);
                }

                ClassFilters.Add(classFilter);
            }

            var size = p.ReadUInt32();
            var data = p.ReadBytes(size);
            var sorts = new WorldPacket(p.GetOpcode(), data);
            for (var i = 0; i < sortCount; ++i)
            {
                AuctionSort sort = new AuctionSort();
                sort.Type = sorts.ReadUInt8();
                sort.Direction = sorts.ReadUInt8();
                Sorts.Add(sort);
            }
        }

        public uint Offset;
        public WowGuid128 Auctioneer;
        public byte MinLevel;
        public byte MaxLevel;
        public int Quality;
        public byte MaxPetLevel;
        public List<byte> KnownPets = new();
        public string Name = string.Empty;
        public bool OnlyUsable;
        public bool ExactMatch;
        public List<ClassFilter> ClassFilters = new List<ClassFilter>();
        public List<AuctionSort> Sorts = new();
    }

    /// Frozen verbatim from <c>AuctionPackets.cs</c>.
    internal sealed class AuctionSellItem
    {
        public void Read(WorldPacket p)
        {
            Auctioneer = p.ReadPackedGuid128();
            MinBid = p.ReadUInt64();
            BuyoutPrice = p.ReadUInt64();
            ExpireTime = p.ReadUInt32();

            if (p.HasBit())
                TaintedBy = new();

            int itemCountBits = ModernVersion.AddedInClassicVersion(1, 14, 3, 2, 5, 4) ? 6 : 5;
            uint itemCount = p.ReadBits<uint>(itemCountBits);

            if (TaintedBy != null)
                TaintedBy.Read(p);

            for (var i = 0; i < itemCount; ++i)
                Items.Add(new AuctionItemForSale(p));
        }

        public ulong BuyoutPrice;
        public WowGuid128 Auctioneer;
        public ulong MinBid;
        public uint ExpireTime;
        public AddOnInfo TaintedBy = null!;
        public List<AuctionItemForSale> Items = new();
    }

    /// Frozen verbatim from <c>AuctionPackets.cs</c>.
    internal sealed class AuctionRemoveItem
    {
        public void Read(WorldPacket p)
        {
            Auctioneer = p.ReadPackedGuid128();
            AuctionID = p.ReadUInt32();
            if (p.HasBit())
                TaintedBy = new();

            if (TaintedBy != null)
                TaintedBy.Read(p);
        }

        public WowGuid128 Auctioneer;
        public uint AuctionID;
        public AddOnInfo TaintedBy = null!;
    }

    /// Frozen verbatim from <c>AuctionPackets.cs</c>.
    internal sealed class AuctionPlaceBid
    {
        public WowGuid128 Auctioneer;
        public ulong BidAmount;
        public uint AuctionID;
        public AddOnInfo TaintedBy = null!;

        public void Read(WorldPacket p)
        {
            Auctioneer = p.ReadPackedGuid128();
            AuctionID = p.ReadUInt32();
            BidAmount = p.ReadUInt64();
            if (p.HasBit())
                TaintedBy = new();

            if (TaintedBy != null)
                TaintedBy.Read(p);
        }
    }
}
