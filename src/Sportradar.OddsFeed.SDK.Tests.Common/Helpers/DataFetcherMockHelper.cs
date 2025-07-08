// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Helpers;

internal static class DataFetcherMockHelper
{
    public static Mock<ILogHttpDataFetcherFastFailing> GetDataFetcherProvidingSummary<T>(T sportEventSummary, Uri sportEventUri) where T : RestMessage
    {
        var matchDataFetcherMock = new Mock<ILogHttpDataFetcherFastFailing>();
        matchDataFetcherMock.Setup(fetcher => fetcher.GetDataAsync(sportEventUri))
                            .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(sportEventSummary));

        return matchDataFetcherMock;
    }

    public static Mock<ILogHttpDataFetcherFastFailing> GetDataFetcherProvidingSummaries<T>(T sportEventSummary, IEnumerable<Uri> sportEventUris) where T : RestMessage
    {
        var matchDataFetcherMock = new Mock<ILogHttpDataFetcherFastFailing>();
        foreach (var sportEventUri in sportEventUris)
        {
            matchDataFetcherMock.Setup(fetcher => fetcher.GetDataAsync(sportEventUri))
                                .ReturnsAsync(DeserializerHelper.SerializeToMemoryStream(sportEventSummary));
        }

        return matchDataFetcherMock;
    }

    public static Mock<ILogHttpDataFetcherFastFailing> GetDataFetcherThrowingCommunicationException(Uri sportEventUri)
    {
        var matchDataFetcherMock = new Mock<ILogHttpDataFetcherFastFailing>();
        matchDataFetcherMock.Setup(fetcher => fetcher.GetDataAsync(sportEventUri))
                             .ThrowsAsync(new CommunicationException());

        return matchDataFetcherMock;
    }
}
