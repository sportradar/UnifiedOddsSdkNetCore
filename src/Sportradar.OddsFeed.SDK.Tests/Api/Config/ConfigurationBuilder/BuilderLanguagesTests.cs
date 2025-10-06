// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Shouldly;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Sportradar.OddsFeed.SDK.Tests.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public class BuilderLanguagesTests : ConfigurationBuilderSetup
{
    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenMissingLanguageForPredefinedEnvironmentThenBuildThrows(SdkEnvironment environment)
    {
        var tokenSetterBuilder = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                                .SetAccessToken(TestConsts.AnyAccessToken)
                                .SelectEnvironment(environment);
        Should.Throw<InvalidOperationException>(() => tokenSetterBuilder.Build());
    }

    [Fact]
    public void WhenMissingLanguageForCustomEnvironmentThenBuildThrows()
    {
        var customBuilder = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                           .SetAccessToken(TestConsts.AnyAccessToken)
                           .SelectCustom()
                           .SetApiHost(TestConsts.AnyApiHost)
                           .SetMessagingHost(TestConsts.AnyRabbitHost);

        Should.Throw<InvalidOperationException>(() => customBuilder.Build());
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenDefaultLanguageSetForPredefinedEnvironmentThenConfigurationHasCorrectLanguage(SdkEnvironment environment)
    {
        var builder = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                     .SetAccessToken(TestConsts.AnyAccessToken)
                     .SelectEnvironment(environment)
                     .SetDefaultLanguage(TestConsts.CultureEn);

        var config = builder.Build();

        config.DefaultLanguage.ShouldBe(TestConsts.CultureEn);
        config.Languages.ShouldHaveSingleItem();
        config.Languages[0].ShouldBe(TestConsts.CultureEn);
    }

    [Fact]
    public void WhenDefaultLanguageSetForCustomEnvironmentThenConfigurationHasCorrectLanguage()
    {
        var builder = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                     .SetAccessToken(TestConsts.AnyAccessToken)
                     .SelectCustom()
                     .SetApiHost(TestConsts.AnyApiHost)
                     .SetMessagingHost(TestConsts.AnyRabbitHost)
                     .SetDefaultLanguage(TestConsts.CultureEn);

        var config = builder.Build();

        config.DefaultLanguage.ShouldBe(TestConsts.CultureEn);
        config.Languages.ShouldHaveSingleItem();
        config.Languages[0].ShouldBe(TestConsts.CultureEn);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenDesiredLanguagesSetForPredefinedEnvironmentThenConfigurationHasCorrectLanguages(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDesiredLanguages(TestConsts.Cultures3)
                    .Build();

        config.DefaultLanguage.ShouldBe(TestConsts.Cultures3.First());
        config.Languages.ShouldBeOfSize(3);
        config.Languages[0].ShouldBe(TestConsts.Cultures3.First());
    }

    [Fact]
    public void WhenDesiredLanguagesSetForCustomEnvironmentThenConfigurationHasCorrectLanguages()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDesiredLanguages(TestConsts.Cultures3)
                    .Build();

        config.DefaultLanguage.ShouldBe(TestConsts.Cultures3.First());
        config.Languages.ShouldBeOfSize(3);
        config.Languages[0].ShouldBe(TestConsts.Cultures3.First());
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenDesiredLanguagesContainDuplicatesForPredefinedEnvironmentThenOnlyUniqueLanguagesAreConfigured(SdkEnvironment environment)
    {
        IReadOnlyCollection<CultureInfo> theSameLanguageRepeated3Times = [TestConsts.CultureEn, TestConsts.CultureEn, TestConsts.CultureEn];

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDesiredLanguages(theSameLanguageRepeated3Times)
                    .Build();

        config.DefaultLanguage.ShouldBe(theSameLanguageRepeated3Times.First());
        config.Languages.ShouldHaveSingleItem();
        config.Languages[0].ShouldBe(theSameLanguageRepeated3Times.First());
    }

    [Fact]
    public void WhenDesiredLanguagesContainDuplicatesForCustomEnvironmentThenOnlyUniqueLanguagesAreConfigured()
    {
        IReadOnlyCollection<CultureInfo> theSameLanguageRepeated3Times = [TestConsts.CultureEn, TestConsts.CultureEn, TestConsts.CultureEn];

        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDesiredLanguages(theSameLanguageRepeated3Times)
                    .Build();

        config.DefaultLanguage.ShouldBe(theSameLanguageRepeated3Times.First());
        config.Languages.ShouldHaveSingleItem();
        config.Languages[0].ShouldBe(theSameLanguageRepeated3Times.First());
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenDefaultLanguageAndDesiredLanguagesSetForPredefinedEnvironmentThenDefaultIsFirstAndAllLanguagesPresent(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureHu)
                    .SetDesiredLanguages(TestConsts.Cultures2)
                    .Build();

        config.DefaultLanguage.ShouldBe(TestConsts.CultureHu);
        config.Languages.ShouldBeOfSize(3);
        config.Languages[0].ShouldBe(TestConsts.CultureHu);
        config.Languages.ShouldContain(TestConsts.CultureEn);
        config.Languages.ShouldContain(TestConsts.CultureEn);
    }

    [Fact]
    public void WhenDefaultLanguageAndDesiredLanguagesSetForCustomEnvironmentThenDefaultIsFirstAndAllLanguagesPresent()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureHu)
                    .SetDesiredLanguages(TestConsts.Cultures2)
                    .Build();

        config.DefaultLanguage.ShouldBe(TestConsts.CultureHu);
        config.Languages.ShouldBeOfSize(3);
        config.Languages[0].ShouldBe(TestConsts.CultureHu);
        config.Languages.ShouldContain(TestConsts.CultureEn);
        config.Languages.ShouldContain(TestConsts.CultureEn);
    }

    [Theory]
    [InlineData(SdkEnvironment.Integration)]
    [InlineData(SdkEnvironment.Production)]
    [InlineData(SdkEnvironment.GlobalIntegration)]
    [InlineData(SdkEnvironment.GlobalProduction)]
    [InlineData(SdkEnvironment.Replay)]
    public void WhenDefaultLanguageIncludedInDesiredLanguagesForPredefinedEnvironmentThenLanguagesAreUniqueAndDefaultLanguageIsPreserved(SdkEnvironment environment)
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectEnvironment(environment)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetDesiredLanguages(TestConsts.Cultures2)
                    .Build();

        config.DefaultLanguage.ShouldBe(TestConsts.CultureEn);
        config.Languages.ShouldBeOfSize(2);
        config.Languages[0].ShouldBe(TestConsts.CultureEn);
        config.Languages.ShouldContain(TestConsts.CultureEn);
        config.Languages.ShouldContain(TestConsts.CultureEn);
    }

    [Fact]
    public void WhenDefaultLanguageIncludedInDesiredLanguagesForCustomEnvironmentThenLanguagesAreUniqueAndDefaultLanguageIsPreserved()
    {
        var config = new TokenSetter(UofConfigurationSectionProviderMock.Object, BookmakerDetailsProvider, ProducersProvider)
                    .SetAccessToken(TestConsts.AnyAccessToken)
                    .SelectCustom()
                    .SetApiHost(TestConsts.AnyApiHost)
                    .SetMessagingHost(TestConsts.AnyRabbitHost)
                    .SetDefaultLanguage(TestConsts.CultureEn)
                    .SetDesiredLanguages(TestConsts.Cultures2)
                    .Build();

        config.DefaultLanguage.ShouldBe(TestConsts.CultureEn);
        config.Languages.ShouldBeOfSize(2);
        config.Languages[0].ShouldBe(TestConsts.CultureEn);
        config.Languages.ShouldContain(TestConsts.CultureEn);
        config.Languages.ShouldContain(TestConsts.CultureEn);
    }
}
