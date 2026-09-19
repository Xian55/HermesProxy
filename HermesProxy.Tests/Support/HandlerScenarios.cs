using HermesProxy.World;
using HermesProxy.World.Enums;

namespace HermesProxy.Tests.Support;

/// <summary>
/// Legacy packets shaped like the Alterac Valley traffic the 2026-09 allocation traces were taken
/// on, shared by the handler tests and benchmarks so both measure the same bytes.
/// </summary>
internal sealed class AlteracValleyScenario
{
    public const uint FireballSpellId = 133;
    public const uint OwnMana = 4210;

    public WowGuid64 LegacyPlayer { get; } = new(HighGuidTypeLegacy.Player, 1);
    public WowGuid64[] LegacyOtherPlayers { get; } =
        [new(HighGuidTypeLegacy.Player, 2), new(HighGuidTypeLegacy.Player, 3), new(HighGuidTypeLegacy.Player, 4)];
    public WowGuid64[] LegacyCreatures { get; } =
        [new(HighGuidTypeLegacy.Creature, 13358, 100), new(HighGuidTypeLegacy.Creature, 13358, 101),
         new(HighGuidTypeLegacy.Creature, 13359, 102), new(HighGuidTypeLegacy.Creature, 12050, 103)];

    public WowGuid128 Player { get; }
    public WowGuid128[] OtherPlayers { get; }
    public WowGuid128[] Creatures { get; }

    public AlteracValleyScenario(LegacyHandlerHarness harness)
    {
        Player = harness.SetActivePlayer(LegacyPlayer);
        OtherPlayers = new WowGuid128[LegacyOtherPlayers.Length];
        for (int i = 0; i < LegacyOtherPlayers.Length; i++)
            OtherPlayers[i] = harness.AddKnownObject(LegacyOtherPlayers[i], ObjectType.Player);
        Creatures = new WowGuid128[LegacyCreatures.Length];
        for (int i = 0; i < LegacyCreatures.Length; i++)
            Creatures[i] = harness.AddKnownObject(LegacyCreatures[i], ObjectType.Unit);
    }

    private static int Field(UnitField field) => LegacyVersion.GetUpdateField(field);
    private static int UnitEnd => LegacyVersion.GetUpdateField(UnitField.UNIT_END);
    private static int PlayerEnd => LegacyVersion.GetUpdateField(PlayerField.PLAYER_END);

    /// <summary>
    /// Eight Values blocks. AzerothCore resends the NPC and dynamic flags in every one, then adds
    /// the one or two fields that changed: health on the creatures, health and power on the other
    /// players, and the player's own mana, the only block that fills a power update.
    /// </summary>
    public byte[] BuildValuesBatch() => LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
    {
        packet.WriteUInt32((uint)(LegacyCreatures.Length + LegacyOtherPlayers.Length + 1));

        for (int i = 0; i < LegacyCreatures.Length; i++)
        {
            LegacyPacketBuilder.WriteValuesBlock(packet, LegacyCreatures[i], UnitEnd,
                (Field(UnitField.UNIT_FIELD_HEALTH), 5000u + (uint)i),
                (Field(UnitField.UNIT_NPC_FLAGS), i == 0 ? 1u : 0u),
                (Field(UnitField.UNIT_DYNAMIC_FLAGS), 0u));
        }

        for (int i = 0; i < LegacyOtherPlayers.Length; i++)
        {
            LegacyPacketBuilder.WriteValuesBlock(packet, LegacyOtherPlayers[i], PlayerEnd,
                (Field(UnitField.UNIT_FIELD_HEALTH), 7000u + (uint)i),
                (Field(UnitField.UNIT_FIELD_POWER1), 3000u + (uint)i),
                (Field(UnitField.UNIT_NPC_FLAGS), 0u),
                (Field(UnitField.UNIT_DYNAMIC_FLAGS), 0u));
        }

        LegacyPacketBuilder.WriteValuesBlock(packet, LegacyPlayer, PlayerEnd,
            (Field(UnitField.UNIT_FIELD_POWER1), OwnMana),
            (Field(UnitField.UNIT_NPC_FLAGS), 0u),
            (Field(UnitField.UNIT_DYNAMIC_FLAGS), 0u));
    });

    /// <summary>The player's own Values block carrying only a mana change.</summary>
    public byte[] BuildOwnPowerUpdate(uint mana) => LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
    {
        packet.WriteUInt32(1);
        LegacyPacketBuilder.WriteValuesBlock(packet, LegacyPlayer, PlayerEnd,
            (Field(UnitField.UNIT_FIELD_POWER1), mana));
    });

    /// <summary>Another player's Values block with only the player flags.</summary>
    public byte[] BuildPlayerFlags(uint flags) => LegacyPacketBuilder.Build(Opcode.SMSG_UPDATE_OBJECT, packet =>
    {
        packet.WriteUInt32(1);
        LegacyPacketBuilder.WriteValuesBlock(packet, LegacyOtherPlayers[0], PlayerEnd,
            (LegacyVersion.GetUpdateField(PlayerField.PLAYER_FLAGS), flags));
    });

    /// <summary>A creature's Fireball at another player: one hit, no misses, a unit target.</summary>
    public byte[] BuildSpellGo() => LegacyPacketBuilder.Build(Opcode.SMSG_SPELL_GO, packet =>
    {
        packet.WritePackedGuid(LegacyCreatures[0]);
        packet.WritePackedGuid(LegacyCreatures[0]);
        packet.WriteUInt8(0);                    // cast count
        packet.WriteUInt32(FireballSpellId);
        packet.WriteUInt32(0);                   // cast flags
        packet.WriteUInt32(123456);              // server time
        packet.WriteUInt8(1);                    // hit targets
        packet.WriteUInt64(LegacyOtherPlayers[0].Low);
        packet.WriteUInt8(0);                    // miss targets
        packet.WriteUInt32((uint)SpellCastTargetFlags.Unit);
        packet.WritePackedGuid(LegacyOtherPlayers[0]);
    });

    public byte[] BuildSpellStart() => LegacyPacketBuilder.Build(Opcode.SMSG_SPELL_START, packet =>
    {
        packet.WritePackedGuid(LegacyCreatures[0]);
        packet.WritePackedGuid(LegacyCreatures[0]);
        packet.WriteUInt8(0);                    // cast count
        packet.WriteUInt32(FireballSpellId);
        packet.WriteUInt32(0);                   // cast flags
        packet.WriteUInt32(3500);                // cast time
        packet.WriteUInt32((uint)SpellCastTargetFlags.Unit);
        packet.WritePackedGuid(LegacyOtherPlayers[0]);
    });
}
