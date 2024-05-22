// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class TestBookmakerDetailsProvider : IBookmakerDetailsProvider
{
    public void LoadBookmakerDetails(UofConfiguration config)
    {
        var bookmakerDetails = new BookmakerDetails(GetBookmakerDetails());
        config.UpdateBookmakerDetails(bookmakerDetails, EnvironmentManager.GetApiHost(config.Environment));
    }

    public static void LoadBookmakerDetails(UofConfiguration config, BookmakerDetailsDto dto)
    {
        var bookmakerDetails = new BookmakerDetails(dto);
        config.UpdateBookmakerDetails(bookmakerDetails, EnvironmentManager.GetApiHost(config.Environment));
    }

    public static BookmakerDetailsDto GetBookmakerDetails()
    {
        return new BookmakerDetailsDto(
            RestMessageBuilder.BuildBookmakerDetails(
                TestData.BookmakerId,
                DateTime.Now.AddDays(1),
                response_code.OK,
                TestData.VirtualHost),
            TimeSpan.Zero);
    }
}
