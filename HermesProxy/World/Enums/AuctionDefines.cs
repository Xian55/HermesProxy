using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace HermesProxy.World.Enums;

public enum AuctionHouseAction
{
    Sell   = 0, // ERR_AUCTION_STARTED
    Cancel = 1, // ERR_AUCTION_REMOVED
    Bid    = 2  // ERR_AUCTION_BID_PLACED
}

public enum AuctionHouseError
{
    Ok                = 0,
    Inventory         = 1,
    InternalError     = 2,  // ERR_AUCTION_DATABASE_ERROR - default
    NotEnoughMoney    = 3,  // ERR_NOT_ENOUGH_MONEY
    ItemNotFound      = 4,  // ERR_ITEM_NOT_FOUND
    HigherBid         = 5,  // ERR_AUCTION_HIGHER_BID
    IncrementBind     = 7,  // ERR_AUCTION_BID_INCREMENT
    CantBidYouAuction = 10, // ERR_AUCTION_BID_OWN
    Restricted        = 13  // ERR_RESTRICTED_ACCOUNT
}

/// <summary>
/// The Filters field of the 4.4.2 client's CMSG_AUCTION_BROWSE_QUERY (TrinityCore cata_classic
/// AuctionHouseFilterMask).
/// </summary>
[Flags]
public enum AuctionHouseFilterMask : uint
{
    None                     = 0x0000,
    UncollectedOnly          = 0x0002,
    UsableOnly               = 0x0004,
    CurrentExpansionOnly     = 0x0008,
    UpgradesOnly             = 0x0010,
    ExactMatch               = 0x0020,
    PoorQuality              = 0x0040,
    CommonQuality            = 0x0080,
    UncommonQuality          = 0x0100,
    RareQuality              = 0x0200,
    EpicQuality              = 0x0400,
    LegendaryQuality         = 0x0800,
    ArtifactQuality          = 0x1000,
    LegendaryCraftedItemOnly = 0x2000,
}

/// <summary>What a 4.4.2 SMSG_AUCTION_LIST_BUCKETS_RESULT answers.</summary>
public enum AuctionHouseBrowseMode : byte
{
    Search       = 0,
    SpecificKeys = 1,
}

/// <summary>How the 4.4.2 client shows a SMSG_AUCTION_LIST_ITEMS_RESULT.</summary>
public enum AuctionHouseListType : byte
{
    Commodities = 1,
    Items       = 2,
}
