using HermesProxy.World.Enums;

namespace HermesProxy.World.Objects;

/// <summary>
/// The power held in each UNIT_FIELD_POWER / MAXPOWER slot on a 4.3.4 server.
/// </summary>
/// <remarks>
/// From Cataclysm a unit's power fields are indexed by its class's power list, in ascending power
/// type, instead of by power type: a warrior's rage is POWER1, where 3.3.5a put it in POWER2.
/// Read the 3.3.5a way, rage landed in the mana slot and never showed on the rage bar. The lists
/// are the 4.3.4 server's own ChrClassesXPowerTypes.dbc; every class also has alternate power
/// (10) last, which nothing here translates.
/// </remarks>
public static class LegacyPowerSlotsCata
{
    private const PowerType SoulShards = (PowerType)7;
    private const PowerType Eclipse = (PowerType)8;
    private const PowerType HolyPower = (PowerType)9;

    private static readonly PowerType[] Warrior = [PowerType.Rage];
    private static readonly PowerType[] Paladin = [PowerType.Mana, HolyPower];
    private static readonly PowerType[] Hunter = [PowerType.Focus];
    private static readonly PowerType[] Rogue = [PowerType.Energy];
    private static readonly PowerType[] ManaOnly = [PowerType.Mana];
    private static readonly PowerType[] DeathKnight = [PowerType.RunicPower];
    private static readonly PowerType[] Warlock = [PowerType.Mana, SoulShards];
    private static readonly PowerType[] Druid = [PowerType.Mana, PowerType.Rage, PowerType.Energy, Eclipse];

    /// <summary>The power in <paramref name="slot"/> for <paramref name="classId"/>, or Invalid.</summary>
    public static PowerType TypeAt(Class classId, int slot)
    {
        PowerType[] types = classId switch
        {
            Class.Warrior => Warrior,
            Class.Paladin => Paladin,
            Class.Hunter => Hunter,
            Class.Rogue => Rogue,
            Class.Deathknight => DeathKnight,
            Class.Warlock => Warlock,
            Class.Druid => Druid,
            Class.Priest or Class.Shaman or Class.Mage => ManaOnly,
            _ => [],
        };
        return slot < types.Length ? types[slot] : PowerType.Invalid;
    }

    /// <summary>A hunter pet's one power, focus, sits in the first slot.</summary>
    public static PowerType PetTypeAt(int slot) => slot == 0 ? PowerType.Focus : PowerType.Invalid;
}
