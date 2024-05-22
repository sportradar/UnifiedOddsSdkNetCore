// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal class StubProducerProvider : IDataProvider<producers>
{
    public IDataFetcher DataFetcher { get; }

    public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

    public producers Producers { get; }

    public StubProducerProvider(producers producers = null)
    {
        DataFetcher = null;
        Producers = producers ?? GetProducers();
    }

    public Task<producers> GetDataAsync(string languageCode)
    {
        return Task.FromResult(Producers);
    }

    public Task<producers> GetDataAsync(params string[] identifiers)
    {
        return Task.FromResult(Producers);
    }

    public producers GetData(string languageCode)
    {
        return Producers;
    }

    public producers GetData(params string[] identifiers)
    {
        return Producers;
    }

    public producers GetProducers()
    {
        var producerList = new List<producer>
        {
            LoadProducer(1, "LO", "Live Odds", "https://stgapi.betradar.com/v1/liveodds/", true, "live", 600),
            LoadProducer(3, "Ctrl", "Betradar Ctrl", "https://stgapi.betradar.com/v1/pre/", true, "prematch", 4320),
            LoadProducer(4, "BetPal", "BetPal", "https://stgapi.betradar.com/v1/betpal/", true, "live", 4320),
            LoadProducer(5, "PremiumCricket", "Premium Cricket", "https://stgapi.betradar.com/v1/premium_cricket/", true, "live|prematch", 4320),
            LoadProducer(6, "VF", "Virtual football", "https://stgapi.betradar.com/v1/vf/", true, "virtual", 180),
            LoadProducer(7, "WNS", "Numbers Betting", "https://stgapi.betradar.com/v1/wns/", true, "prematch", 4320),
            LoadProducer(8, "VBL", "Virtual Basketball League", "https://stgapi.betradar.com/v1/vbl/", false, "virtual", 180)
        };

        var resultProducers = new producers();
        resultProducers.location = "some-location";
        resultProducers.producer = producerList.ToArray();

        return resultProducers;
    }

    private producer LoadProducer(int producerId, string producerName, string producerDesc, string prodApiUrl, bool isActive, string scope, int recoveryWindow)
    {
        return new producer
        {
            id = producerId,
            name = producerName,
            description = producerDesc,
            api_url = prodApiUrl,
            active = isActive,
            scope = scope,
            stateful_recovery_window_in_minutes = recoveryWindow
        };
    }
}
