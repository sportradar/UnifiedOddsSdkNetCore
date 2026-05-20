// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.Endpoints;

public class BookmakerDetailsEndpoint
{
    public static bookmaker_details GetValidBookmakerDetails()
    {
        return new bookmaker_details
        {
            message = null,
            response_code = response_code.OK,
            response_codeSpecified = false,
            expire_at = DateTime.Now.AddDays(1),
            expire_atSpecified = true,
            bookmaker_id = 56,
            bookmaker_idSpecified = true,
            virtual_host = TestConsts.AnyVirtualHost
        };
    }
}
