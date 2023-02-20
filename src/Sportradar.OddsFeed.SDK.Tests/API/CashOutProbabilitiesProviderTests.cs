using System;
using System.Collections.Generic;
using System.Globalization;
using System.Threading.Tasks;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.Internal.EntitiesImpl;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.REST;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class SportEvent : ISportEvent
    {
        public URN Id => new URN("prefix", "type", 1234567890L);
        public Task<string> GetNameAsync(CultureInfo culture) => throw new NotImplementedException();
        public Task<URN> GetSportIdAsync() => throw new NotImplementedException();
        public Task<DateTime?> GetScheduledTimeAsync() => throw new NotImplementedException();
        public Task<DateTime?> GetScheduledEndTimeAsync() => throw new NotImplementedException();
        public Task<bool?> GetStartTimeTbdAsync() => throw new NotImplementedException();
        public Task<URN> GetReplacedByAsync() => throw new NotImplementedException();
    }

    public abstract class CashOutProbabilitiesProviderTests : AutoMockerUnitTest
    {
        private readonly CashOutProbabilitiesProvider _sut;
        private readonly URN _urn = new URN("prefix", "type", 1234567890L);

        protected CashOutProbabilitiesProviderTests()
        {
            Mocker.Use<IEnumerable<CultureInfo>>(new List<CultureInfo> { new CultureInfo("en-US") });
            Mocker.Use(ExceptionHandlingStrategy.THROW);
            _sut = Mocker.CreateInstance<CashOutProbabilitiesProvider>();
        }

        public class WhenGetCashOutProbabilitiesIsSuccessful : CashOutProbabilitiesProviderTests
        {
            private readonly cashout _cashout = new cashout
            {
                event_id = "123"
            };

            private readonly CashOutProbabilities<SportEvent> _cashOutProbabilities =
                new CashOutProbabilities<SportEvent>(
                    new Mock<IMessageTimestamp>().Object,
                    new Mock<IProducer>().Object,
                    new SportEvent(),
                    0,
                    0,
                    null,
                    new Mock<INamedValuesProvider>().Object,
                    Array.Empty<byte>());

            public WhenGetCashOutProbabilitiesIsSuccessful()
            {
                Mocker.GetMock<IDataProvider<cashout>>()
                    .Setup(x => x.GetDataAsync(It.IsAny<string>()))
                    .ReturnsAsync(_cashout);

                Mocker.GetMock<IFeedMessageMapper>()
                    .Setup(x => x.MapCashOutProbabilities<SportEvent>(_cashout, It.IsAny<IEnumerable<CultureInfo>>(), It.IsAny<byte[]>()))
                    .Returns(_cashOutProbabilities);
            }

            [Fact]
            public async Task Then_it_returns_expected_data()
            {
                var result = await _sut.GetCashOutProbabilitiesAsync<SportEvent>(_urn);

                result.Should().NotBeNull();
                result.Event.Id.Should().BeEquivalentTo(_urn);
            }
        }

        public class WhenGetCashOutProbabilitiesReturnsNull : CashOutProbabilitiesProviderTests
        {
            public WhenGetCashOutProbabilitiesReturnsNull()
            {
                Mocker.GetMock<IDataProvider<cashout>>()
                    .Setup(x => x.GetDataAsync(It.IsAny<string>()))
                    .ReturnsAsync((cashout)null);
            }

            [Fact]
            public async Task Then_it_returns_expected_data()
            {
                var result = await _sut.GetCashOutProbabilitiesAsync<SportEvent>(_urn);

                result.Should().BeNull();
            }
        }
    }
}
