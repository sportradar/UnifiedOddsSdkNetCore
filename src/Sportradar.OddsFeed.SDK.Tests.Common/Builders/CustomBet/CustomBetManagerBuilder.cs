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
        var prebuiltBetsDataProvider = new DataProvider<PreBuiltBetsType, PrebuiltBetsDto>("/v1/custombet/prebuilt",
                                                                                           _dataFetcher,
                                                                                           new Deserializer<PreBuiltBetsType>(),
                                                                                           new PrebuiltBetsMapperFactory());
        _dataRouterManager = new DataRouterManagerBuilder()
                            .AddMockedDependencies()
                            .WithPrebuiltBetsDataProvider(prebuiltBetsDataProvider)
                            .WithConfiguration(_configuration)
                            .Build();

        return new CustomBetManager(_dataRouterManager,
                                    _configuration,
                                    _selectionBuilderFactory,
                                    _clientLog,
                                    _executionLog);
    }
}
