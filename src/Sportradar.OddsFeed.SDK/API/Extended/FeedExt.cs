/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using App.Metrics;
using App.Metrics.Timer;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using System;
using Unity;

namespace Sportradar.OddsFeed.SDK.API.Extended
{
    /// <summary>
    /// A <see cref="IOddsFeed"/> implementation acting as an entry point to the odds feed service with possibility to get raw feed and api data
    /// </summary>
    public class FeedExt : Feed, IOddsFeedExt
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for execution logging
        /// </summary>
        private static readonly ILogger Log = SdkLoggerFactory.GetLoggerForExecution(typeof(FeedExt));

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
        private IDataRouterManager _dataRouterManager;

        /// <summary>
        /// The feed message receiver
        /// </summary>
        private IMessageReceiver _feedMessageReceiver;

        /// <summary>
        /// Constructs a new instance of the <see cref="Feed"/> class
        /// </summary>
        /// <param name="config">A <see cref="IOddsFeedConfiguration"/> instance representing feed configuration</param>
        /// <param name="loggerFactory">A <see cref="ILoggerFactory"/> used to create <see cref="ILogger"/> used within sdk</param>
        /// <param name="metricsRoot">A <see cref="IMetricsRoot"/> used to provide metrics within sdk</param>
        public FeedExt(IOddsFeedConfiguration config, ILoggerFactory loggerFactory = null, IMetricsRoot metricsRoot = null)
            : base(config, loggerFactory, metricsRoot)
        {
        }

        private void OnRawApiDataReceived(object sender, RawApiDataEventArgs e)
        {
            if (RawApiDataReceived == null)
            {
                return;
            }

            var timerOptionsOnRawApiDataReceived = new TimerOptions { Context = "FeedExt", Name = "OnRawApiDataReceived", MeasurementUnit = Unit.Items };
            using var t = MetricsRoot.Measure.Timer.Time(timerOptionsOnRawApiDataReceived, $"{e.RestMessage?.GetType().Name} - {e.Language}");
            try
            {
                RawApiDataReceived?.Invoke(sender, e);

                Log.LogInformation($"Dispatching raw api message for {e.Uri} took {t.Elapsed.TotalMilliseconds}ms.");
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"Error dispatching raw api data for {e.Uri}. Took {t.Elapsed.TotalMilliseconds}ms.");
            }
        }

        private void OnRawFeedMessageReceived(object sender, RawFeedMessageEventArgs e)
        {
            if (RawFeedMessageReceived == null)
            {
                return;
            }

            var timerOptionsOnRawFeedMessageReceived = new TimerOptions { Context = "FeedExt", Name = "OnRawFeedMessageReceived", MeasurementUnit = Unit.Items };
            using var t = MetricsRoot.Measure.Timer.Time(timerOptionsOnRawFeedMessageReceived, $"{e.MessageInterest} - {e.FeedMessage?.EventId}");
            try
            {
                RawFeedMessageReceived?.Invoke(sender, e);

                Log.LogInformation($"Dispatching raw feed message [{e.MessageInterest}]: {e.FeedMessage?.GetType().Name} for event {e.FeedMessage?.EventId} took {t.Elapsed.TotalMilliseconds}ms.");
            }
            catch (Exception ex)
            {
                Log.LogError(ex, $"Error dispatching raw feed message [{e.MessageInterest}] for {e.RoutingKey} and {e.FeedMessage?.EventId}. Took {t.Elapsed.TotalMilliseconds}ms.");
            }
        }

        /// <summary>
        /// Closes the current <see cref="Feed" /> instance and disposes resources used by it
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
                    var s = (OddsFeedSession)session;
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
                    var s = (OddsFeedSession)session;
                    s.MessageReceiver.RawFeedMessageReceived += OnRawFeedMessageReceived;
                }
            }

            base.Open();

            _dataRouterManager = UnityContainer.Resolve<IDataRouterManager>();
            if (_dataRouterManager != null)
            {
                _dataRouterManager.RawApiDataReceived += OnRawApiDataReceived;
            }

            _feedMessageReceiver = UnityContainer.Resolve<IMessageReceiver>();
            if (_feedMessageReceiver != null)
            {
                _feedMessageReceiver.RawFeedMessageReceived += OnRawFeedMessageReceived;
            }
        }
    }
}
