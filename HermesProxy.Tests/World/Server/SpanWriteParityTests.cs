using System;
using System.Linq;
using System.Reflection;
using Framework.IO;
using HermesProxy.World;
using HermesProxy.World.Server.Packets;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// WritePacketData ships WriteToSpan's bytes and only falls back to Write() when WriteToSpan
/// reports MaxSize exceeded, so the two bodies must agree byte for byte. Nothing else notices
/// when they drift: the client just receives a packet the Write() body does not describe (#156).
/// Packets are built from default field values, so this pins fixed layout and empty
/// collections rather than every populated shape.
/// </summary>
public class SpanWriteParityTests
{
    private static readonly Assembly ProxyAssembly = typeof(ServerPacket).Assembly;

    private static readonly FieldInfo WorldPacketField =
        typeof(ServerPacket).GetField("_worldPacket", BindingFlags.NonPublic | BindingFlags.Instance)!;

    private static readonly NullabilityInfoContext Nullability = new();

    public static TheoryData<string, bool> SpanWritablePackets()
    {
        var data = new TheoryData<string, bool>();
        var types = ProxyAssembly.GetTypes()
            .Where(t => !t.IsAbstract
                        && typeof(ServerPacket).IsAssignableFrom(t)
                        && typeof(ISpanWritable).IsAssignableFrom(t)
                        && t.GetConstructor(Type.EmptyTypes) != null)
            .OrderBy(t => t.FullName, StringComparer.Ordinal);

        foreach (var type in types)
        {
            data.Add(type.FullName!, false);
            // String fields default to "" or null depending on the packet; blanking them all
            // reaches the bits-then-empty-WriteString shape, which WriteString does not flush.
            data.Add(type.FullName!, true);
        }
        return data;
    }

    [Theory]
    [MemberData(nameof(SpanWritablePackets))]
    public void WriteToSpan_MatchesWrite(string typeName, bool blankStrings)
    {
        AssertParity(ProxyAssembly.GetType(typeName, throwOnError: true)!, blankStrings);
    }

    // LootRollWire writes DungeonEncounterID only on V3_4_3, which the test process never runs
    // as, so the theory above cannot reach that branch. It is where LootRemoved's span body
    // gained the field and LootRollsComplete's lost it, while both Write() bodies were right.
    [Theory]
    [InlineData(typeof(LootRollBroadcast))]
    [InlineData(typeof(LootRollWon))]
    [InlineData(typeof(LootRollsComplete))]
    [InlineData(typeof(LootRemoved))]
    public void WriteToSpan_MatchesWrite_WithDungeonEncounterId(Type type)
    {
        LootRollWire.ForceDungeonEncounterIdForTests = true;
        try
        {
            AssertParity(type, blankStrings: false);
        }
        finally
        {
            LootRollWire.ForceDungeonEncounterIdForTests = null;
        }
    }

    private static void AssertParity(Type type, bool blankStrings)
    {
        // Defaults first: a null sub-object is often an absent optional (a WriteBit(x != null)
        // gate), even when declared `= null!`, and that shape is worth pinning. Packets whose
        // sub-object is mandatory throw on it, and are retried with those filled in.
        try
        {
            AssertParity(type, blankStrings, fillSubObjects: false);
        }
        catch (NullReferenceException)
        {
            AssertParity(type, blankStrings, fillSubObjects: true);
        }
    }

    private static void AssertParity(Type type, bool blankStrings, bool fillSubObjects)
    {
        var viaSpan = Create(type, blankStrings, fillSubObjects);
        var viaWrite = Create(type, blankStrings, fillSubObjects);

        var spanWritable = (ISpanWritable)viaSpan;
        // Exactly MaxSize rather than a rounded-up pool rental, so an undercounted MaxSize fails too.
        var buffer = new byte[spanWritable.MaxSize];
        int written = spanWritable.WriteToSpan(buffer);
        if (written < 0)
            return; // WritePacketData sends Write() itself in this case; nothing can diverge.

        viaWrite.Write();
        byte[] expected = ((WorldPacket)WorldPacketField.GetValue(viaWrite)!).GetData();

        Assert.Equal(Convert.ToHexString(expected), Convert.ToHexString(buffer, 0, written));
    }

    private static ServerPacket Create(Type type, bool blankStrings, bool fillSubObjects)
    {
        ServerPacket packet;
        try
        {
            packet = (ServerPacket)Activator.CreateInstance(type)!;
        }
        catch (TargetInvocationException e) when (e.InnerException is UnmappedOpcodeException)
        {
            Assert.Skip($"{type.Name} has no opcode in {ModernVersion.Build}");
            throw;
        }

        if (blankStrings)
        {
            foreach (var field in type.GetFields(BindingFlags.Public | BindingFlags.Instance))
            {
                if (field.FieldType == typeof(string) && !field.IsInitOnly)
                    field.SetValue(packet, "");
            }
            foreach (var property in type.GetProperties(BindingFlags.Public | BindingFlags.Instance))
            {
                if (property.PropertyType == typeof(string) && property.CanWrite
                    && property.GetIndexParameters().Length == 0)
                    property.SetValue(packet, "");
            }
        }

        if (fillSubObjects)
            FillSubObjects(packet, depth: 0);

        return packet;
    }

    private static void FillSubObjects(object target, int depth)
    {
        if (depth > 3)
            return;

        foreach (var field in target.GetType().GetFields(BindingFlags.Public | BindingFlags.Instance))
        {
            if (field.IsInitOnly
                || field.FieldType == typeof(string)
                || !field.FieldType.IsClass
                || field.FieldType.GetConstructor(Type.EmptyTypes) == null
                || Nullability.Create(field).WriteState != NullabilityState.NotNull)
                continue;

            var value = field.GetValue(target);
            if (value == null)
            {
                value = Activator.CreateInstance(field.FieldType)!;
                field.SetValue(target, value);
            }
            FillSubObjects(value, depth + 1);
        }
    }
}
