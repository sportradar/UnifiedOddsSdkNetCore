// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Microsoft.Extensions.Configuration;
using Sportradar.OddsFeed.SDK.Tests.Common.Mock.Feed;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

public class ProjectConfigurationBuilder
{
    private readonly ProjectConfiguration _config;

    private ProjectConfigurationBuilder()
    {
        _config = new ProjectConfiguration();
    }

    public static ProjectConfigurationBuilder Create()
    {
        return new ProjectConfigurationBuilder();
    }

    public ProjectConfigurationBuilder UseTestRabbitConfiguration()
    {
        _config.SdkRabbitUsername = "testuser";
        _config.SdkRabbitPassword = "testpass";

        _config.DefaultAdminRabbitUserName = "guest";
        _config.DefaultAdminRabbitPassword = "guest";

        _config.RabbitHost = Environment.GetEnvironmentVariable("RABBITMQ_IP") ?? "localhost";
        _config.RabbitPort = 5672;

        _config.VirtualHostName = "/virtualhost";
        _config.UfExchange = "unifiedfeed";

        return this;
    }

    public ProjectConfigurationBuilder LoadConfigurationFromAppSettingsFile()
    {
        var configuration = new ConfigurationBuilder()
                            .SetBasePath(AppContext.BaseDirectory)
                            .AddJsonFile("appsettings.json", optional: true, reloadOnChange: true)
                            .Build();

        _config.Configuration = configuration;

        _config.SdkRabbitUsername =
            configuration["Rabbit::SdkUsername"] ?? _config.SdkRabbitUsername;

        _config.SdkRabbitPassword =
            configuration["Rabbit::SdkPassword"] ?? _config.SdkRabbitPassword;

        _config.DefaultAdminRabbitUserName =
            configuration["Rabbit::AdminUsername"] ?? _config.DefaultAdminRabbitUserName;

        _config.DefaultAdminRabbitPassword =
            configuration["Rabbit::AdminPassword"] ?? _config.DefaultAdminRabbitPassword;

        _config.RabbitHost =
            configuration["Rabbit::LocalHost"] ?? _config.RabbitHost;

        _config.RabbitPort =
            configuration.GetValue("Rabbit::Port", _config.RabbitPort);

        _config.VirtualHostName =
            configuration["Rabbit::VirtualHostName"] ?? _config.VirtualHostName;

        _config.UfExchange =
            configuration["Rabbit::ExchangeName"] ?? _config.UfExchange;

        return this;
    }

    public ProjectConfigurationBuilder UseRandomVirtualHost()
    {
        _config.VirtualHostName =
            $"/virtualhost_{Guid.NewGuid().ToString()[..8]}";

        return this;
    }

    public ProjectConfigurationBuilder UseRabbitHost(string rabbitHost)
    {
        _config.RabbitHost = rabbitHost;

        return this;
    }

    public ProjectConfigurationBuilder UseRabbitPort(int port)
    {
        _config.RabbitPort = port;

        return this;
    }

    public ProjectConfiguration Build()
    {
        return _config;
    }
}
