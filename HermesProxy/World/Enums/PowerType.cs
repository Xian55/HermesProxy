using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums;

public enum PowerType : sbyte
{
    Invalid                       = -1,
    Mana                          = 0,            // UNIT_FIELD_POWER1
    Rage                          = 1,            // UNIT_FIELD_POWER2
    Focus                         = 2,            // UNIT_FIELD_POWER3
    Energy                        = 3,            // UNIT_FIELD_POWER4
    Happiness                     = 4,            // UNIT_FIELD_POWER5
    Rune                          = 5,            // UNIT_FIELD_POWER6
    RunicPower                    = 6,            // UNIT_FIELD_POWER7
    ComboPoints                   = 14,           // not real, so we know to set PLAYER_FIELD_BYTES,1
    // Per-rune-type power slots used by V3_4_3 retail (TC-wotlk_classic Powers enum).
    // The V3_4_3 client expects SMSG_SPELL_GO RemainingPower entries with these types
    // to trigger the per-rune cooldown swirl after a rune-cost cast.
    RuneBlood                     = 20,
    RuneFrost                     = 21,
    RuneUnholy                    = 22,
};
