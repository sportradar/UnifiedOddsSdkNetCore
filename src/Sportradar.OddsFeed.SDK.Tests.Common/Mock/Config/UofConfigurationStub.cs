// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Config;

public class UofConfigurationStub : IUofConfiguration
{
    public string AccessToken
    {
        get;
    }
    public SdkEnvironment Environment
    {
        get;
    }
    public int NodeId
    {
        get;
    }
    public CultureInfo DefaultLanguage
    {
        get;
    }
    public List<CultureInfo> Languages
    {
        get;
    }
    public ExceptionHandlingStrategy ExceptionHandlingStrategy
    {
        get;
    }
    public IBookmakerDetails BookmakerDetails
    {
        get;
    }
    public IUofRabbitConfiguration Rabbit
    {
        get;
    }
    public IUofApiConfiguration Api
    {
        get;
    }
    public IUofProducerConfiguration Producer
    {
        get;
    }
    public IUofCacheConfiguration Cache
    {
        get;
    }
    public IUofAdditionalConfiguration Additional
    {
        get;
    }
    public IUofUsageConfiguration Usage
    {
        get;
    }

    [SuppressMessage("ReSharper", "ConvertToPrimaryConstructor", Justification = "Pipeline format fails with primary constructor")]
    public UofConfigurationStub(IUofConfiguration config, IUofUsageConfiguration usageConfig)
    {
        AccessToken = config.AccessToken;
        Environment = config.Environment;
        NodeId = config.NodeId;
        DefaultLanguage = config.DefaultLanguage;
        Languages = config.Languages;
        ExceptionHandlingStrategy = config.ExceptionHandlingStrategy;
        BookmakerDetails = config.BookmakerDetails;
        Rabbit = config.Rabbit;
        Api = config.Api;
        Producer = config.Producer;
        Cache = config.Cache;
        Additional = config.Additional;
        Usage = usageConfig;
    }
}
