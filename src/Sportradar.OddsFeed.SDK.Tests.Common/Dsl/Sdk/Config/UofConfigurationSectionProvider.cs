// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Moq;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Sdk.Config;

internal class UofConfigurationSectionProviderBuilder
{
    private IUofConfigurationSection _section;

    public static IUofConfigurationSectionProvider CreateWith(IUofConfigurationSection section)
    {
        return new UofConfigurationSectionProviderBuilder().With(section).Build();
    }

    public static IUofConfigurationSectionProvider CreateWithoutSection()
    {
        return new UofConfigurationSectionProviderBuilder().With(null).Build();
    }

    public static IUofConfigurationSectionProvider CreateWithBase()
    {
        return new UofConfigurationSectionProviderBuilder().With(UofConfigurationSections.OnlyRequiredFields()).Build();
    }

    private UofConfigurationSectionProviderBuilder With(IUofConfigurationSection section)
    {
        _section = section;
        return this;
    }

    public IUofConfigurationSectionProvider Build()
    {
        var mockProvider = new Mock<IUofConfigurationSectionProvider>();
        mockProvider.Setup(x => x.GetSection()).Returns(_section);
        return mockProvider.Object;
    }
}
