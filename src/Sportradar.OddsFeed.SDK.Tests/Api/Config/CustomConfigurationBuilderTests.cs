// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

// Ignore Spelling: Ssl
// ReSharper disable TooManyChainedReferences

using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

public class CustomConfigurationBuilderTests : ConfigurationBuilderSetup
{
    [Fact]
    public void ApiHostHasCorrectValue()
    {
        const string customApiHost = "custom_api_host";
        var config = BuildCustomConfig().SetApiHost(customApiHost).Build();

        Assert.Equal(customApiHost, config.Api.Host);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void ApiUseSslHasCorrectValue(bool useSsl)
    {
        var config = BuildCustomConfig().UseApiSsl(useSsl).Build();

        Assert.Equal(useSsl, config.Api.UseSsl);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void MessagingUseSslHasCorrectValue(bool useSsl)
    {
        var config = BuildCustomConfig().UseMessagingSsl(useSsl).Build();

        Assert.Equal(useSsl, config.Rabbit.UseSsl);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Fact]
    public void MessagingHostHasCorrectValue()
    {
        const string customMqHost = "custom_host";
        var config = BuildCustomConfig().SetMessagingHost(customMqHost).Build();

        Assert.Equal(customMqHost, config.Rabbit.Host);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Fact]
    public void MessagingUsernameHasCorrectValue()
    {
        const string customValue = "MyCustomValue";
        var config = BuildCustomConfig().SetMessagingUsername(customValue).Build();

        Assert.Equal(customValue, config.Rabbit.Username);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Fact]
    public void MessagingPasswordHasCorrectValue()
    {
        const string customValue = "MyCustomValue";
        var config = BuildCustomConfig().SetMessagingPassword(customValue).Build();

        Assert.Equal(customValue, config.Rabbit.Password);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }

    [Fact]
    public void MessagingPortHasCorrectValue()
    {
        const int port = 12345;
        var config = BuildCustomConfig().SetMessagingPort(port).Build();

        Assert.Equal(port, config.Rabbit.Port);
        ValidateRabbitConfigForEnvironment(config, config.Environment);
        ValidateApiConfigForEnvironment(config.Api, config.Environment);
    }
}
