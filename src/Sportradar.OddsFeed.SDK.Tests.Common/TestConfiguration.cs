// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

internal static class TestConfiguration
{
    public static IUofConfiguration GetConfig(BookmakerDetailsDto dto = null, ExceptionHandlingStrategy exceptionHandlingStrategy = ExceptionHandlingStrategy.Throw)
    {
        var configBuilder = new TokenSetter(new TestSectionProvider(TestSection.GetDefaultSection()), new TestBookmakerDetailsProvider(), new TestProducersProvider())
            .SetAccessTokenFromConfigFile()
            .SelectEnvironment(SdkEnvironment.Integration)
            .LoadFromConfigFile()
            .SetInactivitySeconds(30)
            .SetHttpClientTimeout(30)
            .SetHttpClientRecoveryTimeout(20)
            .SetDesiredLanguages(TestData.Cultures3)
            .SetExceptionHandlingStrategy(exceptionHandlingStrategy);

        var config = configBuilder.Build();

        if (dto != null)
        {
            ((UofConfiguration)config).UpdateBookmakerDetails(new BookmakerDetails(dto), config.Api.Host);
        }

        return config;
    }
}
