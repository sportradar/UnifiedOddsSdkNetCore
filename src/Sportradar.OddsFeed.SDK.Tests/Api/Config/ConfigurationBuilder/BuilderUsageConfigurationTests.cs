// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderUsageConfigurationTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenBuiltForPredefinedEnvironmentThenUsageHasDefaultValues(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Usage.ShouldHaveDefaultValuesForEnvironment(environment);
    }

    [Fact]
    public void WhenBuiltForCustomEnvironmentThenUsageHasDefaultValues()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .Build();

        config.Usage.ShouldHaveDefaultValuesForEnvironment(SdkEnvironment.Custom);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration, true)]
    [InlineData(SdkEnvironment.Integration, false)]
    [InlineData(SdkEnvironment.Production, true)]
    [InlineData(SdkEnvironment.Production, false)]
    [InlineData(SdkEnvironment.GlobalIntegration, true)]
    [InlineData(SdkEnvironment.GlobalIntegration, false)]
    [InlineData(SdkEnvironment.GlobalProduction, true)]
    [InlineData(SdkEnvironment.GlobalProduction, false)]
    [InlineData(SdkEnvironment.Replay, true)]
    [InlineData(SdkEnvironment.Replay, false)]
    public void WhenUsageExportEnabledSetForPredefinedEnvironmentThenUsageReflectsFlag(SdkEnvironment environment, bool usageExportEnabled)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .EnableUsageExport(usageExportEnabled)
                    .Build();

        config.Usage.IsExportEnabled.ShouldBe(usageExportEnabled);
    }

    [Theory]
    [InlineData(true)]
    [InlineData(false)]
    public void WhenUsageExportEnabledSetForCustomEnvironmentThenUsageReflectsFlag(bool usageExportEnabled)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .EnableUsageExport(usageExportEnabled)
                    .Build();

        config.Usage.IsExportEnabled.ShouldBe(usageExportEnabled);
    }
}
