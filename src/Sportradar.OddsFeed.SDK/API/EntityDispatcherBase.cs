/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using System;
using System.Diagnostics;

namespace Sportradar.OddsFeed.SDK.API
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
        /// Dispatches the <code>message</code>
        /// </summary>
        /// <typeparam name="T">The type of the event arguments</typeparam>
        /// <param name="handler">Event delegate</param>
        /// <param name="eventArgs">Event arguments</param>
        /// <param name="message">A message to dispatch</param>
        protected void Dispatch<T>(EventHandler<T> handler, T eventArgs, FeedMessage message)
        {
            if (handler == null)
            {
                Log.LogWarning($"Cannot dispatch message {message.GetType().Name} because no event listeners are attached to associated event handler. Dropping message[{message}]");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                handler(this, eventArgs);
                stopwatch.Stop();
                Log.LogInformation($"Successfully dispatched message[{message}]. Duration: {stopwatch.ElapsedMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.LogWarning($"Event handler throw an exception while processing message[{message}]. Exception: {ex.Message}", ex);
            }
        }

        /// <summary>
        /// Raises the specified event
        /// </summary>
        /// <typeparam name="T">The type of the event arguments</typeparam>
        /// <param name="handler">A <see cref="EventHandler{T}"/> representing the event</param>
        /// <param name="eventArgs">Event arguments</param>
        /// <param name="messageName">The name of the event</param>
        /// <param name="producerId">The producer id</param>
        protected void Dispatch<T>(EventHandler<T> handler, T eventArgs, string messageName, int producerId)
        {
            if (handler == null)
            {
                var args = eventArgs == null
                               ? string.Empty
                               : eventArgs.GetType().Name;
                Log.LogWarning($"Cannot dispatch message {messageName} because no listeners are attached to associated event handler. EventArgs: {args}.");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                handler(this, eventArgs);
                stopwatch.Stop();
                var prod = producerId == 0 ? string.Empty : $" for producer {producerId}";
                Log.LogInformation($"Successfully dispatched message {messageName}{prod}. Duration: {stopwatch.ElapsedMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.LogWarning(ex, $"Event handler throw an exception while processing message {messageName}. Exception: {ex.Message}");
            }
        }
    }
}
