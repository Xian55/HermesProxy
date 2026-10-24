namespace HermesProxy.World.Objects;

/// <summary>
/// The attack times a 3.3.5a server hastes, as AzerothCore and TrinityCore 3.3.5 set them
/// (Player::_ApplyWeaponDamage, Player::InitDataForForm). The server sends only the hasted result,
/// base × its haste multiplier, so the multiplier is the attack time over this base.
/// </summary>
internal static class AttackHaste
{
    /// <summary>
    /// BASE_ATTACK_TIME: no weapon, a broken or a disarmed one, or an item without a delay (a relic
    /// in the ranged slot).
    /// </summary>
    public const uint UnarmedAttackTime = 2000;

    /// <summary>
    /// SpellShapeshiftForm.dbc's attack speed, which replaces the weapon's in the main and off hand.
    /// Of 3.3.5a's forms only cat, bear and dire bear have one; 0 for the others.
    /// </summary>
    public static uint FormAttackTime(byte form) => form switch
    {
        1 => 1000,      // Cat Form
        5 or 8 => 2500, // Bear Form, Dire Bear Form
        _ => 0,
    };
}
