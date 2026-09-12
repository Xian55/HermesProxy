using HermesProxy.Enums;
using HermesProxy.World.Dispatch;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server.Systems;

/// <summary>
/// Translation for the modern client's toy box and item-collection CMSGs.
/// </summary>
/// <remarks>
/// The toy box does not exist on 3.3.5a at all, so none of this is forwarding — the proxy keeps the
/// learned and favourite toy sets itself, in account metadata, and synthesises the collection the
/// client expects. Every handler is gated on V3_4_3 because that is the only modern build with a
/// toy box to serve.
/// <para>
/// <see cref="HandleUseToy"/> is the one that reaches the server, and it has two routes: a toy still
/// sitting in a bag casts through the normal item path, while a toy the character has learned but no
/// longer carries has to be cast bare, because native 3.4.3 sends no bag item and 3.3.5a only
/// accepts that for a spell already in the spellbook.
/// </para>
/// </remarks>
public static class ToySystem
{
    [HandlesCmsg(Opcode.CMSG_ADD_TOY)]
    public static void HandleAddToy(in AddToy add, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        uint itemId = ctx.GetSession().GameState.GetItemId(add.Guid);
        if (itemId == 0)
            return;

        var session = ctx.GetSession();
        var favorites = Systems.BattlePetSystem.EnsureCollectionFavorites(ctx.GetSession());
        bool firstLearn = favorites.LearnedToys.Add(itemId);
        if (firstLearn)
            session.AccountMetaDataMgr.SaveCollectionFavorites(favorites);

        if (firstLearn)
            CollectionSync.PlayToyLearnVisual(session);

        CollectionSync.SendToys(session);
        ctx.SendPacket(AccountToyUpdate.FromSession(session.GameState));
    }

    [HandlesCmsg(Opcode.CMSG_USE_TOY)]
    public static void HandleUseToy(in UseToy use, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;

        uint itemId = use.ItemId;
        if (itemId == 0)
            return;

        var found = ctx.GetSession().GameState.FindItemInInventoryById(itemId);
        if (found == null)
        {
            // Native 3.4.3 Use Toy casts the item spell with no bag item. 3.3.5a
            // only accepts that when the spell is already in this character's
            // spellbook (HasActiveSpell). Otherwise AC silently drops CMSG_CAST_SPELL.
            if (TryCastToyWithoutItem(in ctx, use, itemId))
                return;
            RejectToyUse(in ctx, use, SpellCastResultV343.ItemNotFound);
            return;
        }

        var template = GameData.GetItemTemplate(itemId);
        bool equipped = found.Value.containerSlot == ItemConst.NullSlot
            && found.Value.slot < EquipmentSlot.End;
        if (template != null && template.InventoryType != 0 && !equipped)
        {
            RejectToyUse(in ctx, use, SpellCastResultV343.EquippedItem);
            return;
        }

        Systems.SpellSystem.UseInventoryItem(in ctx, found.Value.guid, found.Value.containerSlot, found.Value.slot, use.Cast);
    }

    static bool TryCastToyWithoutItem(in SessionContext ctx, in UseToy use, uint itemId)
    {
        uint serverSpellId = use.Cast.SpellID;
        if (GameData.TryGetItemOnUseSpellId(itemId, out uint onUse) && onUse != 0)
            serverSpellId = onUse;

        var known = ctx.GetSession().GameState.KnownSpells;
        if (!known.Contains(serverSpellId) && !known.Contains(use.Cast.SpellID))
            return false;

        Systems.SpellSystem.ForwardKnownSpellCast(in ctx, use.Cast, serverSpellId);
        return true;
    }

    static void RejectToyUse(in SessionContext ctx, in UseToy use, SpellCastResultV343 reason)
    {
        uint mapId = (uint)(ctx.GetSession().GameState.CurrentMapId ?? 0);
        var serverCastId = WowGuid128.Create(
            HighGuidType703.Cast,
            SpellCastSource.Normal,
            mapId,
            use.Cast.SpellID,
            use.Cast.SpellID + ctx.GetSession().GameState.CurrentPlayerGuid.GetCounter());
        ctx.SendPacket(new SpellPrepare { ClientCastID = use.Cast.CastID, ServerCastID = serverCastId });
        ctx.SendPacket(new CastFailed
        {
            SpellID = use.Cast.SpellID,
            SpellXSpellVisualID = use.Cast.SpellXSpellVisualID,
            Reason = (uint)reason,
            CastID = serverCastId,
        });
    }

    [HandlesCmsg(Opcode.CMSG_COLLECTION_ITEM_SET_FAVORITE)]
    public static void HandleCollectionItemSetFavorite(in CollectionItemSetFavorite setFavorite, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;
        if (setFavorite.Type != ItemCollectionType.Toy || setFavorite.ID == 0)
            return;

        var favorites = Systems.BattlePetSystem.EnsureCollectionFavorites(ctx.GetSession());
        if (setFavorite.IsFavorite)
            favorites.FavoriteToys.Add(setFavorite.ID);
        else
            favorites.FavoriteToys.Remove(setFavorite.ID);
        ctx.GetSession().AccountMetaDataMgr.SaveCollectionFavorites(favorites);
        ctx.SendPacket(AccountToyUpdate.FromSession(ctx.GetSession().GameState));
    }

    [HandlesCmsg(Opcode.CMSG_TOY_CLEAR_FANFARE)]
    public static void HandleToyClearFanfare(in ToyClearFanfare _, in SessionContext ctx)
    {
        if (ModernVersion.Build != ClientVersionBuild.V3_4_3_54261)
            return;
    }
}
