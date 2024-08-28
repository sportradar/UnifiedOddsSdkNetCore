// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    /// <summary>
    /// The run-time implementation of the <see cref="IBookingManager"/> interface
    /// </summary>
    internal class BookingManager : IBookingManager
    {
        private readonly ILogger _clientLog = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(BookingManager));
        private readonly ILogger _executionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(BookingManager));

        private const string BookLiveOddsEventUrl = "{0}/v1/liveodds/booking-calendar/events/{1}/book";

        private readonly IUofConfiguration _config;
        private readonly IDataPoster _dataPoster;
        private readonly ICacheManager _cacheManager;

        /// <summary>
        /// Initializes a new instance of the <see cref="BookingManager"/> class
        /// </summary>
        /// <param name="config">The <see cref="IUofConfiguration"/> representing feed configuration</param>
        /// <param name="dataPoster">A <see cref="IDataProvider{T}"/> used to make HTTP POST requests</param>
        /// <param name="cacheManager">A <see cref="ICacheManager"/> used to save booking status</param>
        public BookingManager(IUofConfiguration config, IDataPoster dataPoster, ICacheManager cacheManager)
        {
            _config = config ?? throw new ArgumentNullException(nameof(config));
            _dataPoster = dataPoster ?? throw new ArgumentNullException(nameof(dataPoster));
            _cacheManager = cacheManager ?? throw new ArgumentNullException(nameof(cacheManager));
        }

        /// <summary>
        /// Books the live odds event associated with the provided <see cref="Urn"/> identifier
        /// </summary>
        /// <param name="eventId">The event id</param>
        /// <returns><c>true</c> if event was successfully booked, <c>false</c> otherwise</returns>
        public bool BookLiveOddsEvent(Urn eventId)
        {
            if (eventId == null)
            {
                throw new ArgumentNullException(nameof(eventId));
            }

            try
            {
                _clientLog.LogInformation("Invoking BookingManager.BookLiveOddsEvent({SportEventId})", eventId);
                var postUrl = string.Format(BookLiveOddsEventUrl, _config.Api.BaseUrl, eventId);

                var response = _dataPoster.PostDataAsync(new Uri(postUrl)).GetAwaiter().GetResult();

                if (response.IsSuccessStatusCode)
                {
                    _clientLog.LogInformation("BookingManager.bookLiveOddsEvent({SportEventId}) completed, status: {ResponseStatusCode}", eventId, response.StatusCode);
                    _cacheManager.SaveDto(eventId, eventId, CultureInfo.CurrentCulture, DtoType.BookingStatus, null);
                    return true;
                }
                _cacheManager.RemoveCacheItem(eventId, CacheItemType.SportEvent, "BookingManager");
                var filteredResponse = SdkInfo.ExtractHttpResponseMessage(response.Content);
                _clientLog.LogWarning("BookingManager.bookLiveOddsEvent({SportEventId}) completed, status: {ResponseStatusCode}, message: {FilteredResponse}", eventId, response.StatusCode, filteredResponse);
                throw new CommunicationException($"Failed booking event {eventId}", postUrl, response.StatusCode, filteredResponse, null);
            }
            catch (CommunicationException ce)
            {
                _executionLog.LogError(ce, "Event[{SportEventId}] booking failed. API response code: {ErrorResponseCode}", eventId, ce.ResponseCode);
                if (_config.ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }
            catch (Exception e)
            {
                _executionLog.LogError(e, "Event[{SportEventId}] booking failed", eventId);
                if (_config.ExceptionHandlingStrategy == ExceptionHandlingStrategy.Throw)
                {
                    throw;
                }
            }

            return false;
        }
    }
}
