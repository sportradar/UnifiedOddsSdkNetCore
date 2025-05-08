// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers;

internal static class DataFetcherMockHelper
{
    public static ILogHttpDataFetcherFastFailing GetDataFetcherProvidingSummary<T>(T sportEventSummary, Uri sportEventUri) where T : RestMessage
    {
        var matchDataFetcherMock = new Mock<ILogHttpDataFetcherFastFailing>();
        matchDataFetcherMock.Setup(fetcher => fetcher.GetDataAsync(sportEventUri))
                            .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(sportEventSummary));

        return matchDataFetcherMock.Object;
    }

    public static ILogHttpDataFetcherFastFailing GetDataFetcherThrowingCommunicationException(Uri sportEventUri)
    {
        var matchDataFetcherMock = new Mock<ILogHttpDataFetcherFastFailing>();
        matchDataFetcherMock.Setup(fetcher => fetcher.GetDataAsync(sportEventUri))
                             .ThrowsAsync(new CommunicationException());

        return matchDataFetcherMock.Object;
    }
}
