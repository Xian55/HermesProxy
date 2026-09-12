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
}
