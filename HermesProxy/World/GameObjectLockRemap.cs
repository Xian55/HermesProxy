using HermesProxy.World.Enums;

namespace HermesProxy.World;

/// <summary>
/// Corrects the OPEN_LOCK spell a modern client picks for a GameObject whose Lock row means
/// something different on the legacy server.
///
/// The client chooses which "Opening" spell to cast from its <em>own</em> Lock.db2: it reads
/// the GameObject's lock id, takes the LOCK_KEY_SKILL entry's lock type, and casts the spell
/// whose SPELL_EFFECT_OPEN_LOCK EffectMiscValue matches. The legacy server then re-runs that
/// check against its own Lock.dbc in Spell::CanOpenLock — and if no case matches it leaves
/// reqKey set and answers SPELL_FAILED_BAD_TARGETS, which the client renders as
/// "Invalid target".
///
/// Diffing all 388 shared rows of 3.3.5a Lock.dbc against 3.4.3 Lock.db2 turns up 8 that
/// drift, but only row 99 changes a <em>skill</em> requirement — 14 (Open Attacking) on
/// 3.3.5a versus 5 (Open) on 3.4.3. Row 1748 also drifts, harmlessly: the legacy row still
/// contains the modern type in a later slot, and CanOpenLock scans all eight. So one
/// substitution covers the whole class of failure. Issue #269.
///
/// Deliberately V3_4_3-only. Row 99 reads 14 in both vanilla 1.12.1 and the 1.14.2 client,
/// and 5 in both TBC 2.4.3 and the 2.5.4 client — those pairs agree and must not be
/// rewritten. It was WotLK that moved the row to 14 and the 3.4.3 rebuild that put it back
/// to 5, which is why only this one build combination breaks.
/// </summary>
public static class GameObjectLockRemap
{
    /// <summary>
    /// Lock.dbc row whose LOCK_KEY_SKILL type is 14 (Open Attacking) on 3.3.5a but 5 (Open)
    /// on 3.4.3. Used by 41 GameObjects on a stock WotLK world DB — Statue of Queen Azshara,
    /// Gong of Zul'Farrak, Egg of Onyxia, Stone of Binding, Resonite Crystal and friends.
    /// </summary>
    private const uint OpenAttackingLockId = 99;

    /// <summary>
    /// The legacy spell id to send in place of <paramref name="modernSpellId"/>, or 0 when the
    /// cast should be forwarded unchanged.
    /// </summary>
    /// <param name="modernSpellId">Spell the modern client asked to cast.</param>
    /// <param name="legacyLockId">
    /// Lock id from the legacy server's own gameobject_template, or 0 when the target has no
    /// lock or its template has not been seen yet.
    /// </param>
    public static uint ResolveLegacyOpenLockSpell(uint modernSpellId, uint legacyLockId)
    {
        if (legacyLockId != OpenAttackingLockId)
            return 0;

        // The client picked "Opening" (EffectMiscValue 5) because its own Lock.db2 row 99
        // says Open. The legacy row says Open Attacking, so the server only accepts
        // "Attacking" (EffectMiscValue 14).
        if (modernSpellId != KnownSpellIds.Opening)
            return 0;

        return KnownSpellIds.OpeningAttacking;
    }
}
