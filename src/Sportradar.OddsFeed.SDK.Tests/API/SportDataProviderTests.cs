// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public class SportDataProviderTests
{
    private readonly Urn _competitorId;
    private readonly Mock<IProfileCache> _profileCacheMock;
    private readonly TestUofConfigurationSection _uofConfigurationSection;

    public SportDataProviderTests()
    {
        _competitorId = Urn.Parse("sr:competitor:1234");
        _uofConfigurationSection = new TestUofConfigurationSection();
        _uofConfigurationSection.SetSectionValue("accessToken", "test-token");
        _profileCacheMock = new Mock<IProfileCache>();
    }

    [Fact]
    public async Task GetCompetitorWhenCatchesExceptionShouldRethrowItIfDefaultUofConfigurationIsUsed()
    {
        var defaultConfiguration = new UofConfiguration(new UofConfigurationSectionProvider());

        var getCompetitor = SetupSportDataProviderToHandleExceptionBasedOnConfiguredStrategy(defaultConfiguration);

        await getCompetitor.Should().ThrowAsync<CommunicationException>();
    }

    [Fact]
    public async Task GetCompetitorWhenCatchesExceptionShouldNotRethrowWhenConfiguredToCatch()
    {
        var configurationSectionProviderMock = ConfigurationSectionProviderMock(ExceptionHandlingStrategy.Catch);

        var defaultConfiguration = new UofConfiguration(configurationSectionProviderMock);
        defaultConfiguration.UpdateFromAppConfigSection(true);

        var getCompetitor = SetupSportDataProviderToHandleExceptionBasedOnConfiguredStrategy(defaultConfiguration);

        await getCompetitor.Should().NotThrowAsync();
        (await getCompetitor()).Should().BeNull();
    }

    [Fact]
    public async Task GetCompetitorWhenCatchesExceptionShouldRethrowWhenConfiguredToThrow()
    {
        var configurationSectionProviderMock = ConfigurationSectionProviderMock(ExceptionHandlingStrategy.Throw);
        var defaultConfiguration = new UofConfiguration(configurationSectionProviderMock);
        defaultConfiguration.UpdateFromAppConfigSection(true);

        var getCompetitor = SetupSportDataProviderToHandleExceptionBasedOnConfiguredStrategy(defaultConfiguration);

        await getCompetitor.Should().ThrowAsync<CommunicationException>();
    }

    [Fact]
    public async Task GetCompetitorWhenCatchesExceptionShouldNotRethrowWhenExceptionHandlingStrategyIsCatchUsingConfigurationBuilder()
    {
        var customConfigurationBuilder = GetConfigurationBuilder();
        var defaultConfiguration = customConfigurationBuilder
                                  .SetExceptionHandlingStrategy(ExceptionHandlingStrategy.Catch)
                                  .Build();

        var getCompetitor = SetupSportDataProviderToHandleExceptionBasedOnConfiguredStrategy(defaultConfiguration);

        await getCompetitor.Should().NotThrowAsync();
        (await getCompetitor()).Should().BeNull();
    }

    [Fact]
    public async Task GetCompetitorWhenCatchesExceptionShouldRethrowWhenDefaultExceptionHandlingStrategyFromConfigurationIsLoaded()
    {
        var defaultConfiguration = GetConfigurationBuilder()
                                  .LoadFromConfigFile()
                                  .Build();

        var getCompetitor = SetupSportDataProviderToHandleExceptionBasedOnConfiguredStrategy(defaultConfiguration);

        await getCompetitor.Should().ThrowAsync<Exception>();
    }

    private Func<Task<ICompetitor>> SetupSportDataProviderToHandleExceptionBasedOnConfiguredStrategy(IUofConfiguration defaultConfiguration)
    {
        ConfigureProfileCacheToThrow();

        var sportDataProvider = CreateSportDataProvider(_profileCacheMock.Object, defaultConfiguration);

        var getCompetitor = () => sportDataProvider.GetCompetitorAsync(_competitorId);
        return getCompetitor;
    }

    private ICustomConfigurationBuilder GetConfigurationBuilder()
    {
        var configurationSectionProviderMock = new Mock<IUofConfigurationSectionProvider>();
        configurationSectionProviderMock.Setup(sp => sp.GetSection())
                                        .Returns(_uofConfigurationSection);

        return new TokenSetter(configurationSectionProviderMock.Object, new TestBookmakerDetailsProvider(), new TestProducersProvider())
              .SetAccessToken("test-token")
              .SelectCustom()
              .UseApiSsl(false)
              .SetMessagingHost("test")
              .SetMessagingPort(5672)
              .UseMessagingSsl(false)
              .SetMessagingPassword("test")
              .SetMessagingUsername("test")
              .SetDefaultLanguage(new CultureInfo("en"))
              .SetNodeId(1)
              .SetDesiredLanguages(new CultureInfo[] { new("en") })
              .SetApiHost("testapi");
    }

    private void ConfigureProfileCacheToThrow()
    {
        var languages = TestConfiguration.GetConfig().Languages;
        _profileCacheMock.Setup(m => m.GetCompetitorProfileAsync(It.IsAny<Urn>(), It.IsAny<IReadOnlyCollection<CultureInfo>>(), It.IsAny<bool>()))
                         .ThrowsAsync(new CommunicationException());
    }

    private static IUofConfigurationSectionProvider ConfigurationSectionProviderMock(ExceptionHandlingStrategy exceptionHandlingStrategy)
    {
        var configurationSectionProviderMock = new Mock<IUofConfigurationSectionProvider>();
        var configurationSectionMock = new Mock<IUofConfigurationSection>();
        configurationSectionProviderMock.Setup(m => m.GetSection()).Returns(configurationSectionMock.Object);
        configurationSectionMock.SetupGet(m => m.ExceptionHandlingStrategy).Returns(exceptionHandlingStrategy);
        configurationSectionMock.SetupGet(m => m.AccessToken).Returns("test-token");
        configurationSectionMock.SetupGet(m => m.ApiHost).Returns("test-host");
        configurationSectionMock.SetupGet(m => m.DefaultLanguage).Returns("en");

        return configurationSectionProviderMock.Object;
    }

    private static ISportDataProvider CreateSportDataProvider(IProfileCache profileCache, IUofConfiguration defaultConfiguration)
    {
        return new SportDataProviderBuilder()
              .WithAllMockedDependencies()
              .WithProfileCache(profileCache)
              .WithExceptionStrategy(defaultConfiguration.ExceptionHandlingStrategy)
              .Build();
    }
}
