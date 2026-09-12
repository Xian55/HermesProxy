using Framework.Constants;
using Framework.Logging;
using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Logging;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;
using System;
using System.Collections.Frozen;
using System.Collections.Generic;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{




    // Handlers for CMSG opcodes coming from the modern client
























    // "Leave Vehicle" button on the modern V3_4_3 vehicle UI emits CMSG_MOVE_DISMISS_VEHICLE
    // (with a MovementInfo body) — the legacy 3.3.5a equivalent is CMSG_REQUEST_VEHICLE_EXIT,
    // which is empty and resolves the vehicle from session state. Rewrite the opcode and drop
    // the body to translate. Without this the click was getting routed through HandlePlayerMove
    // and silently degraded to MSG_MOVE_SET_FACING, leaving the player stuck in the vehicle
    // (e.g. Grand Theft Palomino quest 12680).
    //
    // PREV_SEAT / NEXT_SEAT / REQUEST_VEHICLE_EXIT all share the same empty wire shape and
    // are wired here as well, since they did not have any handler at all before.

}
