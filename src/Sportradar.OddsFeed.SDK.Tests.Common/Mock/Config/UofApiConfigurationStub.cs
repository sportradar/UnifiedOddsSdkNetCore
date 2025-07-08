// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using Sportradar.OddsFeed.SDK.Api.Config;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Mock.Config;

public class UofApiConfigurationStub : IUofApiConfiguration
{
    public string Host
    {
        get;
    }
    public string BaseUrl
    {
        get;
    }
    public bool UseSsl
    {
        get;
    }
    public TimeSpan HttpClientTimeout
    {
        get;
        set;
    }
    public TimeSpan HttpClientRecoveryTimeout
    {
        get;
        set;
    }
    public TimeSpan HttpClientFastFailingTimeout
    {
        get;
        set;
    }
    public string ReplayHost
    {
        get;
    }
    public string ReplayBaseUrl
    {
        get;
    }
    public int MaxConnectionsPerServer
    {
        get;
    }

    [SuppressMessage("ReSharper", "ConvertToPrimaryConstructor", Justification = "Pipeline format fails with primary constructor")]
    public UofApiConfigurationStub(IUofApiConfiguration baseApiconfig)
    {
        Host = baseApiconfig.Host;
        BaseUrl = baseApiconfig.BaseUrl;
        UseSsl = baseApiconfig.UseSsl;
        HttpClientTimeout = baseApiconfig.HttpClientTimeout;
        HttpClientRecoveryTimeout = baseApiconfig.HttpClientRecoveryTimeout;
        HttpClientFastFailingTimeout = baseApiconfig.HttpClientFastFailingTimeout;
        ReplayHost = baseApiconfig.ReplayHost;
        ReplayBaseUrl = baseApiconfig.ReplayBaseUrl;
        MaxConnectionsPerServer = baseApiconfig.MaxConnectionsPerServer;
    }
}
