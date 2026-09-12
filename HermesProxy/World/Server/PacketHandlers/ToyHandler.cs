using HermesProxy.Enums;
using HermesProxy.World;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public partial class WorldSocket
{
    [PacketHandler(Opcode.CMSG_ADD_TOY)]
    void HandleAddToy(AddToy add)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        uint itemId = GetSession().GameState.GetItemId(add.Guid);
        if (itemId == 0)
            return;

        var session = GetSession();
        var favorites = Systems.BattlePetSystem.EnsureCollectionFavorites(GetSession());
        bool firstLearn = favorites.LearnedToys.Add(itemId);
        if (firstLearn)
            session.AccountMetaDataMgr.SaveCollectionFavorites(favorites);

        if (firstLearn)
            CollectionSync.PlayToyLearnVisual(session);

        CollectionSync.SendToys(session);
        SendPacket(AccountToyUpdate.FromSession(session.GameState));
    }

    [PacketHandler(Opcode.CMSG_USE_TOY)]
    void HandleUseToy(UseToy use)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        uint itemId = use.ItemId;
        if (itemId == 0)
            return;

        var found = GetSession().GameState.FindItemInInventoryById(itemId);
        if (found == null)
        {
            // Native 3.4.3 Use Toy casts the item spell with no bag item. 3.3.5a
            // only accepts that when the spell is already in this character's
            // spellbook (HasActiveSpell). Otherwise AC silently drops CMSG_CAST_SPELL.
            if (TryCastToyWithoutItem(use, itemId))
                return;
            RejectToyUse(use, SpellCastResultV343.ItemNotFound);
            return;
        }

        var template = GameData.GetItemTemplate(itemId);
        bool equipped = found.Value.containerSlot == ItemConst.NullSlot
            && found.Value.slot < EquipmentSlot.End;
        if (template != null && template.InventoryType != 0 && !equipped)
        {
            RejectToyUse(use, SpellCastResultV343.EquippedItem);
            return;
        }

        UseInventoryItem(found.Value.guid, found.Value.containerSlot, found.Value.slot, use.Cast);
    }

    bool TryCastToyWithoutItem(UseToy use, uint itemId)
    {
        uint serverSpellId = use.Cast.SpellID;
        if (GameData.TryGetItemOnUseSpellId(itemId, out uint onUse) && onUse != 0)
            serverSpellId = onUse;

        var known = GetSession().GameState.KnownSpells;
        if (!known.Contains(serverSpellId) && !known.Contains(use.Cast.SpellID))
            return false;

        ForwardKnownSpellCast(use.Cast, serverSpellId);
        return true;
    }

    void RejectToyUse(UseToy use, SpellCastResultV343 reason)
    {
        uint mapId = (uint)(GetSession().GameState.CurrentMapId ?? 0);
        var serverCastId = WowGuid128.Create(
            HighGuidType703.Cast,
            SpellCastSource.Normal,
            mapId,
            use.Cast.SpellID,
            use.Cast.SpellID + GetSession().GameState.CurrentPlayerGuid.GetCounter());
        SendPacket(new SpellPrepare { ClientCastID = use.Cast.CastID, ServerCastID = serverCastId });
        SendPacket(new CastFailed
        {
            SpellID = use.Cast.SpellID,
            SpellXSpellVisualID = use.Cast.SpellXSpellVisualID,
            Reason = (uint)reason,
            CastID = serverCastId,
        });
    }

    [PacketHandler(Opcode.CMSG_COLLECTION_ITEM_SET_FAVORITE)]
    void HandleCollectionItemSetFavorite(CollectionItemSetFavorite setFavorite)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;
        if (setFavorite.Type != ItemCollectionType.Toy || setFavorite.ID == 0)
            return;

        var favorites = Systems.BattlePetSystem.EnsureCollectionFavorites(GetSession());
        if (setFavorite.IsFavorite)
            favorites.FavoriteToys.Add(setFavorite.ID);
        else
            favorites.FavoriteToys.Remove(setFavorite.ID);
        GetSession().AccountMetaDataMgr.SaveCollectionFavorites(favorites);
        SendPacket(AccountToyUpdate.FromSession(GetSession().GameState));
    }

    [PacketHandler(Opcode.CMSG_TOY_CLEAR_FANFARE)]
    void HandleToyClearFanfare(ToyClearFanfare _)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;
    }
}
