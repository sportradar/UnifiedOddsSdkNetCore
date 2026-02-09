// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;
using Toxiproxy.Net;
using Toxiproxy.Net.Toxics;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Proxy;

public sealed class ProxiedRabbit : IAsyncDisposable
{
    private const int ToxiproxyPort = 8474;
    private const int ProxiedRabbitPort = 8089;

    private static readonly string ToxiProxyHost;
    private static readonly string RabbitHostWithinDockerNetwork;

    private readonly Toxiproxy.Net.Proxy _proxy;
    private readonly Connection _connection;

    static ProxiedRabbit()
    {
        ToxiProxyHost = GetToxiProxyHost();
        RabbitHostWithinDockerNetwork = GetRabbitHostWithinDockerNetwork();
    }

    private ProxiedRabbit(ProjectConfiguration rabbitConfiguration)
    {
        _connection = new Connection(ToxiProxyHost, ToxiproxyPort);
        _proxy = new Toxiproxy.Net.Proxy
        {
            Name = "rabbit-proxy",
            Enabled = true,
            Listen = $"0.0.0.0:{ProxiedRabbitPort}",
            Upstream = $"{RabbitHostWithinDockerNetwork}:{rabbitConfiguration.RabbitPort}"
        };
    }

    public async Task AddProxyToClient()
    {
        var client = _connection.Client();
        await client.AddAsync(_proxy);
    }

    public static ProxiedRabbit ProxyRabbit(ProjectConfiguration rabbitConfiguration)
    {
        return new ProxiedRabbit(rabbitConfiguration);
    }

    public ProjectConfiguration GetProxyRabbitConfiguration()
    {
        return ProjectConfigurationBuilder
              .Create()
              .UseTestRabbitConfiguration()
              .UseRabbitHost(ToxiProxyHost)
              .UseRabbitPort(ProxiedRabbitPort)
              .Build();
    }

    public async Task OpenConnection()
    {
        await _proxy.RemoveToxicAsync("timeout-toxic");
        await _proxy.UpdateAsync();
    }

    public async Task DropConnection()
    {
        var timeoutProxy = new TimeoutToxic
        {
            Name = "timeout-toxic",
            Attributes =
                {
                    Timeout = 10000
                },
            Toxicity = 1.0
        };
        await _proxy.AddAsync(timeoutProxy);
        await _proxy.UpdateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        try
        {
            await _proxy.DeleteAsync();
            _connection.Dispose();
        }
        catch (Exception ex)
        {
            throw new InvalidOperationException("Unable to delete the connection", ex);
        }
    }

    private static string GetToxiProxyHost()
    {
        var isLocal = Environment.GetEnvironmentVariable("CI") == null;
        return isLocal ? "localhost" : "toxiproxy";
    }

    private static string GetRabbitHostWithinDockerNetwork()
    {
        var isLocal = Environment.GetEnvironmentVariable("CI") == null;
        return isLocal ? "rabbitmq" : "toxiproxy";
    }
}
