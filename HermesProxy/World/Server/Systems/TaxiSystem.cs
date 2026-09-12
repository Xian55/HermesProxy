using System;
using System.Collections.Generic;
using Framework.Constants;
using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's flight-master CMSGs, including the Dijkstra route search
/// that turns a single destination into the multi-hop express route a legacy server expects.
/// </summary>
/// <remarks>
/// The path helpers below were instance methods on <c>WorldSocket</c> that never touched
/// <see langword="this"/> — they read <c>GameData</c> and their arguments. They become private
/// statics unchanged; nothing outside this file ever called them.
/// </remarks>
public static class TaxiSystem
{
    [HandlesCmsg(Opcode.CMSG_TAXI_NODE_STATUS_QUERY)]
    [HandlesCmsg(Opcode.CMSG_TAXI_QUERY_AVAILABLE_NODES)]
    public static void HandleTaxiNodesQuery(Opcode opcode, in InteractWithNPC interact, in SessionContext ctx)
    {
        WorldPacket packet = new WorldPacket(opcode);
        packet.WriteGuid(interact.CreatureGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_ENABLE_TAXI_NODE)]
    public static void HandleEnableTaxiNode(in InteractWithNPC interact, in SessionContext ctx)
    {
        // The modern client sends this for a flight master whose node it has not
        // discovered yet, meaning "open the taxi map". Forwarding it as
        // CMSG_TALK_TO_GOSSIP (gossip-hello) makes TrinityCore run the creature's
        // gossip menu instead, which never answers with SMSG_SHOW_TAXI_NODES -- so
        // the node stayed undiscovered, the client re-sent this opcode forever, and
        // CurrentTaxiNode / UsableTaxiNodes never populated, leaving CMSG_ACTIVATE_TAXI
        // and the multi-hop express route dead too. See issue #252.
        //
        // CMSG_TAXI_QUERY_AVAILABLE_NODES is the handler that discovers the node and
        // sends the map, and takes the same payload -- one non-packed creature GUID.
        //
        // Ungated: the opcode is defined at 0x1AC on V1_12_1, V2_4_3 and V3_3_5a with
        // the same payload, and VMaNGOS's vanilla HandleTaxiQueryAvailableNodes is
        // functionally the same as the WotLK one -- SendLearnNewTaxiNode for the
        // unknown-node case, SendTaxiMenu for the known one. Verified on TC 3.3.5a
        // (first click discovers, second opens the map); vanilla and TBC to be
        // confirmed against a live backend with an undiscovered flight master.
        WorldPacket packet = new WorldPacket(Opcode.CMSG_TAXI_QUERY_AVAILABLE_NODES);
        packet.WriteGuid(interact.CreatureGUID.To64());
        ctx.SendPacketToServer(packet);
    }

    [HandlesCmsg(Opcode.CMSG_ACTIVATE_TAXI)]
    public static void HandleActivateTaxi(in ActivateTaxi taxi, in SessionContext ctx)
    {
        // direct path exist
        if (TaxiPathExist(ctx.GetSession().GameState.CurrentTaxiNode, taxi.Node))
        {
            WorldPacket packet = new WorldPacket(Opcode.CMSG_ACTIVATE_TAXI);
            packet.WriteGuid(taxi.FlightMaster.To64());
            packet.WriteUInt32(ctx.GetSession().GameState.CurrentTaxiNode);
            packet.WriteUInt32(taxi.Node);
            ctx.SendPacketToServer(packet);
        }
        else // find shortest path
        {
            HashSet<uint> path = GetTaxiPath(ctx.GetSession().GameState.CurrentTaxiNode, taxi.Node, ctx.GetSession().GameState.UsableTaxiNodes);
            if (path.Count <= 1) // no nodes found
                return;

            WorldPacket packet = new WorldPacket(Opcode.CMSG_ACTIVATE_TAXI_EXPRESS);
            packet.WriteGuid(taxi.FlightMaster.To64());

            // The cost field was removed in 3.2.0.10192 (WPP reads it under
            // RemovedInVersion(V3_2_0_10192); VMaNGOS 1.12 still parses
            // guid > totalcost > node_count, while AzerothCore, cMaNGOS-wotlk and
            // TrinityCore 3.3.5a all parse guid > node_count).
            //
            // Writing it unconditionally made every WotLK server read node_count
            // from our cost field, i.e. 0, then return on `if (nodes.empty())`
            // without a reply of any kind -- so multi-hop flights silently did
            // nothing while direct ones worked. Vanilla and TBC still need it.
            if (LegacyVersion.RemovedInVersion(ClientVersionBuild.V3_2_0_10192))
                packet.WriteUInt32(0); // total cost, not used

            packet.WriteUInt32((uint)path.Count); // node count
            foreach (uint itr in path)
                packet.WriteUInt32(itr);
            ctx.SendPacketToServer(packet);
        }
        ctx.GetSession().GameState.IsWaitingForTaxiStart = true;
    }

    static bool TaxiPathExist(uint from, uint to)
    {
        foreach (var itr in GameData.TaxiPaths)
        {
            if (itr.Value.From == from && itr.Value.To == to ||
                itr.Value.From == to && itr.Value.To == from)
                return true;
        }
        return false;
    }

    static bool IsTaxiNodeKnown(uint node, List<byte> usableNodes)
    {
        if (node == 0)
            return false;

        uint field = (node - 1) / 8;
        // The mask is only as wide as the legacy server sent, and is empty until
        // SMSG_SHOW_TAXI_NODES arrives, while the graph spans every node id in
        // TaxiNodes{N}.csv (440 on WotLK). Past the end simply means "not known".
        if (field >= (uint)usableNodes.Count)
            return false;

        uint submask = 1u << (int)((node - 1) % 8);
        return (usableNodes[(int)field] & submask) == submask;
    }

    static HashSet<uint> GetTaxiPath(uint from, uint to, List<byte> usableNodes)
    {
        // shortest path node list
        HashSet<uint> nodes = new HashSet<uint> { from };

        // Both ends index dist[] / parent[] inside Dijkstra. `to` comes straight
        // from the modern client's CMSG_ACTIVATE_TAXI and `from` from the legacy
        // stream, so neither is trustworthy enough to index an array with.
        int width = GameData.TaxiNodesGraph.GetLength(0);
        if (from >= (uint)width || to >= (uint)width)
            return nodes;

        // copy taxi nodes graph and disable unknown nodes
        int[,] graphCopy = new int[GameData.TaxiNodesGraph.GetLength(0), GameData.TaxiNodesGraph.GetLength(1)];
        Buffer.BlockCopy(GameData.TaxiNodesGraph, 0, graphCopy, 0, GameData.TaxiNodesGraph.Length * sizeof(uint));
        for (uint i = 1; i < graphCopy.GetLength(0); i++)
        {
            if (!IsTaxiNodeKnown(i, usableNodes))
            {
                for (uint itr = 0; itr < graphCopy.GetLength(1); itr++)
                    graphCopy[i, itr] = 0;

                for (uint itr = 0; itr < graphCopy.GetLength(0); itr++)
                    graphCopy[itr, i] = 0;
            }
        }
        int minDist = Dijkstra(graphCopy, (int)from, (int)to, graphCopy.GetLength(0), nodes);
        return nodes;
    }

    static int MinDistance(int[] dist, bool[] sptSet, int vCnt)
    {
        int min = int.MaxValue, min_index = -1;
        for (int v = 0; v < vCnt; v++)
            if (sptSet[v] == false && dist[v] <= min)
            {
                min = dist[v];
                min_index = v;
            }
        return min_index;
    }

    static void SavePath(int[] parent, int j, HashSet<uint> nodes)
    {
        if (parent[j] == -1)
            return;
        SavePath(parent, parent[j], nodes);
        nodes.Add((uint)j);
    }

    // taken from https://www.geeksforgeeks.org/printing-paths-dijkstras-shortest-path-algorithm/
    static int Dijkstra(int[,] graph, int src, int dest, int vCnt, HashSet<uint> nodes)
    {
        int[] dist = new int[vCnt];
        int[] parent = new int[vCnt];
        bool[] sptSet = new bool[vCnt];
        for (int i = 0; i < vCnt; i++)
        {
            dist[i] = int.MaxValue;
            sptSet[i] = false;
            parent[i] = -1;
        }
        dist[src] = 0;
        for (int count = 0; count < vCnt - 1; count++)
        {
            int u = MinDistance(dist, sptSet, vCnt);
            sptSet[u] = true;

            for (int v = 0; v < vCnt; v++)
            {
                if (!sptSet[v] && graph[u, v] != 0 &&
                     dist[u] != int.MaxValue && dist[u] + graph[u, v] < dist[v])
                {
                    parent[v] = u;
                    dist[v] = dist[u] + graph[u, v];
                }
            }
        }
        // save shortest path
        SavePath(parent, dest, nodes);
        // return shortest path distance
        return dist[dest];
    }
}
