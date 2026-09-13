using System;
using System.Collections.Generic;
using Bgs.Protocol;
using Bgs.Protocol.GameUtilities.V1;
using BNetServer.Services;
using Framework.Constants;
using Framework.IO;
using Framework.Logging;
using Framework.Serialization;
using Framework.Util;
using Framework.Web;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// The two session-level CMSGs that speak to the BNet RPC rather than the legacy server.
/// </summary>
/// <remarks>
/// Both reach the socket's RPC service manager, which is why these read <c>ctx.Socket</c> instead
/// of going through the session - the RPC is per-connection state, not per-session.
/// </remarks>
public static class SessionSystem
{
    [HandlesCmsg(Opcode.CMSG_CHANGE_REALM_TICKET)]
    public static void HandleChangeRealmTicket(in ChangeRealmTicket request, in SessionContext ctx)
    {
        ChangeRealmTicketResponse response = new();
        response.Token = request.Token;

        // Native never re-auths here. The legacy auth socket is already closed
        // after the first realm list; a full SRP login is rejected and Allow=false
        // is WOW51900300. Realm list is served from cache.
        if (ctx.Socket!.BnetRpc == null)
        {
            response.Allow = false;
            ctx.SendPacket(response);
            return;
        }

        ctx.Socket!.BnetRpc.SetClientSecret(request.Secret);
        response.Allow = true;
        response.Ticket = new ByteBuffer(new byte[1]);
        ctx.SendPacket(response);
    }

    [HandlesCmsg(Opcode.CMSG_BATTLENET_REQUEST)]
    public static void HandleBattlenetRequest(in BattlenetRequest request, in SessionContext ctx)
    {
        if (ctx.Socket!.BnetRpc == null)
        {
            Log.Print(LogType.Error, $"Client tried {Opcode.CMSG_BATTLENET_REQUEST} without authentication");
            return;
        }

        ctx.Socket!.BnetRpc.Invoke(
            serviceId: 0,
            (OriginalHash)request.Method.GetServiceHash(),
            request.Method.GetMethodId(),
            request.Method.Token,
            request.Data
        );
    }
}
