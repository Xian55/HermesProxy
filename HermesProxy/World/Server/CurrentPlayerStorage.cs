using System.Collections.Generic;
using HermesProxy.World.Enums;
using HermesProxy.World.Objects;
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

    // Build fully-loaded instances into locals and only then publish them. The modern
    // client opens the instance socket on its own network thread while this runs, and
    // its first packets (CMSG_SET_ACTION_BAR_TOGGLES, guild settings) touch these
    // objects, and publishing a half-Reloaded instance is a live NullReferenceException.
    public void LoadCurrentPlayer()
    {
        var quests = new CompletedQuestTracker(_globalSession);
        var settings = new PlayerSettings(_globalSession);
        quests.Reload();
        settings.Reload();

        CompletedQuests = quests;
        Settings = settings;
    }
}

public class PlayerSettings
{
    private InternalStorage _internalStorage = new();
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
        public uint LastSummonedPetSpecies { get; set; }
    }

    public uint LastSummonedPetSpecies => _internalStorage.LastSummonedPetSpecies;

    public void SetLastSummonedPetSpecies(uint speciesId)
    {
        if (_internalStorage.LastSummonedPetSpecies == speciesId)
            return;
        _internalStorage.LastSummonedPetSpecies = speciesId;
        Save();
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
        updateData.EnsureActivePlayerData().EnsureQuestCompleted()[idx] = _cachedQuestCompleted[idx];

        UpdateObject updatePacket = new UpdateObject(Session.GameState);
        updatePacket.ObjectUpdates.Add(updateData);
        Session.WorldClient!.SendPacketToClient(updatePacket);
    }

    /// <summary>
    /// Stamps the whole set onto a login update. Takes the owner block rather than its array so a
    /// character with nothing completed never materialises the 14 KB <c>QuestCompleted[875]</c> —
    /// which is every character on a backend that cannot report quest history in the first place.
    /// </summary>
    public void WriteAllCompletedIntoArray(ActivePlayerData dest)
    {
        if (_cachedQuestCompleted.Count == 0)
            return;

        ulong?[] questCompleted = dest.EnsureQuestCompleted();
        foreach (var kv in _cachedQuestCompleted)
        {
            questCompleted[kv.Key] = kv.Value;
        }
    }
}
