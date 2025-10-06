// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;

// ReSharper disable ConvertToPrimaryConstructor

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class TestBookmakerDetailsProvider : IBookmakerDetailsProvider
{
    private readonly DateTime _expiredAt;
    public TestBookmakerDetailsProvider()
        : this(DateTime.Now.AddDays(1))
    {
    }
    public TestBookmakerDetailsProvider(DateTime expiredAt)
    {
        _expiredAt = expiredAt;
    }

    public void LoadBookmakerDetails(UofConfiguration config)
    {
        var bookmakerDetails = new BookmakerDetails(GetBookmakerDetails(_expiredAt));
        config.UpdateBookmakerDetails(bookmakerDetails, EnvironmentManager.GetApiHost(config.Environment));
    }

    public static void LoadBookmakerDetails(UofConfiguration config, BookmakerDetailsDto dto)
    {
        var bookmakerDetails = new BookmakerDetails(dto);
        config.UpdateBookmakerDetails(bookmakerDetails, EnvironmentManager.GetApiHost(config.Environment));
    }

    private static BookmakerDetailsDto GetBookmakerDetails(DateTime expiredAt)
    {
        var bookmakerDetails = RestMessageBuilder.BuildBookmakerDetails(TestConsts.AnyBookmakerId, expiredAt, response_code.OK, TestConsts.AnyVirtualHost);

        return new BookmakerDetailsDto(bookmakerDetails, TimeSpan.Zero);
    }

    public static BookmakerDetailsDto GetBookmakerDetails()
    {
        return GetBookmakerDetails(DateTime.Now.AddDays(1));
    }
}
