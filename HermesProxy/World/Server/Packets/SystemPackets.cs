/*
 * Copyright (C) 2012-2020 CypherCore <http://github.com/CypherCore>
 * 
 * This program is free software: you can redistribute it and/or modify
 * it under the terms of the GNU General Public License as published by
 * the Free Software Foundation, either version 3 of the License, or
 * (at your option) any later version.
 *
 * This program is distributed in the hope that it will be useful,
 * but WITHOUT ANY WARRANTY; without even the implied warranty of
 * MERCHANTABILITY or FITNESS FOR A PARTICULAR PURPOSE.  See the
 * GNU General Public License for more details.
 *
 * You should have received a copy of the GNU General Public License
 * along with this program.  If not, see <http://www.gnu.org/licenses/>.
 */

using Framework.IO;
using HermesProxy.Enums;
using HermesProxy.World.Enums;
using System;
using System.Collections.Generic;
using System.Text;

namespace HermesProxy.World.Server.Packets;

public class GameRuleValuePair
{
    public void Write(WorldPacket data)
    {
        data.WriteInt32(Rule);
        data.WriteInt32(Value);
    }

    // 4.4.2 adds a float value to every rule.
    public void WriteCataClassic(WorldPacket data)
    {
        data.WriteInt32(Rule);
        data.WriteInt32(Value);
        data.WriteFloat(ValueF);
    }

    public int Rule;
    public int Value;
    public float ValueF;
}

public sealed class FeatureSystemStatus : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<FeatureSystemStatus>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V4_4_2_60895, new PreCataClassicLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<FeatureSystemStatus> Layout = Layouts.ForRunningClient();

    public FeatureSystemStatus() : base(Opcode.SMSG_FEATURE_SYSTEM_STATUS)
    {
    }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>Up to 3.4.3: the writers this packet had before it got layouts, unchanged.</summary>
    internal sealed class PreCataClassicLayout : ServerPacketLayout<FeatureSystemStatus>
    {
        public override void Write(FeatureSystemStatus packet, WorldPacket data) => packet.WriteClassic(data);
    }

    /// <summary>
    /// 4.4.2, field for field TrinityCore cata_classic's FeatureSystemStatus::Write and byte for
    /// byte a packet from that server (the layout test's fixture). Against 3.4.3 it adds the RAF
    /// rewards version, the timerunning season pair, the PvP type override, the addon chat
    /// throttle, a float per game rule, a second set of feature bits and a string. Written the
    /// 3.4.3 way, the client read every flag from the wrong bit.
    /// </summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<FeatureSystemStatus>
    {
        // What a stock cata_classic realm sends for the settings HermesProxy has no source for.
        private const uint NotFoundCacheTimeSeconds = 10;
        private const int AddonChatThrottleMaxTries = 10;
        private const int AddonChatThrottleTriesRestoredPerSecond = 1;
        private const int AddonChatThrottleUsedTriesPerMessage = 1;

        public override void Write(FeatureSystemStatus packet, WorldPacket data)
        {
            data.WriteUInt8(packet.ComplaintStatus);
            data.WriteUInt32(packet.CfgRealmID);
            data.WriteInt32(packet.CfgRealmRecID);

            data.WriteUInt32(packet.RAFSystem.MaxRecruits);
            data.WriteUInt32(packet.RAFSystem.MaxRecruitMonths);
            data.WriteUInt32(packet.RAFSystem.MaxRecruitmentUses);
            data.WriteUInt32(packet.RAFSystem.DaysInCycle);
            data.WriteUInt32(0); // RewardsVersion

            data.WriteUInt32(packet.TokenPollTimeSeconds);
            data.WriteUInt32(packet.KioskSessionMinutes);
            data.WriteInt64(packet.TokenBalanceAmount);
            data.WriteUInt32(packet.BpayStoreProductDeliveryDelay);
            data.WriteUInt32(packet.ClubsPresenceUpdateTimer);
            data.WriteUInt32(packet.HiddenUIClubsPresenceUpdateTimer);

            data.WriteInt32(packet.ActiveSeason);
            data.WriteInt32(packet.GameRuleValues.Count);
            data.WriteInt32(0); // ActiveTimerunningSeasonID
            data.WriteInt32(0); // RemainingTimerunningSeasonSeconds

            data.WriteInt16(packet.MaxPlayerNameQueriesPerPacket);
            data.WriteInt16(packet.PlayerNameQueryTelemetryInterval);
            data.WriteUInt32(NotFoundCacheTimeSeconds);

            data.WriteInt32(0); // RealmPvpTypeOverride

            data.WriteInt32(AddonChatThrottleMaxTries);
            data.WriteInt32(AddonChatThrottleTriesRestoredPerSecond);
            data.WriteInt32(AddonChatThrottleUsedTriesPerMessage);

            foreach (var rulePair in packet.GameRuleValues)
                rulePair.WriteCataClassic(data);

            data.WriteBit(packet.VoiceEnabled);
            data.WriteBit(packet.EuropaTicketSystemStatus != null);
            data.WriteBit(packet.BpayStoreEnabled);
            data.WriteBit(packet.BpayStoreAvailable);
            data.WriteBit(packet.BpayStoreDisabledByParentalControls);
            data.WriteBit(packet.ItemRestorationButtonEnabled);
            data.WriteBit(packet.BrowserEnabled);
            data.WriteBit(packet.SessionAlert != null);

            data.WriteBit(packet.RAFSystem.Enabled);
            data.WriteBit(packet.RAFSystem.RecruitingEnabled);
            data.WriteBit(packet.CharUndeleteEnabled);
            data.WriteBit(packet.RestrictedAccount);
            data.WriteBit(packet.CommerceSystemEnabled);
            data.WriteBit(packet.TutorialsEnabled);
            data.WriteBit(packet.Unk67);              // VeteranTokenRedeemWillKick
            data.WriteBit(packet.WillKickFromWorld);  // WorldTokenRedeemWillKick

            data.WriteBit(packet.KioskModeEnabled);
            data.WriteBit(packet.CompetitiveModeEnabled);
            data.WriteBit(packet.TokenBalanceEnabled);
            data.WriteBit(packet.WarModeFeatureEnabled);
            data.WriteBit(packet.ClubsEnabled);
            data.WriteBit(packet.ClubsBattleNetClubTypeAllowed);
            data.WriteBit(packet.ClubsCharacterClubTypeAllowed);
            data.WriteBit(packet.ClubsPresenceUpdateEnabled);

            data.WriteBit(packet.VoiceChatDisabledByParentalControl);
            data.WriteBit(packet.VoiceChatMutedByParentalControl);
            data.WriteBit(packet.QuestSessionEnabled);
            data.WriteBit(packet.IsMuted);
            data.WriteBit(packet.ClubFinderEnabled);
            data.WriteBit(false); // CommunityFinderEnabled
            data.WriteBit(false); // BrowserCrashReporterEnabled
            data.WriteBit(false); // SpeakForMeAllowed

            data.WriteBit(false); // DoesAccountNeedAADCPrompt
            data.WriteBit(false); // IsAccountOptedInToAADC
            data.WriteBit(packet.LFGListCustomRequiresAuthenticator);
            data.WriteBit(false); // ScriptsDisallowedForBeta
            data.WriteBit(true);  // WarGamesEnabled
            data.WriteBit(packet.RaceClassExpansionLevels.Count > 0);
            data.WriteBit(true);  // Unk441_0, set by a stock realm
            data.WriteBit(false); // Unk441_1

            data.WriteBit(true);  // GroupFinderEnabled
            data.WriteBit(false); // PremadeGroupEnabled
            data.WriteBit(true);  // LFDEnabled
            data.WriteBit(false); // LFREnabled
            data.WriteBit(false); // UseActivePlayerDataQuestCompleted
            data.WriteBit(false); // PetHappinessEnabled
            data.WriteBit(true);  // GuildEventsEditsEnabled
            data.WriteBit(false); // GuildTradeSkillsEnabled

            data.WriteBits(0u, 7); // Unknown1027 length
            data.WriteBit(true);  // BNSendWhisperUseV2Services
            data.WriteBit(true);  // BNSendGameDataUseV2Services
            data.WriteBit(false); // IsAccountCurrencyTransferEnabled
            data.WriteBit(false);
            data.WriteBit(false); // LobbyMatchmakerQueueFromMainlineEnabled
            data.WriteBit(false); // CanSendLobbyMatchmakerPartyCustomizations
            data.FlushBits();

            packet.WriteQuickJoinConfig(data);

            if (packet.SessionAlert != null)
            {
                data.WriteInt32(packet.SessionAlert.Delay);
                data.WriteInt32(packet.SessionAlert.Period);
                data.WriteInt32(packet.SessionAlert.DisplayTime);
            }

            if (packet.RaceClassExpansionLevels.Count > 0)
            {
                data.WriteInt32(packet.RaceClassExpansionLevels.Count);
                foreach (byte level in packet.RaceClassExpansionLevels)
                    data.WriteUInt8(level);
            }

            data.WriteBit(packet.Squelch.IsSquelched);
            data.WritePackedGuid128(packet.Squelch.BnetAccountGuid);
            data.WritePackedGuid128(packet.Squelch.GuildGuid);

            if (packet.EuropaTicketSystemStatus != null)
                packet.EuropaTicketSystemStatus.Write(data);
        }
    }

    private void WriteQuickJoinConfig(WorldPacket data)
    {
        data.WriteBit(QuickJoinConfig.ToastsDisabled);
        data.WriteFloat(QuickJoinConfig.ToastDuration);
        data.WriteFloat(QuickJoinConfig.DelayDuration);
        data.WriteFloat(QuickJoinConfig.QueueMultiplier);
        data.WriteFloat(QuickJoinConfig.PlayerMultiplier);
        data.WriteFloat(QuickJoinConfig.PlayerFriendValue);
        data.WriteFloat(QuickJoinConfig.PlayerGuildValue);
        data.WriteFloat(QuickJoinConfig.ThrottleInitialThreshold);
        data.WriteFloat(QuickJoinConfig.ThrottleDecayTime);
        data.WriteFloat(QuickJoinConfig.ThrottlePrioritySpike);
        data.WriteFloat(QuickJoinConfig.ThrottleMinThreshold);
        data.WriteFloat(QuickJoinConfig.ThrottlePvPPriorityNormal);
        data.WriteFloat(QuickJoinConfig.ThrottlePvPPriorityLow);
        data.WriteFloat(QuickJoinConfig.ThrottlePvPHonorThreshold);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityDefault);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityAbove);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityBelow);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListIlvlScalingAbove);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListIlvlScalingBelow);
        data.WriteFloat(QuickJoinConfig.ThrottleRfPriorityAbove);
        data.WriteFloat(QuickJoinConfig.ThrottleRfIlvlScalingAbove);
        data.WriteFloat(QuickJoinConfig.ThrottleDfMaxItemLevel);
        data.WriteFloat(QuickJoinConfig.ThrottleDfBestPriority);
    }

    private void WriteClassic(WorldPacket data)
    {
        // V3_4_3_54261 (WotLK Classic) wire layout diverges from the unified
        // modern path. WPP V3_4_3_51505→V3_4_4_59817 parser (in
        // WowPacketParserModule.V3_4_0_45166/Parsers/MiscellaneousHandler.cs:414)
        // confirms the V3_4_3 wire DOES NOT carry Scroll-of-Resurrection +
        // Twitter throttle fields and DOES carry an extra RAFSystem.Unknown1007
        // u32, a trailing PlayerNameQueryInterval u32, and an extended bit
        // section ending with IsGroupFinderEnabled / IsLFDEnabled / IsLFREnabled
        // / IsPremadeGroupEnabled.
        //
        // The IsLFDEnabled bit GATES the entire Dungeon Finder UI in the modern
        // V3_4_3 client — without it set, the LFG microbar eye icon is hidden,
        // "Find Group" button is inert, and the left sidebar "Dungeon Finder"
        // category does not populate. Confirmed by Wrathion 3.4.3 reference
        // sniff (World_dungeon_finder_exists_parsed.txt:36832) which sets it
        // True alongside IsGroupFinderEnabled True.
        if (ModernVersion.IsWotLKClassicOrLater)
        {
            WriteV3_4_3(data);
            return;
        }

        data.WriteUInt8(ComplaintStatus);

        data.WriteUInt32(ScrollOfResurrectionRequestsRemaining);
        data.WriteUInt32(ScrollOfResurrectionMaxRequestsPerDay);

        data.WriteUInt32(CfgRealmID);
        data.WriteInt32(CfgRealmRecID);

        data.WriteUInt32(RAFSystem.MaxRecruits);
        data.WriteUInt32(RAFSystem.MaxRecruitMonths);
        data.WriteUInt32(RAFSystem.MaxRecruitmentUses);
        data.WriteUInt32(RAFSystem.DaysInCycle);

        data.WriteUInt32(TwitterPostThrottleLimit);
        data.WriteUInt32(TwitterPostThrottleCooldown);

        data.WriteUInt32(TokenPollTimeSeconds);
        data.WriteUInt32(KioskSessionMinutes);
        data.WriteInt64(TokenBalanceAmount);

        data.WriteUInt32(BpayStoreProductDeliveryDelay);
        data.WriteUInt32(ClubsPresenceUpdateTimer);
        data.WriteUInt32(HiddenUIClubsPresenceUpdateTimer);

        if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
        {
            data.WriteInt32(ActiveSeason);
            data.WriteInt32(GameRuleValues.Count);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                data.WriteInt16(MaxPlayerNameQueriesPerPacket);

            if (ModernVersion.AddedInVersion(9, 2, 7, 1, 14, 4, 3, 4, 0))
                data.WriteInt16(PlayerNameQueryTelemetryInterval);

            foreach (var rulePair in GameRuleValues)
                rulePair.Write(data);
        }

        data.WriteBit(VoiceEnabled);
        data.WriteBit(EuropaTicketSystemStatus != null);
        data.WriteBit(ScrollOfResurrectionEnabled);
        data.WriteBit(BpayStoreEnabled);
        data.WriteBit(BpayStoreAvailable);
        data.WriteBit(BpayStoreDisabledByParentalControls);
        data.WriteBit(ItemRestorationButtonEnabled);
        data.WriteBit(BrowserEnabled);
        data.WriteBit(SessionAlert != null);
        data.WriteBit(RAFSystem.Enabled);
        data.WriteBit(RAFSystem.RecruitingEnabled);
        data.WriteBit(CharUndeleteEnabled);
        data.WriteBit(RestrictedAccount);
        data.WriteBit(CommerceSystemEnabled);
        data.WriteBit(TutorialsEnabled);
        data.WriteBit(TwitterEnabled);
        data.WriteBit(Unk67);
        data.WriteBit(WillKickFromWorld);
        data.WriteBit(KioskModeEnabled);
        data.WriteBit(CompetitiveModeEnabled);
        data.WriteBit(TokenBalanceEnabled);
        data.WriteBit(WarModeFeatureEnabled);
        data.WriteBit(ClubsEnabled);
        data.WriteBit(ClubsBattleNetClubTypeAllowed);
        data.WriteBit(ClubsCharacterClubTypeAllowed);
        data.WriteBit(ClubsPresenceUpdateEnabled);
        data.WriteBit(VoiceChatDisabledByParentalControl);
        data.WriteBit(VoiceChatMutedByParentalControl);
        data.WriteBit(QuestSessionEnabled);
        data.WriteBit(IsMuted);
        data.WriteBit(ClubFinderEnabled);
        data.WriteBit(Unknown901CheckoutRelated);

        if (ModernVersion.AddedInVersion(9, 1, 5, 1, 14, 1, 2, 5, 3))
        {
            data.WriteBit(TextToSpeechFeatureEnabled);
            data.WriteBit(ChatDisabledByDefault);
            data.WriteBit(ChatDisabledByPlayer);
            data.WriteBit(LFGListCustomRequiresAuthenticator);
        }

        if (ModernVersion.IsClassicVersionBuild())
        {
            data.WriteBit(BattlegroundsEnabled);
            data.WriteBit(RaceClassExpansionLevels.Count > 0);
        }
        
        data.FlushBits();

        {
            data.WriteBit(QuickJoinConfig.ToastsDisabled);
            data.WriteFloat(QuickJoinConfig.ToastDuration);
            data.WriteFloat(QuickJoinConfig.DelayDuration);
            data.WriteFloat(QuickJoinConfig.QueueMultiplier);
            data.WriteFloat(QuickJoinConfig.PlayerMultiplier);
            data.WriteFloat(QuickJoinConfig.PlayerFriendValue);
            data.WriteFloat(QuickJoinConfig.PlayerGuildValue);
            data.WriteFloat(QuickJoinConfig.ThrottleInitialThreshold);
            data.WriteFloat(QuickJoinConfig.ThrottleDecayTime);
            data.WriteFloat(QuickJoinConfig.ThrottlePrioritySpike);
            data.WriteFloat(QuickJoinConfig.ThrottleMinThreshold);
            data.WriteFloat(QuickJoinConfig.ThrottlePvPPriorityNormal);
            data.WriteFloat(QuickJoinConfig.ThrottlePvPPriorityLow);
            data.WriteFloat(QuickJoinConfig.ThrottlePvPHonorThreshold);
            data.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityDefault);
            data.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityAbove);
            data.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityBelow);
            data.WriteFloat(QuickJoinConfig.ThrottleLfgListIlvlScalingAbove);
            data.WriteFloat(QuickJoinConfig.ThrottleLfgListIlvlScalingBelow);
            data.WriteFloat(QuickJoinConfig.ThrottleRfPriorityAbove);
            data.WriteFloat(QuickJoinConfig.ThrottleRfIlvlScalingAbove);
            data.WriteFloat(QuickJoinConfig.ThrottleDfMaxItemLevel);
            data.WriteFloat(QuickJoinConfig.ThrottleDfBestPriority);
        }

        if (SessionAlert != null)
        {
            data.WriteInt32(SessionAlert.Delay);
            data.WriteInt32(SessionAlert.Period);
            data.WriteInt32(SessionAlert.DisplayTime);
        }

        if (ModernVersion.IsClassicVersionBuild())
        {
            if (RaceClassExpansionLevels.Count > 0)
            {
                data.WriteInt32(RaceClassExpansionLevels.Count);
                for (var i = 0; i < RaceClassExpansionLevels.Count; ++i)
                    data.WriteUInt8(RaceClassExpansionLevels[i]);
            }
        }

        data.WriteBit(Squelch.IsSquelched);
        data.WritePackedGuid128(Squelch.BnetAccountGuid);
        data.WritePackedGuid128(Squelch.GuildGuid);

        if (EuropaTicketSystemStatus != null)
            EuropaTicketSystemStatus.Write(data);
    }

    private void WriteV3_4_3(WorldPacket data)
    {
        // Field-by-field per WPP V3_4_3 parser
        // (WowPacketParserModule.V3_4_0_45166/Parsers/MiscellaneousHandler.cs:414-535).
        data.WriteUInt8(ComplaintStatus);
        data.WriteUInt32(CfgRealmID);
        data.WriteInt32(CfgRealmRecID);

        // RAFSystem header — 5 uint32 (includes Unknown1007 vs 4 on older builds)
        data.WriteUInt32(RAFSystem.MaxRecruits);
        data.WriteUInt32(RAFSystem.MaxRecruitMonths);
        data.WriteUInt32(RAFSystem.MaxRecruitmentUses);
        data.WriteUInt32(RAFSystem.DaysInCycle);
        data.WriteUInt32(0); // Unknown1007

        data.WriteUInt32(TokenPollTimeSeconds);
        data.WriteUInt32(KioskSessionMinutes);
        data.WriteInt64(TokenBalanceAmount);
        data.WriteUInt32(BpayStoreProductDeliveryDelay);
        data.WriteUInt32(ClubsPresenceUpdateTimer);
        data.WriteUInt32(HiddenUIClubsPresenceUpdateTimer);

        data.WriteInt32(ActiveSeason);
        data.WriteInt32(GameRuleValues.Count);

        data.WriteInt16(MaxPlayerNameQueriesPerPacket);
        data.WriteInt16(PlayerNameQueryTelemetryInterval);
        data.WriteUInt32(10); // PlayerNameQueryInterval (Wrathion reference: 10)

        foreach (var rulePair in GameRuleValues)
            rulePair.Write(data);

        // 44 bits in WPP order, broken into 5-6 bytes via FlushBits at end.
        data.WriteBit(VoiceEnabled);
        data.WriteBit(EuropaTicketSystemStatus != null);
        data.WriteBit(BpayStoreEnabled);
        data.WriteBit(BpayStoreAvailable);
        data.WriteBit(BpayStoreDisabledByParentalControls);
        data.WriteBit(ItemRestorationButtonEnabled);
        data.WriteBit(BrowserEnabled);
        data.WriteBit(SessionAlert != null);

        data.WriteBit(RAFSystem.Enabled);
        data.WriteBit(RAFSystem.RecruitingEnabled);
        data.WriteBit(CharUndeleteEnabled);
        data.WriteBit(RestrictedAccount);
        data.WriteBit(CommerceSystemEnabled);
        data.WriteBit(TutorialsEnabled);
        data.WriteBit(Unk67);
        data.WriteBit(WillKickFromWorld);

        data.WriteBit(KioskModeEnabled);
        data.WriteBit(CompetitiveModeEnabled);
        data.WriteBit(TokenBalanceEnabled);
        data.WriteBit(WarModeFeatureEnabled);
        data.WriteBit(ClubsEnabled);
        data.WriteBit(ClubsBattleNetClubTypeAllowed);
        data.WriteBit(ClubsCharacterClubTypeAllowed);
        data.WriteBit(ClubsPresenceUpdateEnabled);

        data.WriteBit(VoiceChatDisabledByParentalControl);
        data.WriteBit(VoiceChatMutedByParentalControl);
        data.WriteBit(QuestSessionEnabled);
        data.WriteBit(IsMuted);
        data.WriteBit(ClubFinderEnabled);
        data.WriteBit(Unknown901CheckoutRelated);
        data.WriteBit(TextToSpeechFeatureEnabled);
        data.WriteBit(ChatDisabledByDefault);

        data.WriteBit(ChatDisabledByPlayer);
        data.WriteBit(LFGListCustomRequiresAuthenticator);
        data.WriteBit(false); // AddonsDisabled
        data.WriteBit(true);  // WarGamesEnabled (Wrathion default)
        data.WriteBit(false); // Unk343_1
        data.WriteBit(false); // Unk343_2
        data.WriteBit(false); // ContentTrackingEnabled
        data.WriteBit(true);  // IsSellAllJunkEnabled (Wrathion default)

        data.WriteBit(true);  // IsGroupFinderEnabled — required for LFG UI
        data.WriteBit(true);  // IsLFDEnabled — gates the LFG eye microbar icon + Dungeon Finder UI
        data.WriteBit(false); // IsLFREnabled (no LFR in WotLK Classic)
        data.WriteBit(false); // IsPremadeGroupEnabled (no Premade Groups in WotLK Classic)
        data.FlushBits();

        // QuickJoinConfig: ResetBitReader (= FlushBits already done), then 1 bit + 22 floats.
        data.WriteBit(QuickJoinConfig.ToastsDisabled);
        data.WriteFloat(QuickJoinConfig.ToastDuration);
        data.WriteFloat(QuickJoinConfig.DelayDuration);
        data.WriteFloat(QuickJoinConfig.QueueMultiplier);
        data.WriteFloat(QuickJoinConfig.PlayerMultiplier);
        data.WriteFloat(QuickJoinConfig.PlayerFriendValue);
        data.WriteFloat(QuickJoinConfig.PlayerGuildValue);
        data.WriteFloat(QuickJoinConfig.ThrottleInitialThreshold);
        data.WriteFloat(QuickJoinConfig.ThrottleDecayTime);
        data.WriteFloat(QuickJoinConfig.ThrottlePrioritySpike);
        data.WriteFloat(QuickJoinConfig.ThrottleMinThreshold);
        data.WriteFloat(QuickJoinConfig.ThrottlePvPPriorityNormal);
        data.WriteFloat(QuickJoinConfig.ThrottlePvPPriorityLow);
        data.WriteFloat(QuickJoinConfig.ThrottlePvPHonorThreshold);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityDefault);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityAbove);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListPriorityBelow);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListIlvlScalingAbove);
        data.WriteFloat(QuickJoinConfig.ThrottleLfgListIlvlScalingBelow);
        data.WriteFloat(QuickJoinConfig.ThrottleRfPriorityAbove);
        data.WriteFloat(QuickJoinConfig.ThrottleRfIlvlScalingAbove);
        data.WriteFloat(QuickJoinConfig.ThrottleDfMaxItemLevel);
        data.WriteFloat(QuickJoinConfig.ThrottleDfBestPriority);

        if (SessionAlert != null)
        {
            data.WriteInt32(SessionAlert.Delay);
            data.WriteInt32(SessionAlert.Period);
            data.WriteInt32(SessionAlert.DisplayTime);
        }

        // VoiceChatManagerSettings is V8.0.1+; absent on V3_4_3 wire.

        data.WriteBit(Squelch.IsSquelched);
        data.WritePackedGuid128(Squelch.BnetAccountGuid);
        data.WritePackedGuid128(Squelch.GuildGuid);

        if (EuropaTicketSystemStatus != null)
            EuropaTicketSystemStatus.Write(data);
    }

    public bool VoiceEnabled;
    public bool BrowserEnabled;
    public bool BpayStoreAvailable;
    public bool BpayStoreEnabled;
    public SessionAlertConfig SessionAlert = null!;
    public uint ScrollOfResurrectionMaxRequestsPerDay;
    public bool ScrollOfResurrectionEnabled;
    public EuropaTicketConfig EuropaTicketSystemStatus = null!;
    public uint ScrollOfResurrectionRequestsRemaining;
    public uint CfgRealmID;
    public byte ComplaintStatus;
    public int CfgRealmRecID;
    public uint TwitterPostThrottleLimit;
    public uint TwitterPostThrottleCooldown;
    public uint TokenPollTimeSeconds;
    public long TokenBalanceAmount;
    public uint BpayStoreProductDeliveryDelay;
    public uint ClubsPresenceUpdateTimer;
    public uint HiddenUIClubsPresenceUpdateTimer; // Timer for updating club presence when communities ui frame is hidden
    public int ActiveSeason;
    public List<GameRuleValuePair> GameRuleValues = new List<GameRuleValuePair>();
    public short MaxPlayerNameQueriesPerPacket;
    public short PlayerNameQueryTelemetryInterval;
    public uint KioskSessionMinutes;
    public bool ItemRestorationButtonEnabled;
    public bool CharUndeleteEnabled; // Implemented
    public bool BpayStoreDisabledByParentalControls;
    public bool TwitterEnabled;
    public bool CommerceSystemEnabled;
    public bool Unk67;
    public bool WillKickFromWorld;
    public bool RestrictedAccount;
    public bool TutorialsEnabled;
    public bool KioskModeEnabled;
    public bool CompetitiveModeEnabled;
    public bool TokenBalanceEnabled;
    public bool WarModeFeatureEnabled;
    public bool ClubsEnabled;
    public bool ClubsBattleNetClubTypeAllowed;
    public bool ClubsCharacterClubTypeAllowed;
    public bool ClubsPresenceUpdateEnabled;
    public bool VoiceChatDisabledByParentalControl;
    public bool VoiceChatMutedByParentalControl;
    public bool QuestSessionEnabled;
    public bool IsMuted;
    public bool ClubFinderEnabled;
    public bool Unknown901CheckoutRelated;
    public bool TextToSpeechFeatureEnabled;
    public bool ChatDisabledByDefault;
    public bool ChatDisabledByPlayer;
    public bool LFGListCustomRequiresAuthenticator;
    public bool BattlegroundsEnabled;
    public List<byte> RaceClassExpansionLevels = new();

    public SocialQueueConfig QuickJoinConfig;
    public SquelchInfo Squelch;
    public RafSystemFeatureInfo RAFSystem;

    public class SessionAlertConfig
    {
        public int Delay;
        public int Period;
        public int DisplayTime;
    }

    public struct SocialQueueConfig
    {
        public bool ToastsDisabled;
        public float ToastDuration;
        public float DelayDuration;
        public float QueueMultiplier;
        public float PlayerMultiplier;
        public float PlayerFriendValue;
        public float PlayerGuildValue;
        public float ThrottleInitialThreshold;
        public float ThrottleDecayTime;
        public float ThrottlePrioritySpike;
        public float ThrottleMinThreshold;
        public float ThrottlePvPPriorityNormal;
        public float ThrottlePvPPriorityLow;
        public float ThrottlePvPHonorThreshold;
        public float ThrottleLfgListPriorityDefault;
        public float ThrottleLfgListPriorityAbove;
        public float ThrottleLfgListPriorityBelow;
        public float ThrottleLfgListIlvlScalingAbove;
        public float ThrottleLfgListIlvlScalingBelow;
        public float ThrottleRfPriorityAbove;
        public float ThrottleRfIlvlScalingAbove;
        public float ThrottleDfMaxItemLevel;
        public float ThrottleDfBestPriority;
    }

    public struct SquelchInfo
    {
        public bool IsSquelched;
        public WowGuid128 BnetAccountGuid;
        public WowGuid128 GuildGuid;
    }

    public struct RafSystemFeatureInfo
    {
        public bool Enabled;
        public bool RecruitingEnabled;
        public uint MaxRecruits;
        public uint MaxRecruitMonths;
        public uint MaxRecruitmentUses;
        public uint DaysInCycle;
    }
}

public sealed class FeatureSystemStatusGlueScreen : ServerPacket
{
    internal static readonly ServerPacketLayouts<ServerPacketLayout<FeatureSystemStatusGlueScreen>> Layouts = new(
        (ClientVersionBuild.Zero, ClientVersionBuild.V3_4_3_54261, new ClassicEraLayout()),
        (ClientVersionBuild.V3_4_3_54261, ClientVersionBuild.V4_4_2_60895, new WotLKClassicLayout()),
        (ClientVersionBuild.V4_4_2_60895, ClientVersionBuild.Zero, new CataClassicLayout()));

    private static readonly ServerPacketLayout<FeatureSystemStatusGlueScreen> Layout = Layouts.ForRunningClient();

    public FeatureSystemStatusGlueScreen() : base(Opcode.SMSG_FEATURE_SYSTEM_STATUS_GLUE_SCREEN) { }

    public override void Write() => Layout.Write(this, _worldPacket);

    /// <summary>1.14 and 2.5.</summary>
    internal sealed class ClassicEraLayout : ServerPacketLayout<FeatureSystemStatusGlueScreen>
    {
        public override void Write(FeatureSystemStatusGlueScreen packet, WorldPacket data)
        {
            // Legacy modern (V1_14, V2_5) layout — preserve current behavior.
            data.WriteBit(packet.BpayStoreEnabled);
            data.WriteBit(packet.BpayStoreAvailable);
            data.WriteBit(packet.BpayStoreDisabledByParentalControls);
            data.WriteBit(packet.CharUndeleteEnabled);
            data.WriteBit(packet.CommerceSystemEnabled);
            data.WriteBit(packet.Unk14);
            data.WriteBit(packet.WillKickFromWorld);
            data.WriteBit(packet.IsExpansionPreorderInStore);
            data.WriteBit(packet.KioskModeEnabled);
            data.WriteBit(packet.CompetitiveModeEnabled);
            data.WriteBit(packet.TrialBoostEnabled);
            data.WriteBit(packet.TokenBalanceEnabled);
            data.WriteBit(packet.LiveRegionCharacterListEnabled);
            data.WriteBit(packet.LiveRegionCharacterCopyEnabled);
            data.WriteBit(packet.LiveRegionAccountCopyEnabled);
            data.WriteBit(packet.LiveRegionKeyBindingsCopyEnabled);
            data.WriteBit(packet.Unknown901CheckoutRelated);
            data.WriteBit(packet.EuropaTicketSystemStatus != null);
            data.FlushBits();

            if (packet.EuropaTicketSystemStatus != null)
                packet.EuropaTicketSystemStatus.Write(data);

            data.WriteUInt32(packet.TokenPollTimeSeconds);
            data.WriteUInt32(packet.KioskSessionMinutes);
            data.WriteInt64(packet.TokenBalanceAmount);
            data.WriteInt32(packet.MaxCharactersPerRealm);
            data.WriteInt32(packet.LiveRegionCharacterCopySourceRegions.Count);
            data.WriteUInt32(packet.BpayStoreProductDeliveryDelay);
            data.WriteInt32(packet.ActiveCharacterUpgradeBoostType);
            data.WriteInt32(packet.ActiveClassTrialBoostType);
            data.WriteInt32(packet.MinimumExpansionLevel);
            data.WriteInt32(packet.MaximumExpansionLevel);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                data.WriteInt32(packet.ActiveSeason);
                data.WriteInt32(packet.GameRuleValues.Count);

                if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 2, 2, 5, 3))
                    data.WriteInt16(packet.MaxPlayerNameQueriesPerPacket);

                if (ModernVersion.AddedInVersion(9, 2, 7, 1, 14, 4, 3, 4, 0))
                    data.WriteInt16(packet.PlayerNameQueryTelemetryInterval);
            }

            foreach (var sourceRegion in packet.LiveRegionCharacterCopySourceRegions)
                data.WriteInt32(sourceRegion);

            if (ModernVersion.AddedInVersion(9, 2, 0, 1, 14, 1, 2, 5, 3))
            {
                foreach (var rulePair in packet.GameRuleValues)
                    rulePair.Write(data);
            }
        }
    }

    /// <summary>3.4.3.</summary>
    internal sealed class WotLKClassicLayout : ServerPacketLayout<FeatureSystemStatusGlueScreen>
    {
        public override void Write(FeatureSystemStatusGlueScreen packet, WorldPacket data)
        {
            // 3.4.3 (WotLK Classic) layout per WPP V3_4_0_45166 MiscellaneousHandler.cs:124-203
            // (gated on V3_4_3_51505+, before V3_4_4_59817). Reads 30 bits + EuropaTicket-conditional
            // payload + 11 trailing uint/int fields. Critical: this packet is sent BEFORE
            // SMSG_ENUM_CHARACTERS_RESULT — getting it wrong misaligns every byte after, including
            // the character list, breaking client deserialization silently.
            data.WriteBit(packet.BpayStoreEnabled);
            data.WriteBit(packet.BpayStoreAvailable);
            data.WriteBit(packet.BpayStoreDisabledByParentalControls);
            data.WriteBit(packet.CharUndeleteEnabled);
            data.WriteBit(packet.CommerceSystemEnabled);
            data.WriteBit(packet.Unk14);
            data.WriteBit(packet.WillKickFromWorld);
            data.WriteBit(packet.IsExpansionPreorderInStore);

            data.WriteBit(packet.KioskModeEnabled);
            data.WriteBit(packet.CompetitiveModeEnabled);
            data.WriteBit(false); // IsBoostEnabled
            data.WriteBit(packet.TrialBoostEnabled);
            data.WriteBit(packet.TokenBalanceEnabled);
            data.WriteBit(packet.LiveRegionCharacterListEnabled);
            data.WriteBit(packet.LiveRegionCharacterCopyEnabled);
            data.WriteBit(packet.LiveRegionAccountCopyEnabled);

            data.WriteBit(packet.LiveRegionKeyBindingsCopyEnabled);
            data.WriteBit(packet.Unknown901CheckoutRelated);
            data.WriteBit(false); // SoftTargetEnabled
            data.WriteBit(packet.EuropaTicketSystemStatus != null); // IsEuropaTicketSystemStatusEnabled
            data.WriteBit(false); // IsNameReservationEnabled
            data.WriteBit(false); // IsLaunchETA
            data.WriteBit(false); // AddonsDisabled
            data.WriteBit(false); // Unk

            data.WriteBit(false); // Unk
            data.WriteBit(false); // SoMNotificationEnabled
            data.WriteBit(false); // AccountSaveDataExportEnabled
            data.WriteBit(false); // AccountLockedByExport
            data.WriteBit(false); // Unk
            data.WriteBit(false); // IsRealmHiddenAlert (no following 11-bit payload since false)

            data.FlushBits();

            if (packet.EuropaTicketSystemStatus != null)
                packet.EuropaTicketSystemStatus.Write(data);

            data.WriteUInt32(packet.TokenPollTimeSeconds);
            data.WriteUInt32(packet.KioskSessionMinutes);
            data.WriteInt64(packet.TokenBalanceAmount);
            data.WriteInt32(packet.MaxCharactersPerRealm);
            data.WriteInt32(packet.LiveRegionCharacterCopySourceRegions.Count);
            data.WriteUInt32(packet.BpayStoreProductDeliveryDelay);
            data.WriteInt32(packet.ActiveCharacterUpgradeBoostType);
            data.WriteInt32(packet.ActiveClassTrialBoostType);
            data.WriteInt32(packet.MinimumExpansionLevel);
            data.WriteInt32(packet.MaximumExpansionLevel);
            data.WriteInt32(packet.ActiveSeason);
            data.WriteInt32(packet.GameRuleValues.Count);
            data.WriteInt16(packet.MaxPlayerNameQueriesPerPacket);
            data.WriteInt16(packet.PlayerNameQueryTelemetryInterval);
            data.WriteInt32(0);  // PlayerNameQueryInterval
            data.WriteInt32(0);  // DebugTimeEventsSize
            data.WriteInt32(0);  // Unused1007

            // No IsLaunchETA payload (bit was false), no DebugTimeEvents loop (count = 0).

            foreach (var sourceRegion in packet.LiveRegionCharacterCopySourceRegions)
                data.WriteInt32(sourceRegion);

            foreach (var rulePair in packet.GameRuleValues)
                rulePair.Write(data);
        }
    }

    /// <summary>
    /// Cataclysm Classic 4.4.2: TrinityCore cata_classic's writer. <c>FeatureSystemStatusGlueScreenLayoutTests</c>
    /// checks it byte for byte against a native capture.
    /// </summary>
    internal sealed class CataClassicLayout : ServerPacketLayout<FeatureSystemStatusGlueScreen>
    {
        public override void Write(FeatureSystemStatusGlueScreen packet, WorldPacket data)
        {
            data.WriteBit(packet.BpayStoreEnabled);
            data.WriteBit(packet.BpayStoreAvailable);
            data.WriteBit(packet.BpayStoreDisabledByParentalControls);
            data.WriteBit(packet.CharUndeleteEnabled);
            data.WriteBit(packet.CommerceSystemEnabled);
            data.WriteBit(packet.Unk14);                          // VeteranTokenRedeemWillKick
            data.WriteBit(packet.WillKickFromWorld);              // WorldTokenRedeemWillKick
            data.WriteBit(packet.IsExpansionPreorderInStore);

            data.WriteBit(packet.KioskModeEnabled);
            data.WriteBit(packet.CompetitiveModeEnabled);
            data.WriteBit(false);                                 // BoostEnabled
            data.WriteBit(packet.TrialBoostEnabled);
            data.WriteBit(packet.TokenBalanceEnabled);            // RedeemForBalanceAvailable
            data.WriteBit(false);                                 // PaidCharacterTransfersBetweenBnetAccountsEnabled
            data.WriteBit(packet.LiveRegionCharacterListEnabled);
            data.WriteBit(packet.LiveRegionCharacterCopyEnabled);

            data.WriteBit(packet.LiveRegionAccountCopyEnabled);
            data.WriteBit(packet.LiveRegionKeyBindingsCopyEnabled);
            data.WriteBit(false);                                 // BrowserCrashReporterEnabled
            data.WriteBit(false);                                 // IsEmployeeAccount
            data.WriteBit(packet.EuropaTicketSystemStatus != null);
            data.WriteBit(false);                                 // NameReservationOnly
            data.WriteBit(false);                                 // LaunchDurationETA
            data.WriteBit(false);                                 // TimerunningEnabled

            data.WriteBit(false);                                 // Unk441_0
            data.WriteBit(false);                                 // Unk441_1
            data.WriteBit(false);                                 // SoMNotificationEnabled
            data.WriteBit(false);                                 // Unk441_2
            data.WriteBit(false);                                 // AddonsDisabled
            data.WriteBit(false);                                 // Unused1000
            data.WriteBit(false);                                 // AccountSaveDataExportEnabled
            data.WriteBit(false);                                 // AccountLockedByExport

            data.WriteBits(1, 11);                                // RealmHiddenAlert length + 1: no alert

            data.WriteBit(packet.BNSendWhisperUseV2Services);
            data.WriteBit(packet.BNSendGameDataUseV2Services);
            data.WriteBit(packet.CharacterSelectListModeRealmless);
            data.FlushBits();

            if (packet.EuropaTicketSystemStatus != null)
                packet.EuropaTicketSystemStatus.Write(data);

            data.WriteUInt32(packet.TokenPollTimeSeconds);
            data.WriteUInt32(packet.KioskSessionMinutes);
            data.WriteInt64(packet.TokenBalanceAmount);
            data.WriteInt32(packet.MaxCharactersPerRealm);
            data.WriteInt32(packet.LiveRegionCharacterCopySourceRegions.Count);
            data.WriteUInt32(packet.BpayStoreProductDeliveryDelay);
            data.WriteInt32(packet.ActiveCharacterUpgradeBoostType);
            data.WriteInt32(packet.ActiveClassTrialBoostType);
            data.WriteInt32(packet.MinimumExpansionLevel);
            data.WriteInt32(packet.MaximumExpansionLevel);
            data.WriteInt32(packet.ActiveSeason);                 // ContentSetID
            data.WriteInt32(packet.GameRuleValues.Count);
            data.WriteInt32(0);                                   // ActiveTimerunningSeasonID
            data.WriteInt32(0);                                   // RemainingTimerunningSeasonSeconds
            data.WriteInt16(packet.MaxPlayerNameQueriesPerPacket);
            data.WriteInt16(packet.PlayerNameQueryTelemetryInterval);
            data.WriteInt32(packet.PlayerNameQueryInterval);      // NotFoundCacheTimeSeconds
            data.WriteInt32(0);                                   // DebugTimeEvents
            data.WriteInt32(0);                                   // MostRecentTimeEventID
            data.WriteUInt32(0);                                  // EventRealmQueues

            foreach (var sourceRegion in packet.LiveRegionCharacterCopySourceRegions)
                data.WriteInt32(sourceRegion);

            foreach (var rulePair in packet.GameRuleValues)
                rulePair.WriteCataClassic(data);
        }
    }

    public bool BpayStoreAvailable; // NYI
    public bool BpayStoreDisabledByParentalControls; // NYI
    public bool CharUndeleteEnabled;
    public bool BpayStoreEnabled; // NYI
    public bool CommerceSystemEnabled; // NYI
    public bool Unk14; // NYI
    public bool WillKickFromWorld; // NYI
    public bool IsExpansionPreorderInStore; // NYI
    public bool KioskModeEnabled; // NYI
    public bool CompetitiveModeEnabled; // NYI
    public bool TrialBoostEnabled; // NYI
    public bool TokenBalanceEnabled; // NYI
    public bool LiveRegionCharacterListEnabled; // NYI
    public bool LiveRegionCharacterCopyEnabled; // NYI
    public bool LiveRegionAccountCopyEnabled; // NYI
    public bool LiveRegionKeyBindingsCopyEnabled = false;
    public bool Unknown901CheckoutRelated = false; // NYI
    public EuropaTicketConfig EuropaTicketSystemStatus = null!;
    public List<int> LiveRegionCharacterCopySourceRegions = new();
    public uint TokenPollTimeSeconds;     // NYI
    public long TokenBalanceAmount;     // NYI 
    public int MaxCharactersPerRealm;
    public uint BpayStoreProductDeliveryDelay;     // NYI
    public int ActiveCharacterUpgradeBoostType;     // NYI
    public int ActiveClassTrialBoostType;     // NYI
    public int MinimumExpansionLevel;
    public int MaximumExpansionLevel;
    public int ActiveSeason;
    public List<GameRuleValuePair> GameRuleValues = new List<GameRuleValuePair>();
    public short MaxPlayerNameQueriesPerPacket;
    public short PlayerNameQueryTelemetryInterval;
    public int PlayerNameQueryInterval;           // 4.4.2+; 3.4.3 writes 0 in its place
    public bool CharacterSelectListModeRealmless; // 4.4.2+
    public bool BNSendWhisperUseV2Services;       // 4.4.2+
    public bool BNSendGameDataUseV2Services;      // 4.4.2+
    public uint KioskSessionMinutes;
}

public class MOTD : ServerPacket, ISpanWritable
{
    public MOTD() : base(Opcode.SMSG_MOTD) { }

    public override void Write()
    {
        _worldPacket.WriteBits(Text.Count, 4);
        _worldPacket.FlushBits();

        foreach (var line in Text)
        {
            _worldPacket.WriteBits(line.GetByteCount(), 7);
            _worldPacket.FlushBits();
            _worldPacket.WriteString(line);
        }
    }

    // Cap for MOTD lines - reduced from 16 to 4 based on typical usage (1 byte = empty)
    private const int MaxLines = 4;
    // Cap per line (7 bits = max 128 chars)
    private const int MaxLineBytes = 128;
    // 4 bits(1) + per line: 7 bits(1) + text
    public int MaxSize => 1 + MaxLines * (1 + MaxLineBytes);

    public int WriteToSpan(Span<byte> buffer)
    {
        if (Text.Count > MaxLines)
            return -1;

        // Pre-validate all line lengths
        foreach (var line in Text)
        {
            if (Encoding.UTF8.GetByteCount(line) > MaxLineBytes)
                return -1;
        }

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)Text.Count, 4);
        writer.FlushBits();

        foreach (var line in Text)
        {
            int lineBytes = Encoding.UTF8.GetByteCount(line);
            writer.WriteBits((uint)lineBytes, 7);
            writer.FlushBits();
            writer.WriteString(line);
        }
        return writer.Position;
    }

    public List<string> Text = new List<string>();
}

public class SetTimeZoneInformation : ServerPacket, ISpanWritable
{
    public SetTimeZoneInformation() : base(Opcode.SMSG_SET_TIME_ZONE_INFORMATION) { }

    // The 3.4.3 client reads a third zone, ServerRegionalTZ: three 7-bit lengths, then three strings.
    // Sent with two, it took the first character of the first string as the third length and read
    // past the end of the packet (#361).
    private static bool HasRegionalZone => ForceV343ForTests ?? ModernVersion.IsWotLKClassicOrLater;

    // The test process never runs as V3_4_3; tests set this to reach the three-zone layout.
    internal static bool? ForceV343ForTests;

    public override void Write()
    {
        _worldPacket.WriteBits(ServerTimeTZ.GetByteCount(), 7);
        _worldPacket.WriteBits(GameTimeTZ.GetByteCount(), 7);
        if (HasRegionalZone)
            _worldPacket.WriteBits(ServerRegionalTZ.GetByteCount(), 7);
        // WriteString does not flush for an empty string, so empty zones would lose the lengths.
        _worldPacket.FlushBits();
        _worldPacket.WriteString(ServerTimeTZ);
        _worldPacket.WriteString(GameTimeTZ);
        if (HasRegionalZone)
            _worldPacket.WriteString(ServerRegionalTZ);
    }

    // Cap for timezone strings - 7 bits = max 128 chars each
    private const int MaxTZBytes = 64;
    // 21 bits (3 bytes) + 3 strings
    public int MaxSize => 3 + MaxTZBytes * 3;

    public int WriteToSpan(Span<byte> buffer)
    {
        int serverBytes = Encoding.UTF8.GetByteCount(ServerTimeTZ);
        int gameBytes = Encoding.UTF8.GetByteCount(GameTimeTZ);
        int regionalBytes = Encoding.UTF8.GetByteCount(ServerRegionalTZ);
        if (serverBytes > MaxTZBytes || gameBytes > MaxTZBytes || regionalBytes > MaxTZBytes)
            return -1;

        var writer = new SpanPacketWriter(buffer);
        writer.WriteBits((uint)serverBytes, 7);
        writer.WriteBits((uint)gameBytes, 7);
        if (HasRegionalZone)
            writer.WriteBits((uint)regionalBytes, 7);
        writer.FlushBits();
        writer.WriteString(ServerTimeTZ);
        writer.WriteString(GameTimeTZ);
        if (HasRegionalZone)
            writer.WriteString(ServerRegionalTZ);
        return writer.Position;
    }

    public string ServerTimeTZ = string.Empty;
    public string GameTimeTZ = string.Empty;
    public string ServerRegionalTZ = string.Empty;
}

public struct SavedThrottleObjectState
{
    public uint MaxTries;
    public uint PerMilliseconds;
    public uint TryCount;
    public uint LastResetTimeBeforeNow;

    public void Write(WorldPacket data)
    {
        data.WriteUInt32(MaxTries);
        data.WriteUInt32(PerMilliseconds);
        data.WriteUInt32(TryCount);
        data.WriteUInt32(LastResetTimeBeforeNow);
    }
}

public class EuropaTicketConfig
{
    public bool TicketsEnabled;
    public bool BugsEnabled;
    public bool ComplaintsEnabled;
    public bool SuggestionsEnabled;

    public SavedThrottleObjectState ThrottleState;

    public void Write(WorldPacket data)
    {
        data.WriteBit(TicketsEnabled);
        data.WriteBit(BugsEnabled);
        data.WriteBit(ComplaintsEnabled);
        data.WriteBit(SuggestionsEnabled);

        ThrottleState.Write(data);
    }
}
