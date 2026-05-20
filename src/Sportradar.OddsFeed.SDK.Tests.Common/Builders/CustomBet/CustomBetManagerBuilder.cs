// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.CustomBet;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders.CustomBet;

internal sealed class CustomBetManagerBuilder
{
    private IDataRouterManager _dataRouterManager;
    private IUofConfiguration _configuration;
    private ICustomBetSelectionBuilderFactory _selectionBuilderFactory;
    private IDataFetcher _dataFetcher;
    private IDataPoster _dataPoster;
    private ILogger _clientLog;
    private ILogger _executionLog;

    private CustomBetManagerBuilder()
    {
    }

    public static CustomBetManagerBuilder Create()
    {
        return new CustomBetManagerBuilder();
    }

    public CustomBetManagerBuilder WithConfiguration(IUofConfiguration configuration)
    {
        _configuration = configuration;
        return this;
    }

    public CustomBetManagerBuilder WithSelectionBuilderFactory(ICustomBetSelectionBuilderFactory factory)
    {
        _selectionBuilderFactory = factory;
        return this;
    }

    public CustomBetManagerBuilder WithDataFetcher(IDataFetcher dataFetcher)
    {
        _dataFetcher = dataFetcher;
        return this;
    }

    public CustomBetManagerBuilder WithDataPoster(IDataPoster dataPoster)
    {
        _dataPoster = dataPoster;
        return this;
    }

    public CustomBetManagerBuilder WithClientLogger(ILogger clientLog)
    {
        _clientLog = clientLog;
        return this;
    }

    public CustomBetManagerBuilder WithExecutionLogger(ILogger executionLog)
    {
        _executionLog = executionLog;
        return this;
    }

    public ICustomBetManager Build()
    {
        _dataRouterManager = new DataRouterManagerBuilder()
                            .AddMockedDependencies()
                            .WithPrebuiltBetsDataProvider(GetPrebuiltBetsDataProviderWith(_dataFetcher))
                            .WithCalculateProbabilityProvider(GetCalculateProbabilityProviderWith(_dataPoster))
                            .WithCalculateProbabilityFilteredProvider(GetCalculateProbabilityFilteredProviderWith(_dataPoster))
                            .WithConfiguration(_configuration).Build();

        return new CustomBetManager(_dataRouterManager,
                                    _configuration,
                                    _selectionBuilderFactory,
                                    _clientLog,
                                    _executionLog);
    }

    private static CalculateProbabilityFilteredProvider GetCalculateProbabilityFilteredProviderWith(IDataPoster dataPoster)
    {
        return new CalculateProbabilityFilteredProvider(
                                                        "/v1/custombet/calculate-filter",
                                                        dataPoster,
                                                        new Deserializer<FilteredCalculationResponseType>(),
                                                        new CalculationFilteredMapperFactory());
    }

    private static CalculateProbabilityProvider GetCalculateProbabilityProviderWith(IDataPoster dataPoster)
    {
        return new CalculateProbabilityProvider("/v1/custombet/calculate",
                                                dataPoster,
                                                new Deserializer<CalculationResponseType>(),
                                                new CalculationMapperFactory());
    }

    private static DataProvider<PreBuiltBetsType, PrebuiltBetsDto> GetPrebuiltBetsDataProviderWith(IDataFetcher dataFetcher)
    {
        return new DataProvider<PreBuiltBetsType, PrebuiltBetsDto>("/v1/custombet/prebuilt",
                                                                   dataFetcher,
                                                                   new Deserializer<PreBuiltBetsType>(),
                                                                   new PrebuiltBetsMapperFactory());
    }
}
