/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using RabbitMQ.Client.Events;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to dispatch global SDK events
    /// </summary>
    internal interface IGlobalEventDispatcher
    {
        /// <summary>
        /// Dispatches the <see cref="IProducerStatusChange"/> message
        /// </summary>
        /// <param name="producerStatusChange">The <see cref="IProducerStatusChange"/> instance to be dispatched</param>
        void DispatchProducerDown(IProducerStatusChange producerStatusChange);

        /// <summary>
        /// Dispatches the <see cref="IProducerStatusChange"/> message
        /// </summary>
        /// <param name="producerStatusChange">The <see cref="IProducerStatusChange"/> instance to be dispatched</param>
        void DispatchProducerUp(IProducerStatusChange producerStatusChange);

        /// <summary>
        /// Dispatches the information that the connection to the feed was lost
        /// </summary>
        void DispatchDisconnected();

        /// <summary>
        /// Dispatches the information that the requested event recovery completed
        /// <param name="requestId">The identifier of the recovery request</param>
        /// <param name="eventId">The associated event identifier</param>
        /// </summary>
        void DispatchEventRecoveryCompleted(long requestId, URN eventId);

        /// <summary>
        /// Dispatches the information that the exception was thrown in connection loop
        /// <param name="callbackExceptionEventArgs">The information about the exception</param>
        /// </summary>
        void DispatchConnectionException(CallbackExceptionEventArgs callbackExceptionEventArgs);
    }
}