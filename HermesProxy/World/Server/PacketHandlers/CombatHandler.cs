using HermesProxy.Enums;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server
{
    public partial class WorldSocket
    {
        // Handlers for CMSG opcodes coming from the modern client
        [PacketHandler(Opcode.CMSG_ATTACK_SWING)]
        void HandleAttackSwing(AttackSwing attack)
        {
            var victim64 = attack.Victim.To64();
            var state = GetSession().GameState;

            if (state.CurrentAttackTarget == victim64)
                return;

            // If we had a pending stop (STOP→SWING sequence), cancel it — just send the new SWING
            // The server handles target switching within ATTACK_SWING without needing an explicit STOP
            if (state.DeferredAttackStop)
                state.DeferredAttackStop = false;

            state.CurrentAttackTarget = victim64;
            state.WaitingForAttackStart = true;
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ATTACK_SWING);
            packet.WriteGuid(victim64);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_ATTACK_STOP)]
        void HandleAttackSwing(AttackStop attack)
        {
            var state = GetSession().GameState;

            // Always defer ATTACK_STOP when we have an active attack target
            // It will be flushed when SMSG_ATTACK_STOP arrives from server, or
            // cancelled if a new CMSG_ATTACK_SWING arrives first (target switch)
            if (state.CurrentAttackTarget != default)
            {
                state.DeferredAttackStop = true;
                return;
            }

            WorldPacket packet = new WorldPacket(Opcode.CMSG_ATTACK_STOP);
            SendPacketToServer(packet);
        }
        [PacketHandler(Opcode.CMSG_SET_SHEATHED)]
        void HandleSetSheathed(SetSheathed sheath)
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_SET_SHEATHED);
            packet.WriteInt32(sheath.SheathState);
            SendPacketToServer(packet);
        }
    }
}
