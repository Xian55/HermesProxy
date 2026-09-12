using System.Collections.Generic;
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

}
