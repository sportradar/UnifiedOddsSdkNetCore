// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Api
{
    /// <summary>
    /// A base class for classes used to dispatch messages
    /// </summary>
    public class EntityDispatcherBase
    {
        /// <summary>
        /// A <see cref="ILogger"/> instance used for logging
        /// </summary>
        private static readonly ILogger Log = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(EntityDispatcherBase));

        /// <summary>
        /// Initializes a new instance of the <see cref="EntityDispatcherBase"/>
        /// </summary>
        protected EntityDispatcherBase()
        {
        }

        /// <summary>
        /// Dispatches the <c>feed message</c>
        /// </summary>
        /// <typeparam name="T">The type of the event arguments</typeparam>
        /// <param name="handler">Event delegate</param>
        /// <param name="eventArgs">Event arguments</param>
        /// <param name="message">A message to dispatch</param>
        protected void Dispatch<T>(EventHandler<T> handler, T eventArgs, FeedMessage message)
        {
            if (handler == null)
            {
                Log.LogWarning("Cannot dispatch message {MessageType} because no event listeners are attached to associated event handler. Dropping message [{FeedMessage}]", message.GetType().Name, message);
                return;
            }

            using (var tt = new TelemetryTracker(UofSdkTelemetry.DispatchFeedMessage, "msg_type", message.GetType().Name))
            {
                try
                {
                    handler(this, eventArgs);
                    Log.LogInformation("Successfully dispatched message[{FeedMessage}]. Duration: {Elapsed} ms", message, tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "Event handler throw an exception while processing message [{FeedMessage}]. Duration: {Elapsed} ms", message, tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                }
            }
        }

        /// <summary>
        /// Raises the specified sdk event (ProducerUp, ProducerDown, ...)
        /// </summary>
        /// <typeparam name="T">The type of the event arguments</typeparam>
        /// <param name="handler">A <see cref="EventHandler{T}"/> representing the event</param>
        /// <param name="eventArgs">Event arguments</param>
        /// <param name="eventHandlerName">The name of the event</param>
        /// <param name="producerId">The producer id</param>
        protected void Dispatch<T>(EventHandler<T> handler, T eventArgs, string eventHandlerName, int producerId)
        {
            if (handler == null)
            {
                var args = Equals(eventArgs, default(T))
                               ? string.Empty
                               : eventArgs.GetType().Name;
                Log.LogWarning("Cannot invoke event {EventHandler} because no listeners are attached to associated event handler. EventArgs: {Args}", eventHandlerName, args);
                return;
            }

            using (var tt = new TelemetryTracker(UofSdkTelemetry.DispatchSdkMessage))
            {
                try
                {
                    handler(this, eventArgs);
                    var prod = producerId == 0 ? string.Empty : $" for producer {producerId.ToString()}";
                    Log.LogInformation("Successfully invoked event {EventHandler}{ProducerInfo}. Duration: {Elapsed} ms", eventHandlerName, prod, tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                }
                catch (Exception ex)
                {
                    Log.LogWarning(ex, "Event handler {EventHandler} throw an exception. Duration: {Elapsed} ms", eventHandlerName, tt.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                }
            }
        }
    }
}
