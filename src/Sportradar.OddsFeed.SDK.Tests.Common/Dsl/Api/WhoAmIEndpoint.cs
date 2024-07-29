// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;

public class WhoAmIEndpoint
{
    private bookmaker_details _bookmakerDetails = new bookmaker_details();

    public static WhoAmIEndpoint Create()
    {
        return new WhoAmIEndpoint().WithExpiration(DateTime.Now.AddDays(1)).WithResponseCode(response_code.OK);
    }

    public static bookmaker_details CreateForAT()
    {
        return new WhoAmIEndpoint().WithBookmakerId(1).WithVirtualHost(RabbitManagement.VirtualHostName).WithExpiration(DateTime.Now.AddDays(1)).Build();
    }

    public WhoAmIEndpoint WithBookmakerDetails(Action<bookmaker_details> options)
    {
        _bookmakerDetails = new bookmaker_details();
        options(_bookmakerDetails);
        return this;
    }

    public WhoAmIEndpoint WithBookmakerId(int bookmakerId)
    {
        _bookmakerDetails.bookmaker_id = bookmakerId;
        _bookmakerDetails.bookmaker_idSpecified = true;
        return this;
    }

    public WhoAmIEndpoint WithVirtualHost(string virtualHost)
    {
        _bookmakerDetails.virtual_host = virtualHost;
        return this;
    }

    public WhoAmIEndpoint WithExpiration(DateTime expiration)
    {
        _bookmakerDetails.expire_at = expiration;
        _bookmakerDetails.expire_atSpecified = true;
        return this;
    }

    public WhoAmIEndpoint WithResponseCode(response_code responseCode)
    {
        _bookmakerDetails.response_code = responseCode;
        _bookmakerDetails.response_codeSpecified = true;
        return this;
    }

    public bookmaker_details Build()
    {
        return _bookmakerDetails;
    }
}
