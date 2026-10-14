using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Objects;

public static class ClassPowerTypes
{
    // ChrClassesXPowerTypes.db2
    public static sbyte GetPowerSlotForClass(Class classId, PowerType power)
    {
        // 4.4.2's table gives a hunter focus where the older clients have mana, and each Cataclysm
        // class power a slot of its own; for the rest it is the older clients' table. Without
        // these the value had no slot and the bar stayed at 0.
        if (ModernVersion.AddedInVersion(4, 4, 0))
        {
            switch (classId, power)
            {
                case (Class.Hunter, PowerType.Focus):
                    return 0;
                case (Class.Paladin, PowerType.HolyPower):
                case (Class.Warlock, PowerType.SoulShards):
                    return 1;
                case (Class.Druid, PowerType.Eclipse):
                    return 4;
            }
        }

        switch (classId)
        {
            case Class.Warrior:
            {
                switch (power)
                {
                    case PowerType.Rage:
                        return 0;
                    case PowerType.ComboPoints:
                        return 1;
                }
                break;
            }
            case Class.Paladin:
            {
                switch (power)
                {
                    case PowerType.Mana:
                        return 0;
                }
                break;
            }
            case Class.Hunter:
            {
                switch (power)
                {
                    case PowerType.Mana:
                        return 0;
                }
                break;
            }
            case Class.Rogue:
            {
                switch (power)
                {
                    case PowerType.Energy:
                        return 0;
                    case PowerType.ComboPoints:
                        return 1;
                }
                break;
            }
            case Class.Priest:
            {
                switch (power)
                {
                    case PowerType.Mana:
                        return 0;
                }
                break;
            }
            case Class.Deathknight:
            {
                switch (power)
                {
                    case PowerType.RunicPower:
                        return 0;
                }
                break;
            }
            case Class.Shaman:
            {
                switch (power)
                {
                    case PowerType.Mana:
                        return 0;
                }
                break;
            }
            case Class.Mage:
            {
                switch (power)
                {
                    case PowerType.Mana:
                        return 0;
                }
                break;
            }
            case Class.Warlock:
            {
                switch (power)
                {
                    case PowerType.Mana:
                        return 0;
                }
                break;
            }
            case Class.Druid:
            {
                switch (power)
                {
                    case PowerType.Mana:
                        return 0;
                    case PowerType.Rage:
                        return 1;
                    case PowerType.Energy:
                        return 2;
                    case PowerType.ComboPoints:
                        return 3;
                }
                break;
            }
        }

        return -1;
    }
    public static sbyte GetPowerSlotForPet(PowerType power)
    {
        switch (power)
        {
            case PowerType.Focus:
                return 0;
            case PowerType.Happiness:
                return 3;
        }

        return -1;
    }
}
