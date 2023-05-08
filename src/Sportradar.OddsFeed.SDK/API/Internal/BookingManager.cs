/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
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
        private readonly ILogger _clientLog = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(BookingManager));
        private readonly ILogger _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(BookingManager));

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
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dataPoster = dataPoster ?? throw new ArgumentNullException(nameof(dataPoster));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        /// <summary>
        /// Books the live odds event associated with the provided {@link URN} identifier
        /// </summary>
        /// <param name="eventId">The event id</param>
        /// <returns><c>true</c> if event was successfully booked, <c>false</c> otherwise</returns>
        public bool BookLiveOddsEvent(URN eventId)
        {
            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            try
            {
                _clientLog.LogInformation($"Invoking BookingManager.BookLiveOddsEvent({eventId})");
                var postUrl = string.Format(BookLiveOddsEventUrl, _config.ApiBaseUri, eventId);

                var response = _dataPoster.PostDataAsync(new Uri(postUrl)).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    _clientLog.LogInformation($"BookingManager.bookLiveOddsEvent({eventId}) completed, status: {response.StatusCode}.");
                    _cacheManager.SaveDto(eventId, eventId, CultureInfo.CurrentCulture, DtoType.BookingStatus, null);
                    return true;
                }
                _cacheManager.RemoveCacheItem(eventId, CacheItemType.SportEvent, "BookingManager");
                var filteredResponse = SdkInfo.ExtractHttpResponseMessage(response.Content);
                _clientLog.LogWarning($"BookingManager.bookLiveOddsEvent({eventId}) completed, status: {response.StatusCode}, message: {filteredResponse}");
                throw new CommunicationException($"Failed booking event {eventId}", postUrl, response.StatusCode, filteredResponse, null);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogError(ce, $"Event[{eventId}] booking failed. API response code: {ce.ResponseCode}.");
                if (_config.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _executionLog.LogError(e, $"Event[{eventId}] booking failed.");
                if (_config.ExceptionHandlingStrategy == ExceptionHandlingStrategy.THROW)
                {
                    throw;
                }
            }

            return false;
        }
    }
}
