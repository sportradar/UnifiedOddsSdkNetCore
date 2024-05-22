// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Api;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

/// <summary>
/// Class TestProducersProvider for setting default producers list
/// Implements the <see cref="IProducersProvider" />
/// </summary>
/// <seealso cref="IProducersProvider" />
public class TestProducersProvider : IProducersProvider
{
    /// <summary>
    /// Gets the producers
    /// </summary>
    /// <value>The producers</value>
    public List<IProducer> Producers { get; }

    /// <summary>
    /// Initializes a new instance of the <see cref="TestProducersProvider"/> class. Loads default list of producers.
    /// </summary>
    internal TestProducersProvider(IDataProvider<producers> apiDataProvider = null)
    {
        apiDataProvider ??= new StubProducerProvider();

        const int maxInactivitySeconds = 20;
        const int maxRecoveryTime = 3600;

        var apiProducers = apiDataProvider.GetData("some-language-code");

        Producers = new List<IProducer>();
        foreach (var producer in apiProducers.producer)
        {
            Producers.Add(new Producer((int)producer.id, producer.name, producer.description, producer.api_url, producer.active, maxInactivitySeconds, maxRecoveryTime, producer.scope, producer.stateful_recovery_window_in_minutes));
        }
    }

    /// <summary>
    /// Gets the available producers from api (default setup used in most tests)
    /// </summary>
    /// <returns>A list of <see cref="IProducer"/></returns>
    public IReadOnlyCollection<IProducer> GetProducers()
    {
        return Producers;
    }
}
