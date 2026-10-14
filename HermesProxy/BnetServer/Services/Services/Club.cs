using Bgs.Protocol;
using Bgs.Protocol.Club.V1;
using Framework.Constants;
using HermesProxy.World;
using Membership = Bgs.Protocol.Club.V1.Membership;

namespace BNetServer.Services;

/// <summary>
/// The club services the 4.4.2 client asks the world server for, answered for the one club the
/// proxy has: the player's guild (<see cref="GuildClub"/>). Communities and groups do not exist
/// here, as on a TrinityCore cata_classic realm, which answers their club types the same way.
/// </summary>
/// <remarks>
/// Every other client is answered as the unimplemented-service stub answered it, OK and empty:
/// clubs were never offered to it.
/// </remarks>
public partial class BnetServices
{
    private GuildClub Club => GetSession().GameState.GuildClub;

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubMembershipService, (uint)ClubMembershipServiceMethods.Subscribe)]
    BattlenetRpcErrorCode HandleClubMembershipSubscribe(Membership.SubscribeRequest request, Membership.SubscribeResponse response)
    {
        if (GuildClub.Served)
            Club.SubscribeMemberships(response);
        return BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubMembershipService, (uint)ClubMembershipServiceMethods.Unsubscribe)]
    BattlenetRpcErrorCode HandleClubMembershipUnsubscribe(Membership.UnsubscribeRequest request, NoData response)
    {
        if (GuildClub.Served)
            Club.UnsubscribeMemberships();
        return BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.GetClubType)]
    BattlenetRpcErrorCode HandleClubGetClubType(GetClubTypeRequest request, GetClubTypeResponse response)
    {
        if (!GuildClub.Served)
            return BattlenetRpcErrorCode.Ok;
        if (!GuildClub.IsGuildClubType(request.Type))
            return BattlenetRpcErrorCode.NotImplemented;

        response.Type = GuildClub.GuildClubType();
        return BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.Subscribe)]
    BattlenetRpcErrorCode HandleClubSubscribe(SubscribeRequest request, NoData response)
    {
        return GuildClub.Served ? Club.Subscribe(request.ClubId) : BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.Unsubscribe)]
    BattlenetRpcErrorCode HandleClubUnsubscribe(UnsubscribeRequest request, NoData response)
    {
        if (GuildClub.Served)
            Club.Unsubscribe(request.ClubId);
        return BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.GetMembers)]
    BattlenetRpcErrorCode HandleClubGetMembers(GetMembersRequest request, GetMembersResponse response)
    {
        return GuildClub.Served ? Club.GetMembers(request.ClubId, response) : BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.GetStreams)]
    BattlenetRpcErrorCode HandleClubGetStreams(GetStreamsRequest request, GetStreamsResponse response)
    {
        return GuildClub.Served ? Club.GetStreams(request.ClubId, response) : BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.SubscribeStream)]
    BattlenetRpcErrorCode HandleClubSubscribeStream(SubscribeStreamRequest request, NoData response)
    {
        if (!GuildClub.Served)
            return BattlenetRpcErrorCode.Ok;
        if (!Club.IsGivenClub(request.ClubId) || request.StreamId.Count == 0 || !GuildClub.IsGuildStream(request.StreamId[0]))
            return BattlenetRpcErrorCode.Denied;
        return BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.UnsubscribeStream)]
    BattlenetRpcErrorCode HandleClubUnsubscribeStream(UnsubscribeStreamRequest request, NoData response)
    {
        return BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.SetStreamFocus)]
    BattlenetRpcErrorCode HandleClubSetStreamFocus(SetStreamFocusRequest request, NoData response)
    {
        return BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.AdvanceStreamViewTime)]
    BattlenetRpcErrorCode HandleClubAdvanceStreamViewTime(AdvanceStreamViewTimeRequest request, NoData response)
    {
        return BattlenetRpcErrorCode.Ok;
    }

    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.CreateMessage)]
    BattlenetRpcErrorCode HandleClubCreateMessage(CreateMessageRequest request, CreateMessageResponse response)
    {
        return GuildClub.Served
            ? Club.CreateMessage(request.ClubId, request.StreamId, request.Options?.Content ?? "", response)
            : BattlenetRpcErrorCode.Ok;
    }

    // A stock realm keeps no stream history either.
    [Service(ServiceRequirement.LoggedIn, OriginalHash.ClubService, (uint)ClubServiceMethods.GetStreamHistory)]
    BattlenetRpcErrorCode HandleClubGetStreamHistory(GetStreamHistoryRequest request, GetStreamHistoryResponse response)
    {
        return GuildClub.Served ? BattlenetRpcErrorCode.RpcNotImplemented : BattlenetRpcErrorCode.Ok;
    }
}
