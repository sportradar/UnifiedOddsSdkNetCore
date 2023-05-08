using System;
using System.Linq;
using System.Net;
using System.Net.Http;
using Sportradar.OddsFeed.SDK.API;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Entities.REST
{
    public class BookingManagerTests
    {
        private readonly TestDataFetcher _dataFetcher;
        private readonly IBookingManager _bookingManager;
        private readonly IBookingManager _bookingManagerCatch;
        private const string BookingUrl = "booking-calendar";

        public BookingManagerTests()
        {
            _dataFetcher = new TestDataFetcher();
            var cacheManager = new CacheManager();
            var config1 = TestConfigurationInternal.GetConfig();
            _bookingManager = new BookingManager(config1, _dataFetcher, cacheManager);
            var config2 = TestConfigurationInternal.GetConfig();
            config2.ExceptionHandlingStrategy = ExceptionHandlingStrategy.CATCH;
            _bookingManagerCatch = new BookingManager(config2, _dataFetcher, cacheManager);
        }

        [Fact]
        public void BookingManagerSetupCorrectly()
        {
            Assert.NotNull(_bookingManager);
            Assert.NotNull(_bookingManagerCatch);
        }

        [Fact]
        public void BookLiveOddsEventNullEventIdThrows()
        {
            Assert.Throws<ArgumentNullException>(() => _bookingManager.BookLiveOddsEvent(null));
        }

        [Fact]
        public void BookLiveOddsEventSucceeded()
        {
            const string xml = "/booking-calendar/";
            _dataFetcher.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>(xml, 1, new HttpResponseMessage(HttpStatusCode.Accepted)));
            var isBooked = _bookingManager.BookLiveOddsEvent(ScheduleData.MatchId);
            Assert.True(isBooked);
        }

        [Fact]
        public void BookLiveOddsEventCallsCorrectEndpoint()
        {
            const string xml = "/booking-calendar/";
            _dataFetcher.PostResponses.Add(new Tuple<string, int, HttpResponseMessage>(xml, 1, new HttpResponseMessage(HttpStatusCode.Accepted)));
            Assert.Empty(_dataFetcher.CalledUrls);

            _bookingManager.BookLiveOddsEvent(ScheduleData.MatchId);
            Assert.Single(_dataFetcher.CalledUrls);
            Assert.Contains(BookingUrl, _dataFetcher.CalledUrls.First());
        }

        [Fact]
        public void BookLiveOddsEventCallRespectExceptionHandlingStrategyCatch()
        {
            const string exceptionMessage = "Not found for id";
            const string xml = "/booking-calendar/";
            _dataFetcher.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

            var isBooked = _bookingManagerCatch.BookLiveOddsEvent(ScheduleData.MatchId);
            Assert.False(isBooked);
            Assert.Single(_dataFetcher.CalledUrls);
            Assert.Contains(BookingUrl, _dataFetcher.CalledUrls.First());
        }

        [Fact]
        public void BookLiveOddsEventCallRespectExceptionHandlingStrategyThrow()
        {
            const string exceptionMessage = "Not found for id";
            const string xml = "/booking-calendar/";
            _dataFetcher.UriExceptions.Add(new Tuple<string, Exception>(xml, new CommunicationException(exceptionMessage, xml, HttpStatusCode.NotFound, null)));

            var isBooked = false;
            var exception = Assert.Throws<CommunicationException>(() => isBooked = _bookingManager.BookLiveOddsEvent(ScheduleData.MatchId));
            Assert.False(isBooked);
            Assert.NotNull(exception);
            Assert.Equal(HttpStatusCode.NotFound, exception.ResponseCode);
            Assert.Equal(exceptionMessage, exception.Message);
            Assert.Equal(xml, exception.Url);
            Assert.Single(_dataFetcher.CalledUrls);
            Assert.Contains(BookingUrl, _dataFetcher.CalledUrls.First());
        }
    }
}
