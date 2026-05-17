namespace HermesProxy.World.Enums;

// Mirrors TrinityCore wotlk_classic DBCEnums.h:1691 PlayerInteractionType.
// Wire-tagged Int32 carried by SMSG_NPC_INTERACTION_OPEN_RESULT (opcode 10378
// on V3_4_3), which is the modern replacement for legacy SMSG_SHOW_BANK and
// related "open X UI" packets.
public enum PlayerInteractionType : int
{
    None                        = 0,
    TradePartner                = 1,
    Item                        = 2,
    Gossip                      = 3,
    QuestGiver                  = 4,
    Merchant                    = 5,
    TaxiNode                    = 6,
    Trainer                     = 7,
    Banker                      = 8,
    AlliedRaceDetailsGiver      = 9,
    GuildBanker                 = 10,
    Registrar                   = 11,
    Vendor                      = 12,
    PetitionVendor              = 13,
    GuildTabardVendor           = 14,
    TalentMaster                = 15,
    SpecializationMaster        = 16,
    MailInfo                    = 17,
    SpiritHealer                = 18,
    AreaSpiritHealer            = 19,
    Binder                      = 20,
    Auctioneer                  = 21,
    StableMaster                = 22,
    BattleMaster                = 23,
    Transmogrifier              = 24,
    LFGDungeon                  = 25,
    VoidStorageBanker           = 26,
    BlackMarketAuctioneer       = 27,
    AdventureMap                = 28,
    WorldMap                    = 29,
}
