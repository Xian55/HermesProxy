using System.Collections.Generic;
using HermesProxy;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;

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
}
