// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common.Builders;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Sdk.Config;

public static class UofConfigurationSections
{
    internal static UofConfigurationSectionBuilder GetBuilderWithOnlyRequiredFields()
    {
        return UofConfigurationSectionBuilder.Create()
                                             .WithAccessToken(TestConsts.AnyAccessToken)
                                             .WithDefaultLanguage(TestConsts.CultureEn.TwoLetterISOLanguageName)
                                             .WithEnvironment(SdkEnvironment.Integration);
    }

    internal static IUofConfigurationSection OnlyRequiredFields()
    {
        return GetBuilderWithOnlyRequiredFields().Build();
    }
}
