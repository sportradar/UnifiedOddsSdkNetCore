/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to issue message recovery requests to the feed.
    /// </summary>
    public interface IRecoveryRequestIssuer
    {
        /// <summary>
        /// Asynchronously requests a recovery for the specified producer for changes which occurred after specified time
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery.</param>
        /// <param name="dateAfter">Specifies the time after which the changes should be recovered.</param>
        /// <param name="nodeId">The id of the node where recovery messages will be processed</param>
        /// <returns><see cref="Task{Long}"/> representing a asynchronous method.
        /// Once the execution is complete it provides the request id associated with the recovery</returns>
        Task<long> RequestRecoveryAfterTimestampAsync(IProducer producer, DateTime dateAfter, int nodeId);

        /// <summary>
        /// Asynchronously requests a recovery of current odds for specific producer
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to make the recovery.</param>
        /// <param name="nodeId">The id of the node where recovery messages will be processed</param>
        /// <returns><see cref="Task{Long}"/> representing a asynchronous method.
        /// Once the execution is complete it provides the request id associated with the recovery</returns>
        Task<long> RequestFullOddsRecoveryAsync(IProducer producer, int nodeId);
    }
}