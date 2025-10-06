// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using System.Net;
using System.Net.Http;
using FluentAssertions;
using Moq;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Api;

public abstract class BookingManagerTests : AutoMockerUnitTest
{
    private readonly BookingManager _sut;
    private readonly Urn _urn = new Urn("prefix", "type", 1234567890L);
    private bool _result;

    protected BookingManagerTests()
    {
        var oddsFeedConfigMock = new Mock<IUofConfiguration>();
        oddsFeedConfigMock.Setup(x => x.Api.BaseUrl).Returns("https://address.com/api/");

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
        public void Then_it_returns_true()
        {
            _result.Should().BeTrue();
        }
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
        public void Then_it_returns_false()
        {
            _result.Should().BeFalse();
        }
    }
}
