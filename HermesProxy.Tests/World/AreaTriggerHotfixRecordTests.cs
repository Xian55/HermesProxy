using System;
using Framework.IO;
using HermesProxy.World;
using Xunit;

namespace HermesProxy.Tests.World;

/// <summary>
/// The AreaTrigger row the proxy adds to a client by hotfix, in both shapes the supported clients
/// read. A client does not refuse the other shape: it stores the row and reads the radius and the
/// box from the wrong place, and the trigger never fires.
/// </summary>
public sealed class AreaTriggerHotfixRecordTests
{
    // Warsong Gulch, the Alliance flag: a sphere of 5 yards on map 489.
    private static readonly GameData.AreaTrigger AllianceFlag = new()
    {
        PositionX = 1539.89f,
        PositionY = 1481.36f,
        PositionZ = 352.659f,
        Id = 3646,
        MapId = 489,
        Radius = 5,
        ActionSetId = 18443,
    };

    private static string Write(bool wideFields)
    {
        using ByteBuffer content = new();
        GameData.WriteAreaTriggerRecord(content, AllianceFlag, wideFields);
        return Convert.ToHexString(content.GetData());
    }

    /// <summary>
    /// Layout 3845397A, as 3.4.3 has it. The bytes are the row as a client kept it in its
    /// Cache/ADB/enUS/DBCache.bin.
    /// </summary>
    [Fact]
    public void NarrowFields_Are50Bytes_PhaseFlagsOneByteAndActionSetTwo()
    {
        const string expected =
            "00" +                       // Message, empty
            "7B7CC044852BB9445A54B043" + // Pos
            "3E0E0000" +                 // ID
            "E901" +                     // ContinentID
            "00" +                       // PhaseUseFlags
            "0000" + "0000" +            // PhaseID, PhaseGroupID
            "0000A040" +                 // Radius
            "00000000000000000000000000000000" + // Box length, width, height, yaw
            "00" + "0000" +              // ShapeType, ShapeID
            "0B48" +                     // AreaTriggerActionSetID
            "00";                        // Flags

        Assert.Equal(expected, Write(wideFields: false));
        Assert.Equal(50 * 2, expected.Length);
    }

    /// <summary>
    /// Layout B4ACA6C2, as 4.4.2 has it (TrinityCore cata_classic AreaTriggerMeta): PhaseUseFlags
    /// and AreaTriggerActionSetID are 32 bits.
    /// </summary>
    [Fact]
    public void WideFields_Are55Bytes_PhaseFlagsAndActionSetFourEach()
    {
        const string expected =
            "00" +
            "7B7CC044852BB9445A54B043" +
            "3E0E0000" +
            "E901" +
            "00000000" +                 // PhaseUseFlags
            "0000" + "0000" +
            "0000A040" +
            "00000000000000000000000000000000" +
            "00" + "0000" +
            "0B480000" +                 // AreaTriggerActionSetID
            "00";

        Assert.Equal(expected, Write(wideFields: true));
        Assert.Equal(55 * 2, expected.Length);
    }
}
