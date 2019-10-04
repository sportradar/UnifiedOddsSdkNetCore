/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Moq;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.API.Internal.Config;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Test.Shared
{
    internal class TestConfigurationInternal : OddsFeedConfigurationInternal
    {
        public TestConfigurationInternal(IOddsFeedConfiguration publicConfig, BookmakerDetailsDTO dto)
            : base(publicConfig, GetMock(dto))
        {
            Load();
        }

        private static BookmakerDetailsProvider GetMock(BookmakerDetailsDTO dto)
        {
            var mock = new Mock<BookmakerDetailsProvider>(dto.Id.ToString(), new TestDataFetcher(), new Deserializer<bookmaker_details>(), new BookmakerDetailsMapperFactory());
            mock.Setup(p => p.GetData(It.IsAny<string>())).Returns(dto);
            return mock.Object;
        }

        public static BookmakerDetailsDTO GetBookmakerDetails()
        {
            return new BookmakerDetailsDTO(
                RestMessageBuilder.BuildBookmakerDetails(
                    TestData.BookmakerId,
                    DateTime.Now,
                    response_code.OK,
                    TestData.VirtualHost),
                    TimeSpan.Zero);
        }

        public static TestConfigurationInternal GetConfig(IOddsFeedConfiguration publicConfig = null, BookmakerDetailsDTO dto = null)
        {
            var configBuilder = new TokenSetter(new TestSectionProvider(TestSection.DefaultSection))
                .SetAccessTokenFromConfigFile()
                .SelectIntegration()
                .LoadFromConfigFile()
                .SetInactivitySeconds(30)
                .SetSupportedLanguages(new[] { TestData.Culture });
            var config = configBuilder.Build();

            return new TestConfigurationInternal(publicConfig ?? config, dto ?? GetBookmakerDetails());
        }
    }
}
