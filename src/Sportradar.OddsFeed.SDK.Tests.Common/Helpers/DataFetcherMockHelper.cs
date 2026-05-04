// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.IO;
using System.Text;
using System.Xml.Serialization;
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

    public static IDataFetcher GetDataFetcherThrowingCommunicationExceptionAsync()
    {
        var dataFetcherMock = new Mock<IDataFetcher>();
        dataFetcherMock.Setup(fetcher => fetcher.GetDataAsync(It.IsAny<Uri>(), It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<IReadOnlyDictionary<string, string>>()))
                       .ThrowsAsync(new CommunicationException());

        return dataFetcherMock.Object;
    }

    public static IDataFetcher GetDataFetcherThrowingExceptionAsync()
    {
        var dataFetcherMock = new Mock<IDataFetcher>();
        dataFetcherMock.Setup(fetcher => fetcher.GetDataAsync(It.IsAny<Uri>(), It.IsAny<IReadOnlyDictionary<string, string>>(), It.IsAny<IReadOnlyDictionary<string, string>>()))
                       .ThrowsAsync(new InvalidOperationException());

        return dataFetcherMock.Object;
    }

    public static IDataFetcher GetDataFetcherProvidingPrebuiltBetsForAnyRequest(PreBuiltBetsType prebuiltBets)
    {
        return GetDataFetcherProvidingPrebuiltBets(prebuiltBets, _ => true, _ => true);
    }

    public static IDataFetcher GetDataFetcherProvidingPrebuiltBetsForQuery(PreBuiltBetsType prebuiltBets, Func<IReadOnlyDictionary<string, string>, bool> queryPredicate)
    {
        return GetDataFetcherProvidingPrebuiltBets(prebuiltBets, queryPredicate, _ => true);
    }

    public static IDataFetcher GetDataFetcherProvidingPrebuiltBetsForHeaders(PreBuiltBetsType prebuiltBets, Func<IReadOnlyDictionary<string, string>, bool> headersPredicate)
    {
        return GetDataFetcherProvidingPrebuiltBets(prebuiltBets, _ => true, headersPredicate);
    }

    private static IDataFetcher GetDataFetcherProvidingPrebuiltBets(PreBuiltBetsType prebuiltBets,
        Func<IReadOnlyDictionary<string, string>, bool> queryPredicate,
        Func<IReadOnlyDictionary<string, string>, bool> headersPredicate)
    {
        var dataFetcherMock = new Mock<IDataFetcher>();
        dataFetcherMock
            .Setup(fetcher => fetcher.GetDataAsync(
                It.IsAny<Uri>(),
                It.Is<IReadOnlyDictionary<string, string>>(query => queryPredicate(query)),
                It.Is<IReadOnlyDictionary<string, string>>(headers => headersPredicate(headers))))
            .ReturnsAsync(GetPrebuiltBetsStream(prebuiltBets));

        return dataFetcherMock.Object;
    }

    private static Stream GetPrebuiltBetsStream(PreBuiltBetsType prebuiltBetsType)
    {
        var serializer = new XmlSerializer(typeof(PreBuiltBetsType));
        var memoryStream = new MemoryStream();

        using (var writer = new StreamWriter(memoryStream, new UTF8Encoding(false), leaveOpen: true))
        {
            serializer.Serialize(writer, prebuiltBetsType);
        }

        memoryStream.Position = 0;
        return memoryStream;
    }
}
