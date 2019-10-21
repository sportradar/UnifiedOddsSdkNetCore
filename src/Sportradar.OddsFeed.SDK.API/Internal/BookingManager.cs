/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using System.Globalization;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Enums;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// The run-time implementation of the <see cref="IBookingManager"/> interface
    /// </summary>
    internal class BookingManager : IBookingManager
    {
        private readonly ILog _clientLog = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(BookingManager));
        private readonly ILog _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(BookingManager));

        private const string BookLiveOddsEventUrl = "{0}/v1/liveodds/booking-calendar/events/{1}/book";

        private readonly IOddsFeedConfigurationInternal _config;
        private readonly IDataPoster _dataPoster;
        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingManager"/> class
        /// </summary>
        /// <param name="config">The <see cref="IOddsFeedConfigurationInternal"/> representing feed configuration</param>
        /// <param name="dataPoster">A <see cref="IDataProvider{T}"/> used to make HTTP POST requests</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to save booking status</param>
        public BookingManager(IOddsFeedConfigurationInternal config, IDataPoster dataPoster, ICacheManager cacheManager)
        {
            Guard.Argument(config).NotNull();
            Guard.Argument(dataPoster).NotNull();
            Guard.Argument(cacheManager).NotNull();

            _config = config;
            _dataPoster = dataPoster;
            _cacheManager = cacheManager;
        }

        /// <summary>
        /// Books the live odds event associated with the provided {@link URN} identifier
        /// </summary>
        /// <param name="eventId">The event id</param>
        /// <returns><c>true</c> if event was successfully booked, <c>false</c> otherwise</returns>
        public bool BookLiveOddsEvent(URN eventId)
        {
            Guard.Argument(eventId).NotNull();

            try
            {
                _clientLog.Info($"Invoking BookingManager.BookLiveOddsEvent({eventId})");
                var postUrl = string.Format(BookLiveOddsEventUrl, _config.ApiBaseUri, eventId);

                var response = _dataPoster.PostDataAsync(new Uri(postUrl)).Result;

                _clientLog.Info($"BookingManager.bookLiveOddsEvent({eventId}) completed, status: {response.StatusCode}.");
                if (response.IsSuccessStatusCode)
                {
                    _cacheManager.SaveDto(eventId, eventId, CultureInfo.CurrentCulture, DtoType.BookingStatus, null);
                    return true;
                }

                _cacheManager.RemoveCacheItem(eventId, CacheItemType.SportEvent, "BookingManager");

                _executionLog.Warn($"Event[{eventId}] booking failed. API response code: {response.StatusCode}.");
            }
            catch (CommunicationException ce)
            {
                _executionLog.Warn($"Event[{eventId}] booking failed, CommunicationException: {ce.Message}");
            }
            catch (Exception e)
            {
                _executionLog.Warn($"Event[{eventId}] booking failed.", e);
            }

            return false;
        }
    }
}
