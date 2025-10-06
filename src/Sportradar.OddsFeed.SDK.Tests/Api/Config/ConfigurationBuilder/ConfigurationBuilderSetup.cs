// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Tests.Common;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class ConfigurationBuilderSetup
{
    internal readonly IBookmakerDetailsProvider BookmakerDetailsProvider;
    internal readonly IProducersProvider ProducersProvider;
    internal readonly Mock<IUofConfigurationSectionProvider> UofConfigurationSectionProviderMock;

    protected ConfigurationBuilderSetup()
    {
        UofConfigurationSectionProviderMock = new Mock<IUofConfigurationSectionProvider>();
        ProducersProvider = new TestProducersProvider();
        BookmakerDetailsProvider = new TestBookmakerDetailsProvider();
    }
}
