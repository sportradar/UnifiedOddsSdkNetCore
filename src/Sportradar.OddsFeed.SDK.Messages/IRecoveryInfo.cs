/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Messages
{
    /// <summary>
    /// Defines a contract for recovery info which contains data about last recovery attempt
    /// </summary>
    public interface IRecoveryInfo
    {
        /// <summary>
        /// Gets the after timestamp of the recovery or 0 if full recovery was done
        /// </summary>
        /// <value>The after</value>
        long After { get; }

        /// <summary>
        /// Gets the timestamp specifying when the recovery was initiated
        /// </summary>
        /// <value>The timestamp</value>
        long Timestamp { get; }

        /// <summary>
        /// Gets the request identifier
        /// </summary>
        /// <value>The request identifier</value>
        long RequestId { get; }

        /// <summary>
        /// Gets the response code of the recovery request
        /// </summary>
        /// <value>The response code of the recovery request</value>
        int ResponseCode { get; }

        /// <summary>
        /// Gets the response message of the recovery request
        /// </summary>
        /// <value> the response message of the recovery request</value>
        string ResponseMessage { get; }

        /// <summary>
        /// Gets the node identifier
        /// </summary>
        /// <value>The node identifier</value>
        int NodeId { get; }
    }
}