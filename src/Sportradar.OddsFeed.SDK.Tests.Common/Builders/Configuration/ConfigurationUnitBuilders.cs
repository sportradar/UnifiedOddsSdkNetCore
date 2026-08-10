// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.Logging.Abstractions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using BookmakerDetailsProvider = Sportradar.OddsFeed.SDK.Api.Internal.Config.BookmakerDetailsProvider;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.Configuration;

internal static class ConfigurationUnitBuilders
{
    public static ConfigurationUnitBuilder StubbingOutDataProviders()
    {
        return new ConfigurationUnitBuilder();
    }
}

internal class ConfigurationUnitBuilder
{
    private IDataFetcher _bookmakerDetailsFetcher;
    private IDataFetcher _producersFetcher;

    public ConfigurationUnitBuilder WithBookmakerDetailsFetcher(IDataFetcher dataFetcher)
    {
        _bookmakerDetailsFetcher = dataFetcher;
        return this;
    }

    public ConfigurationUnitBuilder WithProducersFetcher(IDataFetcher dataFetcher)
    {
        _producersFetcher = dataFetcher;
        return this;
    }

    public TokenSetter BuildTokenSetter()
    {
        var bookmakerDetailsDataProvider = new DataProvider<bookmaker_details, BookmakerDetailsDto>("{0}/v1/users/whoami.xml",
                                                                                                    _bookmakerDetailsFetcher ?? ThrowingDataFetcher(),
                                                                                                    new Deserializer<bookmaker_details>(),
                                                                                                    new BookmakerDetailsMapperFactory());

        var producersFetcher = _producersFetcher ?? ThrowingDataFetcher();
        var producersDataProvider = new NonMappingDataProvider<producers>("/v1/descriptions/producers.xml",
                                                                          producersFetcher,
                                                                          new Deserializer<producers>());

        return new TokenSetter(
            new Mock<IUofConfigurationSectionProvider>().Object,
            _ => new BookmakerDetailsProvider(bookmakerDetailsDataProvider, new NullLogger<BookmakerDetailsProvider>()),
            config => new ProducersProvider(producersDataProvider, config));
    }

    private static IDataFetcher ThrowingDataFetcher()
    {
        var mock = new Mock<IDataFetcher>();
        mock.Setup(f => f.GetData(It.IsAny<Uri>()))
            .Throws(new InvalidOperationException("No producers fetcher was configured on ConfigurationUnitBuilder"));
        return mock.Object;
    }
}
