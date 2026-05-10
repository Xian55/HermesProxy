using System.Collections.Generic;
using HermesProxy.World.Enums;
using HermesProxy.World.Server.Packets;

namespace HermesProxy.World.Server;

public class CurrentPlayerStorage
{
    private readonly GlobalSessionData _globalSession;
    public CompletedQuestTracker CompletedQuests { get; private set; } = null!;
    public PlayerSettings Settings { get; private set; } = null!;

    public CurrentPlayerStorage(GlobalSessionData globalSession)
    {
        _globalSession = globalSession;
    }

    public void LoadCurrentPlayer()
    {
        CompletedQuests = new CompletedQuestTracker(_globalSession);
        Settings = new PlayerSettings(_globalSession);
        CompletedQuests.Reload();
        Settings.Reload();
    }
}

public class PlayerSettings
{
    private InternalStorage _internalStorage = null!;
    private PlayerFlags _lastCapturedFlags;

    public bool NeedToForcePatchFlags { get; private set; }


    public GlobalSessionData Session { get; }

    public PlayerSettings(GlobalSessionData globalSession)
    {
        Session = globalSession;
    }

    public void SetAutoBlockGuildInvites(bool value)
    {
        _internalStorage.AutoBlockGuildInvites = value;
        NeedToForcePatchFlags = true;
        Save();
    }

    // V3_4_3 modern client never reads the legacy MultiActionBars descriptor field
    // and immediately overwrites the legacy DB with mask=0 a few seconds after every
    // login. To make Action Bar 2/3/4/5 checkboxes sticky, the proxy persists the last
    // user-intended mask here and re-injects matching CVars (bottomLeftActionBar, etc.)
    // via an unsolicited SMSG_UPDATE_ACCOUNT_DATA(type=0) on the next login.
    public byte? MultiActionBarsMask => _internalStorage.MultiActionBarsMask;

    public void SetMultiActionBarsMask(byte mask)
    {
        if (_internalStorage.MultiActionBarsMask == mask)
            return;
        _internalStorage.MultiActionBarsMask = mask;
        Save();
    }

    public void PatchFlags(ref PlayerFlags flags)
    {
        _lastCapturedFlags = flags;
        NeedToForcePatchFlags = false;

        if (_internalStorage.AutoBlockGuildInvites)
            flags |= PlayerFlags.AutoDeclineGuild;
        else
            flags &= ~(PlayerFlags.AutoDeclineGuild);
    }

    public PlayerFlags CreateNewFlags()
    {
        var flags = _lastCapturedFlags;
        PatchFlags(ref flags);
        return flags;
    }

    private void Save()
    {
        Session.AccountMetaDataMgr.SaveCharacterSettingsStorage(Session.GameState.CurrentPlayerInfo!.Realm.Name, Session.GameState.CurrentPlayerInfo!.Name!, _internalStorage);
    }
    
    public class InternalStorage
    {
        // A JSON encoder / decoder is used to store the settings
        // Make use of a public { get; set; } Property so that the JSON serializer can change it

        // The player can request a change in the Interface settings
        // but the actual value has to be reflected in the local CharacterFlags
        public bool AutoBlockGuildInvites { get; set; }

        // V3_4_3 action bar visibility checkbox state (bit 0=Action Bar 2,
        // bit 1=Action Bar 3, bit 2=Action Bar 4, bit 3=Action Bar 5).
        // Null = never set; mask is replayed via synthesised account-data CVars
        // on next login because the V3_4_3 client overwrites the legacy
        // MultiActionBars descriptor with 0 right after every login.
        public byte? MultiActionBarsMask { get; set; }
    }

    public void Reload()
    {
        _internalStorage = Session.AccountMetaDataMgr.LoadCharacterSettingsStorage(Session.GameState.CurrentPlayerInfo!.Realm.Name, Session.GameState.CurrentPlayerInfo!.Name!);
    }
}

public class CompletedQuestTracker
{
    private Dictionary<int, ulong> _cachedQuestCompleted = new();

    public GlobalSessionData Session { get; }

    public CompletedQuestTracker(GlobalSessionData globalSession)
    {
        Session = globalSession;
    }

    public void MarkQuestAsNotCompleted(uint questQuestId)
    {
        Session.AccountMetaDataMgr.MarkQuestAsNotCompleted(Session.GameState.CurrentPlayerInfo!.Realm.Name, Session.GameState.CurrentPlayerInfo!.Name!, questQuestId);

        var questBit = GameData.GetUniqueQuestBit(questQuestId);
        if (questBit.HasValue)
        {
            SendSingleUpdateToClient(questBit.Value, false);
        }
    }

    public void MarkQuestAsCompleted(uint questQuestId)
    {
        Session.AccountMetaDataMgr.MarkQuestAsCompleted(Session.GameState.CurrentPlayerInfo!.Realm.Name, Session.GameState.CurrentPlayerInfo!.Name!, questQuestId);

        var questBit = GameData.GetUniqueQuestBit(questQuestId);
        if (questBit.HasValue)
        {
            SendSingleUpdateToClient(questBit.Value, true);
        }
    }

    public void Reload()
    {
        var questIds = Session.AccountMetaDataMgr.GetAllCompletedQuests(Session.GameState.CurrentPlayerInfo!.Realm.Name, Session.GameState.CurrentPlayerInfo!.Name!);

        _cachedQuestCompleted = new Dictionary<int, ulong>();
        foreach (uint questId in questIds)
        {
            uint? questBit = GameData.GetUniqueQuestBit(questId);
            if (!questBit.HasValue)
                continue;

            int idx = (int)(((questBit - 1) >> 6));
            int bitIdx = (int)((questBit - 1) & 63);
            _cachedQuestCompleted.TryAdd(idx, 0);
            _cachedQuestCompleted[idx] |= ((ulong)1) << bitIdx;
        }
    }
    
    private void SendSingleUpdateToClient(uint questBit, bool isSet)
    {
        int idx = (int)(((questBit - 1) >> 6));
        int bitIdx = (int)((questBit - 1) & 63);
        _cachedQuestCompleted.TryAdd(idx, 0);
        if (isSet)
            _cachedQuestCompleted[idx] |= ((ulong)1) << bitIdx;
        else
            _cachedQuestCompleted[idx] &= ~(((ulong)1) << bitIdx);
        
        ObjectUpdate updateData = new ObjectUpdate(Session.GameState.CurrentPlayerGuid, UpdateTypeModern.Values, Session);
        updateData.ActivePlayerData.QuestCompleted[idx] = _cachedQuestCompleted[idx];

        UpdateObject updatePacket = new UpdateObject(Session.GameState);
        updatePacket.ObjectUpdates.Add(updateData);
        Session.WorldClient!.SendPacketToClient(updatePacket);
    }

    public void WriteAllCompletedIntoArray(ulong?[] dest)
    {
        foreach (var kv in _cachedQuestCompleted)
        {
            dest[kv.Key] = kv.Value;
        }
    }
}
