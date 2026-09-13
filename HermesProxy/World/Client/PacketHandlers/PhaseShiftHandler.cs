using HermesProxy.World;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;

namespace HermesProxy.World.Client;

public partial class WorldClient
{
    [HandlesSmsg(Opcode.SMSG_PHASE_SHIFT_CHANGE)]
    internal void HandlePhaseShiftChange(WorldPacket packet)
    {
        uint mask = packet.ReadUInt32();
        var msg = PhaseShiftTranslation.ToModern(mask, GetSession().GameState.CurrentPlayerGuid);
        BattleGroundLogMessages.PhaseShift(_melLog, mask, msg.PhaseShiftFlags, msg.Phases.Count);
        SendPacketToClient(msg);
    }
}
