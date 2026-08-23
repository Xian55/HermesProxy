using System;
using System.IO;
using System.Runtime.CompilerServices;
using System.Text;
using System.Text.Json;
using HermesProxy;
using HermesProxy.World.Enums;
using HermesProxy.World.Server;
using Xunit;

namespace HermesProxy.Tests.World.Server;

/// <summary>
/// Regression coverage for the enter-world crash: the modern client opens the instance
/// socket on its own thread and touches CurrentPlayerStorage while HandlePlayerLogin is
/// still loading it, so a half-built PlayerSettings must never become visible.
/// </summary>
public class CurrentPlayerStorageTests : IDisposable
{
    private readonly string _accountName = "HermesTests_" + Guid.NewGuid().ToString("N");
    private readonly string _realmName = "TestRealm";
    private readonly string _charName = "Testchar";

    private string CharDirectory =>
        Path.GetFullPath(Path.Combine("AccountData", _accountName, _realmName, _charName));

    private string SettingsPath => Path.Combine(CharDirectory, "settings.json");

    private GlobalSessionData CreateSession()
    {
        var session = (GlobalSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GlobalSessionData));
        var gameState = (GameSessionData)RuntimeHelpers.GetUninitializedObject(typeof(GameSessionData));

        var realm = new Realm();
        realm.SetName(_realmName);

        gameState.CurrentPlayerInfo = new OwnCharacterInfo { Name = _charName, Realm = realm };
        session.GameState = gameState;
        session.AccountMetaDataMgr = new AccountMetaDataManager(_accountName);
        return session;
    }

    public void Dispose()
    {
        var accountDir = Path.GetFullPath(Path.Combine("AccountData", _accountName));
        if (Directory.Exists(accountDir))
            Directory.Delete(accountDir, recursive: true);
    }

    [Fact]
    public void PlayerSettings_BeforeReload_DoesNotThrow()
    {
        var settings = new PlayerSettings(CreateSession());

        // _internalStorage must carry a usable default; before the fix it was null! and
        // every one of these reads was a NullReferenceException.
        Assert.Null(settings.MultiActionBarsMask);

        PlayerFlags flags = PlayerFlags.None;
        settings.PatchFlags(ref flags);
        Assert.Equal(PlayerFlags.None, flags & PlayerFlags.AutoDeclineGuild);
    }

    [Fact]
    public void CurrentPlayerStorage_BeforeLoad_ExposesNoHalfBuiltState()
    {
        var storage = new CurrentPlayerStorage(CreateSession());

        // Callers use `Settings?.` precisely because this is null until a player is loaded.
        Assert.Null(storage.Settings);
        Assert.Null(storage.CompletedQuests);
    }

    [Fact]
    public void LoadCurrentPlayer_PublishesFullyLoadedInstances()
    {
        var storage = new CurrentPlayerStorage(CreateSession());
        storage.LoadCurrentPlayer();

        Assert.NotNull(storage.Settings);
        Assert.NotNull(storage.CompletedQuests);
        Assert.Null(storage.Settings.MultiActionBarsMask);
        Assert.True(File.Exists(SettingsPath));
    }

    [Fact]
    public void LoadCurrentPlayer_RoundTripsPersistedSettings()
    {
        var session = CreateSession();
        var storage = new CurrentPlayerStorage(session);
        storage.LoadCurrentPlayer();
        storage.Settings.SetMultiActionBarsMask(0x0B);

        var reloaded = new CurrentPlayerStorage(session);
        reloaded.LoadCurrentPlayer();

        Assert.Equal((byte)0x0B, reloaded.Settings.MultiActionBarsMask);
    }

    [Fact]
    public void LoadCurrentPlayer_WhenLoadFails_LeavesPreviousStateIntact()
    {
        var storage = new CurrentPlayerStorage(CreateSession());
        storage.LoadCurrentPlayer();
        storage.Settings.SetMultiActionBarsMask(0x05);

        PlayerSettings loaded = storage.Settings;
        CompletedQuestTracker loadedQuests = storage.CompletedQuests;

        // Corrupt the backing file so the next Reload throws partway through.
        File.WriteAllText(SettingsPath, "{ this is not json", Encoding.UTF8);

        Assert.ThrowsAny<JsonException>(() => storage.LoadCurrentPlayer());

        // The failed load must not have published anything. Before the fix, the new
        // half-built PlayerSettings was already visible with a null _internalStorage.
        Assert.Same(loaded, storage.Settings);
        Assert.Same(loadedQuests, storage.CompletedQuests);
        Assert.Equal((byte)0x05, storage.Settings.MultiActionBarsMask);
    }
}
