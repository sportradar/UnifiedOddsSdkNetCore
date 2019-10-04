/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using Common.Logging;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal;
using Sportradar.OddsFeed.SDK.Messages.EventArguments;
using Unity;

namespace Sportradar.OddsFeed.SDK.API.Extended
{
    /// <summary>
    /// A <see cref="IOddsFeed"/> implementation acting as an entry point to the odds feed service with possibility to get raw feed and api data
    /// </summary>
    public class FeedExt : Feed, IOddsFeedExt
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for execution logging
        /// </summary>
        private static readonly ILog Log = SdkLoggerFactory.GetLoggerForExecution(typeof(FeedExt));

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
        public FeedExt(IOddsFeedConfiguration config)
            : base(config)
        {
        }

        private void OnRawApiDataReceived(object sender, RawApiDataEventArgs e)
        {
            try
            {
                RawApiDataReceived?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                Log.Error($"Error dispatching raw api data for {e.Uri}", ex);
            }
        }

        private void OnRawFeedMessageReceived(object sender, RawFeedMessageEventArgs e)
        {
            try
            {
                RawFeedMessageReceived?.Invoke(sender, e);
            }
            catch (Exception ex)
            {
                Log.Error($"Error dispatching raw feed message for {e.RoutingKey} and {e.FeedMessage.EventId}", ex);
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
            if(Sessions != null)
            {
                foreach (var session in Sessions)
                {
                    var s = (OddsFeedSession) session;
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
