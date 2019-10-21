/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// A base class for classes used to dispatch messages
    /// </summary>
    public class EntityDispatcherBase
    {
        /// <summary>
        /// A <see cref="ILog"/> instance used for logging
        /// </summary>
        private static readonly ILog Log = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(EntityDispatcherBase));

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
                Log.Warn($"Cannot dispatch message {message.GetType().Name} because no event listeners are attached to associated event handler. Dropping message[{message}]");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                handler(this, eventArgs);
                stopwatch.Stop();
                Log.Info($"Successfully dispatched message[{message}]. Duration:{stopwatch.ElapsedMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                stopwatch.Stop();
                Log.Warn($"Event handler throw an exception while processing message[{message}]. Exception", ex);
            }
        }

        /// <summary>
        /// Raises the specified event
        /// </summary>
        /// <typeparam name="T">The type of the event arguments</typeparam>
        /// <param name="handler">A <see cref="EventHandler{T}"/> representing the event</param>
        /// <param name="eventArgs">Event arguments</param>
        /// <param name="messageName">The name of the event</param>
        protected void Dispatch<T>(EventHandler<T> handler, T eventArgs, string messageName)
        {
            if (handler == null)
            {
                var args = eventArgs == null
                               ? string.Empty
                               : eventArgs.GetType().Name;
                Log.Warn($"Cannot dispatch message {messageName} because no listeners are attached to associated event handler. EventArgs: {args}.");
                return;
            }

            var stopwatch = Stopwatch.StartNew();
            try
            {
                handler(this, eventArgs);
                stopwatch.Stop();
                Log.Info($"Successfully dispatched message {messageName}. Duration:{stopwatch.ElapsedMilliseconds} ms.");
            }
            catch (Exception ex)
            {
                Log.Warn($"Event handler throw an exception while processing message {messageName}. Exception", ex);
            }
        }
    }
}
