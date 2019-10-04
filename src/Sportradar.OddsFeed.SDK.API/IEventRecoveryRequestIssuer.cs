/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.API.Contracts;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract implemented by classes used to issue event message recovery requests to the feed
    /// </summary>
    [ContractClass(typeof(EventRecoveryRequestIssuerContract))]
    public interface IEventRecoveryRequestIssuer
    {
        /// <summary>
        /// Asynchronously requests messages for the specified sport event and returns a request number used when issuing the request
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="eventId">A <see cref="URN"/> specifying the sport event for which to request the messages</param>
        /// <returns> <see cref="Task{HttpStatusCode}"/> representing the async operation</returns>
        Task<long> RecoverEventMessagesAsync(IProducer producer, URN eventId);

        /// <summary>
        /// Asynchronously requests stateful messages for the specified sport event and returns a request number used when issuing the request
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery</param>
        /// <param name="eventId">A <see cref="URN"/> specifying the sport event for which to request the messages</param>
        /// <returns> <see cref="Task{HttpStatusCode}"/> representing the async operation</returns>
        Task<long> RecoverEventStatefulMessagesAsync(IProducer producer, URN eventId);
    }
}
