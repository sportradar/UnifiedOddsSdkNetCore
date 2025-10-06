// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderBookmakerDetailsTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void ConfigurationIsBuiltForPredefinedEnvironmentWhenTokenSetterHasBookmakerDetailsThenBookmakerConfigurationIsNotEmpty(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object,
                                     BookmakerDetailsProvider,
                                     ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureHu)
                    .Build();

        config.BookmakerDetails.ShouldNotBeEmpty();
    }

    [Fact]
    public void ConfigurationIsBuiltForCustomEnvironmentWhenTokenSetterHasBookmakerDetailsThenBookmakerConfigurationIsNotEmpty()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object,
                                     BookmakerDetailsProvider,
                                     ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureHu)
                    .Build();

        config.BookmakerDetails.ShouldNotBeEmpty();
    }
}
