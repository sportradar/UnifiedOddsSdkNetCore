using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public abstract class BookingManagerTests : AutoMockerUnitTest
    {
        private readonly BookingManager _sut;
        private readonly URN _urn = new URN("prefix", "type", 1234567890L);
        private bool _result;

        protected BookingManagerTests()
        {
            var oddsFeedConfigMock = new Mock<IOddsFeedConfigurationInternal>();
            oddsFeedConfigMock.Setup(x => x.ApiBaseUri).Returns("https://address.com/api/");

            Mocker.Use(oddsFeedConfigMock.Object);

            _sut = Mocker.CreateInstance<BookingManager>();
        }

        public class WhenBookLiveOddsEventIsSuccessful : BookingManagerTests
        {
            public WhenBookLiveOddsEventIsSuccessful()
            {
                Mocker.GetMock<IDataPoster>()
                    .Setup(x => x.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()))
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.OK));

                _result = _sut.BookLiveOddsEvent(_urn);
            }

            [Fact]
            public void Then_event_id_is_saved_to_cache()
            {
                Mocker.GetMock<ICacheManager>()
                    .Verify(x => x.SaveDto(_urn, _urn, CultureInfo.CurrentCulture, DtoType.BookingStatus, null),
                        Times.Once);
            }

            [Fact]
            public void Then_it_returns_true() => _result.Should().BeTrue();
        }

        public class WhenBookLiveOddsEventFails : BookingManagerTests
        {
            public WhenBookLiveOddsEventFails()
            {
                Mocker.GetMock<IDataPoster>()
                    .Setup(x => x.PostDataAsync(It.IsAny<Uri>(), It.IsAny<HttpContent>()))
                    .ReturnsAsync(new HttpResponseMessage(HttpStatusCode.InternalServerError));

                Assert.Throws<CommunicationException>(() => _result = _sut.BookLiveOddsEvent(_urn));
            }

            [Fact]
            public void Then_event_id_is_removed_from_cache()
            {
                Mocker.GetMock<ICacheManager>()
                    .Verify(x => x.RemoveCacheItem(_urn, CacheItemType.SportEvent, "BookingManager"), Times.Once);
            }

            [Fact]
            public void Then_it_returns_false() => _result.Should().BeFalse();
        }
    }
}
