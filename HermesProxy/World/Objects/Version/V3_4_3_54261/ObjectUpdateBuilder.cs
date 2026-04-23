using HermesProxy.World.Server.Packets;
using System;

namespace HermesProxy.World.Objects.Version.V3_4_3_54261;

// Phase 1 stub — real implementation (descriptor-tree serializers) ships in Phase 5.
// See wotlk.md for the ~3,400-line reference in the fork's cc12fd6 commit and
// the Approach A (hand-port) vs Approach B (source-generated) fork in the road.
public class ObjectUpdateBuilder
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
