// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal;
using Sportradar.OddsFeed.SDK.Api.Internal.ApiAccess;
using Sportradar.OddsFeed.SDK.Api.Internal.FeedAccess;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;

namespace Sportradar.OddsFeed.SDK.Api.Extended
{
    /// <summary>
    /// A <see cref="IUofSdk"/> implementation acting as an entry point to the odds feed service with possibility to get raw feed and api data
    /// </summary>
    public class UofSdkExtended : UofSdk, IUofSdkExtended
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private static readonly ILogger Log = SdkLoggerFactory.GetLoggerForExecution(typeof(UofSdkExtended));

        /// <summary>
        /// Occurs when any feed message arrives
        /// </summary>
        public event EventHandler<RawFeedMessageEventArgs> RawFeedMessageReceived;

        /// <summary>
        /// Occurs when data from Sports API arrives
        /// </summary>
        public event EventHandler<RawApiDataEventArgs> RawApiDataReceived;

        /// <summary>
        /// The data router manager
        /// </summary>
        private readonly IDataRouterManager _dataRouterManager;

        /// <summary>
        /// The feed message receiver
        /// </summary>
        private readonly IMessageReceiver _feedMessageReceiver;

        /// <summary>
        /// Constructs a new instance of the <see cref="IUofSdkExtended"/> class
        /// </summary>
        /// <param name="serviceProvider">A <see cref="IServiceProvider"/> instance including UofSdk configuration</param>
        public UofSdkExtended(IServiceProvider serviceProvider)
            : base(serviceProvider)
        {
            _dataRouterManager = ServiceProvider.GetRequiredService<IDataRouterManager>();

            _feedMessageReceiver = ServiceProvider.GetRequiredService<IMessageReceiver>();
        }

        private void OnRawApiDataReceived(object sender, RawApiDataEventArgs e)
        {
            if (RawApiDataReceived == null)
            {
                return;
            }

            using (var telemetryTracker = new TelemetryTracker(UofSdkTelemetry.FeedExtRawApiDataReceived, "message_type", e.RestMessage?.GetType().Name))
            {
                try
                {
                    RawApiDataReceived?.Invoke(sender, e);

                    Log.LogInformation("Dispatching raw api message for {Uri} took {Elapsed} ms", e.Uri, telemetryTracker.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "Error dispatching raw api data for {Uri}. Took {Elapsed} ms", e.Uri, telemetryTracker.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        private void OnRawFeedMessageReceived(object sender, RawFeedMessageEventArgs e)
        {
            if (RawFeedMessageReceived == null)
            {
                return;
            }

            using (var telemetryTracker = new TelemetryTracker(UofSdkTelemetry.FeedExtRawFeedDataReceived, "message_type", e.FeedMessage?.GetType().Name))
            {
                try
                {
                    RawFeedMessageReceived?.Invoke(sender, e);

                    var requestId = e.FeedMessage?.RequestId == null ? null : $" ({e.FeedMessage.RequestId.ToString()})";
                    Log.LogInformation("Dispatching raw feed message [{MessageInterest}]: {FeedMessageType} for event {EventId}{RequestId} took {Elapsed} ms",
                        e.MessageInterest,
                        e.FeedMessage?.GetType().Name,
                        e.FeedMessage?.EventId,
                        requestId,
                        telemetryTracker.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Log.LogError(ex, "Error dispatching raw feed message [{MessageInterest}] for {RoutingKey} and {EventId}. Took {Elapsed} ms",
                        e.MessageInterest,
                        e.RoutingKey,
                        e.FeedMessage?.EventId,
                        telemetryTracker.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        /// Closes the current <see cref="IUofSdkExtended" /> instance and disposes resources used by it
        /// </summary>
        public new void Close()
        {
            if (_dataRouterManager != null)
            {
                _dataRouterManager.RawApiDataReceived -= OnRawApiDataReceived;
            }
            if (_feedMessageReceiver != null)
            {
                _feedMessageReceiver.RawFeedMessageReceived -= OnRawFeedMessageReceived;
            }
            if (Sessions != null)
            {
                foreach (var session in Sessions)
                {
                    var s = (UofSession)session;
                    s.MessageReceiver.RawFeedMessageReceived -= OnRawFeedMessageReceived;
                }
            }

            base.Close();
        }

        /// <summary>
        /// Opens the current feed by opening all created sessions
        /// </summary>
        /// <exception cref="ObjectDisposedException"></exception>
        /// <exception cref="InvalidOperationException">
        /// The feed is already opened
        /// or
        /// The configuration is not valid
        /// </exception>
        /// <exception cref="CommunicationException">
        /// Connection to the REST-ful API failed, Probable Reason={Invalid or expired token}
        /// or
        /// Connection to the message broker failed, Probable Reason={Invalid or expired token}
        /// or
        /// </exception>
        public new void Open()
        {
            if (Sessions != null)
            {
                foreach (var session in Sessions)
                {
                    var s = (UofSession)session;
                    s.MessageReceiver.RawFeedMessageReceived += OnRawFeedMessageReceived;
                }
            }

            base.Open();

            if (_dataRouterManager != null)
            {
                _dataRouterManager.RawApiDataReceived += OnRawApiDataReceived;
            }
            if (_feedMessageReceiver != null)
            {
                _feedMessageReceiver.RawFeedMessageReceived += OnRawFeedMessageReceived;
            }
        }
    }
}
