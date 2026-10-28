// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Builders;

public class UofConfigurationSectionBuilder
{
    private readonly Mock<IUofConfigurationSection> _mock = new Mock<IUofConfigurationSection>();

    private UofConfigurationSectionBuilder()
    {
        // Set default values based on UofConfigurationSection defaults
        _mock.SetupGet(x => x.RabbitUseSsl).Returns(true);
        _mock.SetupGet(x => x.RabbitPort).Returns(0);
        _mock.SetupGet(x => x.ApiUseSsl).Returns(true);
        _mock.SetupGet(x => x.ExceptionHandlingStrategy).Returns(ExceptionHandlingStrategy.Throw);
        _mock.SetupGet(x => x.NodeId).Returns(0);
        _mock.SetupGet(x => x.Environment).Returns(SdkEnvironment.Integration);
    }

    public static UofConfigurationSectionBuilder Create()
    {
        return new UofConfigurationSectionBuilder();
    }

    public UofConfigurationSectionBuilder WithAccessToken(string accessToken)
    {
        _mock.SetupGet(x => x.AccessToken).Returns(accessToken);
        return this;
    }

    public UofConfigurationSectionBuilder WithRabbitHost(string rabbitHost)
    {
        _mock.SetupGet(x => x.RabbitHost).Returns(rabbitHost);
        return this;
    }

    public UofConfigurationSectionBuilder WithRabbitVirtualHost(string rabbitVirtualHost)
    {
        _mock.SetupGet(x => x.RabbitVirtualHost).Returns(rabbitVirtualHost);
        return this;
    }

    public UofConfigurationSectionBuilder WithRabbitPort(int rabbitPort)
    {
        _mock.SetupGet(x => x.RabbitPort).Returns(rabbitPort);
        return this;
    }

    public UofConfigurationSectionBuilder WithRabbitUsername(string rabbitUsername)
    {
        _mock.SetupGet(x => x.RabbitUsername).Returns(rabbitUsername);
        return this;
    }

    public UofConfigurationSectionBuilder WithRabbitPassword(string rabbitPassword)
    {
        _mock.SetupGet(x => x.RabbitPassword).Returns(rabbitPassword);
        return this;
    }

    public UofConfigurationSectionBuilder WithRabbitUseSsl(bool rabbitUseSsl)
    {
        _mock.SetupGet(x => x.RabbitUseSsl).Returns(rabbitUseSsl);
        return this;
    }

    public UofConfigurationSectionBuilder WithApiHost(string apiHost)
    {
        _mock.SetupGet(x => x.ApiHost).Returns(apiHost);
        return this;
    }

    public UofConfigurationSectionBuilder WithApiUseSsl(bool apiUseSsl)
    {
        _mock.SetupGet(x => x.ApiUseSsl).Returns(apiUseSsl);
        return this;
    }

    public UofConfigurationSectionBuilder WithDefaultLanguage(string defaultLanguage)
    {
        _mock.SetupGet(x => x.DefaultLanguage).Returns(defaultLanguage);
        return this;
    }

    public UofConfigurationSectionBuilder WithLanguages(string languages)
    {
        _mock.SetupGet(x => x.Languages).Returns(languages);
        return this;
    }

    public UofConfigurationSectionBuilder WithExceptionHandlingStrategy(ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        _mock.SetupGet(x => x.ExceptionHandlingStrategy).Returns(exceptionHandlingStrategy);
        return this;
    }

    public UofConfigurationSectionBuilder WithDisabledProducers(string disabledProducers)
    {
        _mock.SetupGet(x => x.DisabledProducers).Returns(disabledProducers);
        return this;
    }

    public UofConfigurationSectionBuilder WithNodeId(int nodeId)
    {
        _mock.SetupGet(x => x.NodeId).Returns(nodeId);
        return this;
    }

    public UofConfigurationSectionBuilder WithEnvironment(SdkEnvironment environment)
    {
        _mock.SetupGet(x => x.Environment).Returns(environment);
        return this;
    }

    internal IUofConfigurationSection Build()
    {
        return _mock.Object;
    }
}
