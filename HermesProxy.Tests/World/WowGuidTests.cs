using System;
using HermesProxy.World;
using HermesProxy.World.Enums;
using Moq;
using Xunit;

namespace HermesProxy.Tests.World
{
    /// <summary>
    /// Test helper utilities for WowGuid tests
    /// </summary>
    public static class WowGuidTestHelper
    {
        /// <summary>
        /// Creates a mock GameSessionData for testing purposes
        /// </summary>
        public static GameSessionData CreateMockGameSessionData()
        {
            // Create via reflection since constructor is private
            var gameSessionType = typeof(GameSessionData);
            var instance = System.Runtime.Serialization.FormatterServices.GetUninitializedObject(gameSessionType);
            return (GameSessionData)instance;
        }
    }

    public class WowGuid64Tests
    {
        [Fact]
        public void Constructor_WithZeroId_CreatesEmptyGuid()
        {
            // Arrange & Act
            var guid = new WowGuid64(0);

            // Assert
            Assert.Equal(0ul, guid.Low);
            Assert.True(guid.IsEmpty());
        }

        [Fact]
        public void Constructor_WithHighGuidTypeAndCounter()
        {
            // Arrange
            var highGuidType = HighGuidTypeLegacy.Creature;
            uint counter = 12345;

            // Act
            var guid = new WowGuid64(highGuidType, counter);

            // Assert
            Assert.NotEqual(0ul, guid.Low);
            Assert.Equal(counter, guid.GetCounter());
        }

        [Fact]
        public void Constructor_WithHighGuidTypeEntryAndCounter()
        {
            // Arrange
            var highGuidType = HighGuidTypeLegacy.Creature;
            uint entry = 1234;
            uint counter = 5678;

            // Act
            var guid = new WowGuid64(highGuidType, entry, counter);

            // Assert
            Assert.NotEqual(0ul, guid.Low);
            Assert.Equal(entry, guid.GetEntry());
            Assert.Equal(counter, guid.GetCounter());
        }

        [Fact]
        public void GetHighGuidTypeLegacy_WithZeroLow_ReturnsNone()
        {
            // Arrange
            var guid = new WowGuid64(0);

            // Act
            var highGuidType = guid.GetHighGuidTypeLegacy();

            // Assert
            Assert.Equal(HighGuidTypeLegacy.None, highGuidType);
        }

        [Fact]
        public void HasEntry_WithDifferentTypes()
        {
            // Assert
            // Types that have entries
            Assert.True(new WowGuid64(HighGuidTypeLegacy.Creature, 1, 1).HasEntry());
            Assert.True(new WowGuid64(HighGuidTypeLegacy.GameObject, 2, 2).HasEntry());
            Assert.True(new WowGuid64(HighGuidTypeLegacy.Pet, 3, 3).HasEntry());
            Assert.True(new WowGuid64(HighGuidTypeLegacy.Vehicle, 4, 4).HasEntry());
            Assert.True(new WowGuid64(HighGuidTypeLegacy.Transport, 5, 5).HasEntry());

            // Types without entries
            Assert.False(new WowGuid64(HighGuidTypeLegacy.Player, 1).HasEntry());
            Assert.False(new WowGuid64(HighGuidTypeLegacy.Item, 1).HasEntry());
        }

        [Fact]
        public void GetHighType_ReturnsExpectedType()
        {
            // Arrange
            var guid64 = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act
            var highType = guid64.GetHighType();

            // Assert
            Assert.Equal(HighGuidType.Creature, highType);
        }

        [Fact]
        public void IsPlayer_WithPlayerGuid_ReturnsTrue()
        {
            // Arrange
            var playerGuid = new WowGuid64(HighGuidTypeLegacy.Player, 1);

            // Act
            var result = playerGuid.IsPlayer();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPlayer_WithNonPlayerGuid_ReturnsFalse()
        {
            // Arrange
            var creatureGuid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act
            var result = creatureGuid.IsPlayer();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsCreature_WithCreatureGuid_ReturnsTrue()
        {
            // Arrange
            var creatureGuid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act
            var result = creatureGuid.IsCreature();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsCreature_WithNonCreatureGuid_ReturnsFalse()
        {
            // Arrange
            var playerGuid = new WowGuid64(HighGuidTypeLegacy.Player, 1);

            // Act
            var result = playerGuid.IsCreature();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsItem_WithItemGuid_ReturnsTrue()
        {
            // Arrange
            var itemGuid = new WowGuid64(HighGuidTypeLegacy.Item, 1);

            // Act
            var result = itemGuid.IsItem();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsItem_WithNonItemGuid_ReturnsFalse()
        {
            // Arrange
            var playerGuid = new WowGuid64(HighGuidTypeLegacy.Player, 1);

            // Act
            var result = playerGuid.IsItem();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetObjectType_WithPlayerGuid_ReturnsPlayer()
        {
            // Arrange
            var playerGuid = new WowGuid64(HighGuidTypeLegacy.Player, 1);

            // Act
            var objectType = playerGuid.GetObjectType();

            // Assert
            Assert.Equal(ObjectType.Player, objectType);
        }

        [Fact]
        public void GetObjectType_WithCreatureGuid_ReturnsUnit()
        {
            // Arrange
            var creatureGuid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act
            var objectType = creatureGuid.GetObjectType();

            // Assert
            Assert.Equal(ObjectType.Unit, objectType);
        }

        [Fact]
        public void GetObjectType_WithItemGuid_ReturnsItem()
        {
            // Arrange
            var itemGuid = new WowGuid64(HighGuidTypeLegacy.Item, 1);

            // Act
            var objectType = itemGuid.GetObjectType();

            // Assert
            Assert.Equal(ObjectType.Item, objectType);
        }

        [Fact]
        public void IsWorldObject_WithWorldObjectTypes_ReturnsTrue()
        {
            // Assert
            Assert.True(new WowGuid64(HighGuidTypeLegacy.Player, 1).IsWorldObject());
            Assert.True(new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1).IsWorldObject());
            Assert.True(new WowGuid64(HighGuidTypeLegacy.GameObject, 200, 1).IsWorldObject());
        }

        [Fact]
        public void IsWorldObject_WithNonWorldObjectTypes_ReturnsFalse()
        {
            // Assert
            Assert.False(new WowGuid64(HighGuidTypeLegacy.Item, 1).IsWorldObject());
        }

        [Fact]
        public void IsTransport_WithTransportType_ReturnsTrue()
        {
            // Arrange
            var transportGuid = new WowGuid64(HighGuidTypeLegacy.Transport, 1, 1);

            // Act
            var result = transportGuid.IsTransport();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsTransport_WithNonTransportType_ReturnsFalse()
        {
            // Arrange
            var creatureGuid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act
            var result = creatureGuid.IsTransport();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void Equality_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var guid1 = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);
            var guid2 = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act & Assert
            Assert.Equal(guid1, guid2);
            Assert.True(guid1 == guid2);
            Assert.False(guid1 != guid2);
        }

        [Fact]
        public void Equality_WithDifferentValues_ReturnsFalse()
        {
            // Arrange
            var guid1 = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);
            var guid2 = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 2);

            // Act & Assert
            Assert.NotEqual(guid1, guid2);
            Assert.True(guid1 != guid2);
            Assert.False(guid1 == guid2);
        }

        [Fact]
        public void GetHashCode_WithSameValues_ReturnsSameHash()
        {
            // Arrange
            var guid1 = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);
            var guid2 = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act & Assert
            Assert.Equal(guid1.GetHashCode(), guid2.GetHashCode());
        }

        [Fact]
        public void IsEmpty_WithZeroGuid_ReturnsTrue()
        {
            // Arrange
            var guid = WowGuid64.Empty;

            // Act & Assert
            Assert.True(guid.IsEmpty());
        }

        [Fact]
        public void IsEmpty_WithNonZeroGuid_ReturnsFalse()
        {
            // Arrange
            var guid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act & Assert
            Assert.False(guid.IsEmpty());
        }

        [Fact]
        public void ToString_WithEmptyGuid_ReturnsRecordFormat()
        {
            // Arrange
            var guid = WowGuid64.Empty;

            // Act
            var result = guid.ToString();

            // Assert
            // Record struct uses default ToString format
            Assert.Contains("Low = 0", result);
        }

        [Fact]
        public void ToString_WithValidGuid_ContainsLowValue()
        {
            // Arrange
            var guid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act
            var result = guid.ToString();

            // Assert
            // Record struct uses default ToString format containing property names
            Assert.Contains("Low =", result);
            Assert.NotEqual("WowGuid64 { Low = 0 }", result);
        }

        [Fact]
        public void To64_ReturnsSelf()
        {
            // Arrange
            var guid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act
            var result = guid.To64();

            // Assert
            Assert.Equal(guid, result);
        }

        [Fact]
        public void StaticEmpty_ReturnsZeroGuid()
        {
            // Act & Assert
            Assert.True(WowGuid64.Empty.IsEmpty());
            Assert.Equal(0ul, WowGuid64.Empty.Low);
        }

        [Fact]
        public void GetLowValue_ReturnsLow()
        {
            // Arrange
            var guid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act
            var lowValue = guid.GetLowValue();

            // Assert
            Assert.Equal(guid.Low, lowValue);
        }

        [Fact]
        public void GetHighValue_ReturnsZero()
        {
            // Arrange
            var guid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 1);

            // Act
            var highValue = guid.GetHighValue();

            // Assert
            // WowGuid64 has no High value, always returns 0
            Assert.Equal(0ul, highValue);
        }

        [Fact]
        public void Constructor_WithZeroCounter_CreatesEmptyGuid()
        {
            // Arrange & Act
            var guid = new WowGuid64(HighGuidTypeLegacy.Creature, 0);

            // Assert
            Assert.Equal(0ul, guid.Low);
            Assert.True(guid.IsEmpty());
        }

        [Fact]
        public void Constructor_WithZeroCounterAndEntry_CreatesEmptyGuid()
        {
            // Arrange & Act
            var guid = new WowGuid64(HighGuidTypeLegacy.Creature, 100, 0);

            // Assert
            Assert.Equal(0ul, guid.Low);
            Assert.True(guid.IsEmpty());
        }
    }

    public class WowGuid128Tests
    {
        [Fact]
        public void DefaultConstructor_CreatesEmptyGuid()
        {
            // Arrange & Act
            var guid = new WowGuid128();

            // Assert
            Assert.Equal(0ul, guid.Low);
            Assert.Equal(0ul, guid.High);
            Assert.True(guid.IsEmpty());
        }

        [Fact]
        public void Constructor_WithHighAndLow()
        {
            // Arrange
            ulong high = 0x3000000000000000;
            ulong low = 0x0000000000000123;

            // Act
            var guid = new WowGuid128(low, high);

            // Assert
            Assert.Equal(high, guid.High);
            Assert.Equal(low, guid.Low);
        }

        [Fact]
        public void GetSubType_ExtractsSubTypeFromHigh()
        {
            // Arrange
            var guid = new WowGuid128(0, 0x0000000000000001);

            // Act
            var subType = guid.GetSubType();

            // Assert
            Assert.Equal(1, subType);
        }

        [Fact]
        public void GetRealmId_ExtractsRealmIdFromHigh()
        {
            // Arrange
            // RealmId is (High >> 42) & 0x1FFF
            // Set bit 42 to get realmId = 1
            ulong high = 1UL << 42;
            var guid = new WowGuid128(0, high);

            // Act
            var realmId = guid.GetRealmId();

            // Assert
            Assert.Equal(1, realmId);
        }

        [Fact]
        public void GetServerId_ExtractsServerIdFromLow()
        {
            // Arrange
            var guid = new WowGuid128(0x0100000000000000, 0);

            // Act
            var serverId = guid.GetServerId();

            // Assert
            Assert.NotEqual(0u, serverId);
        }

        [Fact]
        public void GetMapId_ExtractsMapIdFromHigh()
        {
            // Arrange
            // MapId is (High >> 29) & 0x1FFF (13 bits)
            // Set bit 29 to get mapId = 1
            ulong high = 1UL << 29;
            var guid = new WowGuid128(0, high);

            // Act
            var mapId = guid.GetMapId();

            // Assert
            Assert.Equal(1, mapId);
        }

        [Fact]
        public void GetEntry_ExtractsEntryFromHigh()
        {
            // Arrange
            var guid = new WowGuid128(0, 0x0000000001000000);

            // Act
            var entry = guid.GetEntry();

            // Assert
            Assert.NotEqual(0u, entry);
        }

        [Fact]
        public void GetCounter_ExtractsCounterFromLow()
        {
            // Arrange
            var guid = new WowGuid128(0x000000000000FFFF, 0);

            // Act
            var counter = guid.GetCounter();

            // Assert
            Assert.NotEqual(0ul, counter);
        }

        [Fact]
        public void HasEntry_WithDifferentTypes()
        {
            // Assert
            // Creature has entry
            Assert.True(WowGuid128.Create(HighGuidType703.Creature, 0, 100, 1).HasEntry());
            // GameObject has entry
            Assert.True(WowGuid128.Create(HighGuidType703.GameObject, 0, 200, 1).HasEntry());
            // Player does not have entry
            Assert.False(WowGuid128.Create(HighGuidType703.Player, 1).HasEntry());
        }

        [Fact]
        public void GetHighType_ReturnsCorrectType()
        {
            // Arrange
            var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 100, 1);

            // Act
            var highType = guid.GetHighType();

            // Assert
            Assert.Equal(HighGuidType.Creature, highType);
        }

        [Fact]
        public void IsPlayer_WithPlayerGuid_ReturnsTrue()
        {
            // Arrange
            var playerGuid = WowGuid128.Create(HighGuidType703.Player, 1);

            // Act
            var result = playerGuid.IsPlayer();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsPlayer_WithNonPlayerGuid_ReturnsFalse()
        {
            // Arrange
            var creatureGuid = WowGuid128.Create(HighGuidType703.Creature, 0, 100, 1);

            // Act
            var result = creatureGuid.IsPlayer();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsCreature_WithCreatureGuid_ReturnsTrue()
        {
            // Arrange
            var creatureGuid = WowGuid128.Create(HighGuidType703.Creature, 0, 100, 1);

            // Act
            var result = creatureGuid.IsCreature();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsCreature_WithNonCreatureGuid_ReturnsFalse()
        {
            // Arrange
            var playerGuid = WowGuid128.Create(HighGuidType703.Player, 1);

            // Act
            var result = playerGuid.IsCreature();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void IsItem_WithItemGuid_ReturnsTrue()
        {
            // Arrange
            var itemGuid = WowGuid128.Create(HighGuidType703.Item, 1);

            // Act
            var result = itemGuid.IsItem();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void IsItem_WithNonItemGuid_ReturnsFalse()
        {
            // Arrange
            var playerGuid = WowGuid128.Create(HighGuidType703.Player, 1);

            // Act
            var result = playerGuid.IsItem();

            // Assert
            Assert.False(result);
        }

        [Fact]
        public void GetObjectType_WithPlayerGuid_ReturnsPlayer()
        {
            // Arrange
            var playerGuid = WowGuid128.Create(HighGuidType703.Player, 1);

            // Act
            var objectType = playerGuid.GetObjectType();

            // Assert
            Assert.Equal(ObjectType.Player, objectType);
        }

        [Fact]
        public void GetObjectType_WithCreatureGuid_ReturnsUnit()
        {
            // Arrange
            var creatureGuid = WowGuid128.Create(HighGuidType703.Creature, 0, 100, 1);

            // Act
            var objectType = creatureGuid.GetObjectType();

            // Assert
            Assert.Equal(ObjectType.Unit, objectType);
        }

        [Fact]
        public void IsWorldObject_WithWorldObjectTypes_ReturnsTrue()
        {
            // Assert
            Assert.True(WowGuid128.Create(HighGuidType703.Player, 1).IsWorldObject());
            Assert.True(WowGuid128.Create(HighGuidType703.Creature, 0, 100, 1).IsWorldObject());
        }

        [Fact]
        public void IsTransport_WithTransportType_ReturnsTrue()
        {
            // Arrange
            var transportGuid = WowGuid128.Create(HighGuidType703.Transport, 1);

            // Act
            var result = transportGuid.IsTransport();

            // Assert
            Assert.True(result);
        }

        [Fact]
        public void Equality_WithSameValues_ReturnsTrue()
        {
            // Arrange
            var guid1 = new WowGuid128(0x3000000000000001, 0x0000000000000100);
            var guid2 = new WowGuid128(0x3000000000000001, 0x0000000000000100);

            // Act & Assert
            Assert.Equal(guid1, guid2);
            Assert.True(guid1 == guid2);
            Assert.False(guid1 != guid2);
        }

        [Fact]
        public void Equality_WithDifferentHighValues_ReturnsFalse()
        {
            // Arrange
            var guid1 = new WowGuid128(0x3000000000000001, 0);
            var guid2 = new WowGuid128(0x4000000000000001, 0);

            // Act & Assert
            Assert.NotEqual(guid1, guid2);
            Assert.True(guid1 != guid2);
        }

        [Fact]
        public void Equality_WithDifferentLowValues_ReturnsFalse()
        {
            // Arrange
            var guid1 = new WowGuid128(0x3000000000000001, 0x0000000000000100);
            var guid2 = new WowGuid128(0x3000000000000001, 0x0000000000000200);

            // Act & Assert
            Assert.NotEqual(guid1, guid2);
            Assert.True(guid1 != guid2);
        }

        [Fact]
        public void GetHashCode_WithSameValues_ReturnsSameHash()
        {
            // Arrange
            var guid1 = new WowGuid128(0x3000000000000001, 0x0000000000000100);
            var guid2 = new WowGuid128(0x3000000000000001, 0x0000000000000100);

            // Act & Assert
            Assert.Equal(guid1.GetHashCode(), guid2.GetHashCode());
        }

        [Fact]
        public void IsEmpty_WithZeroGuid_ReturnsTrue()
        {
            // Arrange
            var guid = WowGuid128.Empty;

            // Act & Assert
            Assert.True(guid.IsEmpty());
        }

        [Fact]
        public void IsEmpty_WithNonZeroGuid_ReturnsFalse()
        {
            // Arrange
            var guid = new WowGuid128(0x3000000000000001, 0x0000000000000100);

            // Act & Assert
            Assert.False(guid.IsEmpty());
        }

        [Fact]
        public void ToString_WithEmptyGuid_ReturnsRecordFormat()
        {
            // Arrange
            var guid = WowGuid128.Empty;

            // Act
            var result = guid.ToString();

            // Assert
            // Record struct uses default ToString format
            Assert.Contains("High = 0", result);
            Assert.Contains("Low = 0", result);
        }

        [Fact]
        public void ToString_WithValidGuid_ContainsHighAndLow()
        {
            // Arrange
            var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 100, 1);

            // Act
            var result = guid.ToString();

            // Assert
            // Record struct uses default ToString format with property names
            Assert.Contains("High =", result);
            Assert.Contains("Low =", result);
        }

        [Fact]
        public void To128_ReturnsSelf()
        {
            // Arrange
            var guid = WowGuid128.Create(HighGuidType703.Creature, 0, 100, 1);
            var gameState = WowGuidTestHelper.CreateMockGameSessionData();

            // Act
            var result = guid.To128(gameState);

            // Assert
            Assert.Equal(guid, result);
        }

        [Fact]
        public void StaticEmpty_ReturnsZeroGuid()
        {
            // Act & Assert
            Assert.True(WowGuid128.Empty.IsEmpty());
            Assert.Equal(0ul, WowGuid128.Empty.Low);
            Assert.Equal(0ul, WowGuid128.Empty.High);
        }

        [Fact]
        public void GetLowValue_ReturnsLow()
        {
            // Arrange
            var guid = new WowGuid128(0x3000000000000001, 0x0000000000000100);

            // Act
            var lowValue = guid.GetLowValue();

            // Assert
            Assert.Equal(guid.Low, lowValue);
        }

        [Fact]
        public void GetHighValue_ReturnsHigh()
        {
            // Arrange
            var guid = new WowGuid128(0x3000000000000001, 0x0000000000000100);

            // Act
            var highValue = guid.GetHighValue();

            // Assert
            Assert.Equal(guid.High, highValue);
        }

        [Fact]
        public void Create_GlobalType_CreatesValidGuid()
        {
            // Arrange & Act
            var guid = WowGuid128.Create(HighGuidType703.BNetAccount, 42);

            // Assert
            Assert.False(guid.IsEmpty());
            Assert.Equal(42ul, guid.GetCounter());
        }

        [Fact]
        public void Create_RealmSpecificType_CreatesValidGuid()
        {
            // Arrange & Act
            var guid = WowGuid128.Create(HighGuidType703.Player, 123);

            // Assert
            Assert.False(guid.IsEmpty());
            Assert.Equal(123ul, guid.GetCounter());
        }

        [Fact]
        public void Create_MapSpecificType_CreatesValidGuid()
        {
            // Arrange & Act
            var guid = WowGuid128.Create(HighGuidType703.Creature, 42, 100, 1);

            // Assert
            Assert.False(guid.IsEmpty());
            Assert.Equal(100u, guid.GetEntry());
            Assert.Equal(1ul, guid.GetCounter());
        }

        [Fact]
        public void CreateUnknownPlayerGuid_CreatesValidPlayerGuid()
        {
            // Arrange & Act
            var guid = WowGuid128.CreateUnknownPlayerGuid();

            // Assert
            Assert.True(guid.IsPlayer());
            Assert.True(WowGuid128.IsUnknownPlayerGuid(guid));
        }

        [Fact]
        public void IsUnknownPlayerGuid_WithKnownPlayerGuid_ReturnsFalse()
        {
            // Arrange
            var knownPlayerGuid = WowGuid128.Create(HighGuidType703.Player, 1);

            // Act & Assert
            Assert.False(WowGuid128.IsUnknownPlayerGuid(knownPlayerGuid));
        }

        [Fact]
        public void CreateLootGuid_CreatesValidLootGuid()
        {
            // Arrange & Act
            var guid = WowGuid128.CreateLootGuid(HighGuidTypeLegacy.Creature, 100, 1);

            // Assert
            Assert.False(guid.IsEmpty());
        }
    }

    public class WowGuidConversionTests
    {
        [Fact]
        public void Convert64To128_WithPlayerGuid_PreservesType()
        {
            // Arrange
            var guid64 = new WowGuid64(HighGuidTypeLegacy.Player, 123);
            var gameState = WowGuidTestHelper.CreateMockGameSessionData();

            // Act
            var guid128 = guid64.To128(gameState);

            // Assert
            Assert.Equal(HighGuidType.Player, guid128.GetHighType());
        }

        [Fact]
        public void Convert128To64_WithPlayerGuid_PreservesCounter()
        {
            // Arrange
            var guid128 = WowGuid128.Create(HighGuidType703.Player, 123);

            // Act
            var guid64 = guid128.To64();

            // Assert
            Assert.Equal(HighGuidType.Player, guid64.GetHighType());
            Assert.Equal(123ul, guid64.GetCounter());
        }

        [Fact]
        public void Convert64To128_WithPlayerGuid_PreservesTypeAndCounter()
        {
            // Arrange
            var guid64 = new WowGuid64(HighGuidTypeLegacy.Player, 123);
            var gameState = WowGuidTestHelper.CreateMockGameSessionData();

            // Act
            var guid128 = guid64.To128(gameState);

            // Assert
            // The conversion should preserve the GUID type for player guids
            Assert.Equal(HighGuidType.Player, guid128.GetHighType());
            Assert.Equal(123ul, guid128.GetCounter());
        }

        [Fact]
        public void Convert128To64_WithCreatureGuid_PreservesEntry()
        {
            // Arrange
            var guid128 = WowGuid128.Create(HighGuidType703.Creature, 0, 100, 1);

            // Act
            var guid64 = guid128.To64();

            // Assert
            Assert.Equal(100u, guid64.GetEntry());
            Assert.Equal(1ul, guid64.GetCounter());
        }

        [Fact]
        public void Convert64To128_WithEmptyGuid_CreatesEmptyGuid128()
        {
            // Arrange
            var guid64 = WowGuid64.Empty;
            var gameState = WowGuidTestHelper.CreateMockGameSessionData();

            // Act
            var guid128 = guid64.To128(gameState);

            // Assert
            Assert.True(guid128.IsEmpty());
        }

        [Fact]
        public void Convert128To64_WithEmptyGuid_CreatesEmptyGuid64()
        {
            // Arrange
            var guid128 = WowGuid128.Empty;

            // Act
            var guid64 = guid128.To64();

            // Assert
            Assert.True(guid64.IsEmpty());
        }

        [Fact]
        public void StaticConvertUniqGuid_WithValidGuid_ReturnsValidGuid64()
        {
            // Arrange
            var uniqGuid = WowGuid128.Create(HighGuidType703.Uniq, (ulong)UniqGuid.SpellTargetTradeItem);

            // Act
            var guid64 = WowGuid128.ConvertUniqGuid(uniqGuid);

            // Assert
            // Result should be a valid WowGuid64 (may be empty for this specific uniq type)
            Assert.NotEqual(default(WowGuid64), guid64);
        }
    }

    public class WowGuidStaticTests
    {
        [Fact]
        public void Create64_WithUniqType_ReturnsEmpty()
        {
            // Arrange & Act
            var guid = WowGuid64.Create(WowGuid128.Create(HighGuidType703.Uniq, 1));

            // Assert
            // Uniq type is converted via ConvertUniqGuid, which may return empty
            Assert.Equal(WowGuid64.Empty, guid);
        }

        [Fact]
        public void Create64_WithPlayerType_ReturnsPlayerGuid()
        {
            // Arrange
            var guid128 = WowGuid128.Create(HighGuidType703.Player, 123);

            // Act
            var guid64 = WowGuid64.Create(guid128);

            // Assert
            Assert.True(guid64.IsPlayer());
            Assert.Equal(123ul, guid64.GetCounter());
        }

        [Fact]
        public void Create64_WithCreatureType_ReturnsCreatureGuid()
        {
            // Arrange
            var guid128 = WowGuid128.Create(HighGuidType703.Creature, 0, 100, 42);

            // Act
            var guid64 = WowGuid64.Create(guid128);

            // Assert
            Assert.True(guid64.IsCreature());
            Assert.Equal(100u, guid64.GetEntry());
            Assert.Equal(42ul, guid64.GetCounter());
        }

        [Fact]
        public void Create64_WithItemType_ReturnsItemGuid()
        {
            // Arrange
            var guid128 = WowGuid128.Create(HighGuidType703.Item, 456);

            // Act
            var guid64 = WowGuid64.Create(guid128);

            // Assert
            Assert.True(guid64.IsItem());
            Assert.Equal(456ul, guid64.GetCounter());
        }

        [Fact]
        public void Create64_WithInvalidType_ReturnsEmpty()
        {
            // Arrange & Act
            var guid = WowGuid64.Create(WowGuid128.Empty);

            // Assert
            Assert.True(guid.IsEmpty());
        }
    }

    public class WowGuidNullabilityTests
    {
        [Fact]
        public void EqualityOperator_WithNullableValueTypes_HandlesProperly()
        {
            // Arrange
            WowGuid64? guid1 = null;
            WowGuid64? guid2 = null;

            // Act & Assert
            Assert.Equal(guid1, guid2);
        }

        [Fact]
        public void EqualityOperator_WithOneNullAndOneNonNull_ReturnsFalse()
        {
            // Arrange
            WowGuid64? guid1 = null;
            WowGuid64? guid2 = new WowGuid64(HighGuidTypeLegacy.Player, 1);

            // Act & Assert
            Assert.NotEqual(guid1, guid2);
        }

        [Fact]
        public void Equals_WithDifferentType_ReturnsFalse()
        {
            // Arrange
            var guid = new WowGuid64(HighGuidTypeLegacy.Player, 1);

            // Act & Assert
            Assert.False(guid.Equals("not a guid"));
        }
    }
}
