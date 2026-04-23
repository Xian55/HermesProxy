using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Objects.Version.V3_4_3_54261;

// Phase 5 source-generated descriptor-tree serializer for WotLK Classic 3.4.3.
// The Write{Create,Update}*Data leaf methods are emitted by
// HermesProxy.SourceGen.ObjectUpdateBuilderGenerator from [DescriptorCreateField]
// attributes on the per-version field enums (e.g. V3_4_3_54261.ObjectField).
// Bootstrap scope: WriteCreateObjectData only. The WriteToPacket dispatcher and
// the remaining per-type methods (Item/Unit/Player/…) land in follow-up PRs.
public partial class ObjectUpdateBuilder
{
    private readonly ObjectUpdate _updateData;
    private readonly GameSessionData _gameState;

    public ObjectUpdateBuilder(ObjectUpdate updateData, GameSessionData gameState)
    {
        _updateData = updateData;
        _gameState = gameState;
    }

    public void WriteToPacket(WorldPacket packet)
    {
        throw new NotImplementedException(
            "V3_4_3_54261 ObjectUpdateBuilder is scaffolding only — the 3.4.3 descriptor-tree serializer ships in Phase 5. "
            + "See wotlk.md for the implementation plan.");
    }
}
