/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// The recovery info
    /// </summary>
    /// <seealso cref="IRecoveryInfo" />
    public class RecoveryInfo : IRecoveryInfo
    {
        /// <summary>
        /// Gets the after timestamp of the recovery
        /// </summary>
        /// <value>The after</value>
        public long After { get; }

        /// <summary>
        /// Gets the timestamp specifying when the recovery was requested
        /// </summary>
        /// <value>The timestamp</value>
        public long Timestamp { get; }

        /// <summary>
        /// Gets the request identifier
        /// </summary>
        /// <value>The request identifier</value>
        public long RequestId { get; }

        /// <summary>
        /// Gets the response code of the recovery request
        /// </summary>
        /// <value>The response code of the recovery request</value>
        public int ResponseCode { get; }

        /// <summary>
        /// Gets the response message of the recovery request
        /// </summary>
        /// <value> the response message of the recovery request</value>
        public string ResponseMessage { get; }

        /// <summary>
        /// Gets the node identifier
        /// </summary>
        /// <value>The node identifier</value>
        public int NodeId { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryInfo"/> class
        /// </summary>
        /// <param name="after">The after</param>
        /// <param name="timestamp">The timestamp</param>
        /// <param name="requestId">The request id</param>
        /// <param name="nodeId">The node id</param>
        /// <param name="responseCode">The response code</param>
        /// <param name="responseMessage">The response message</param>
        public RecoveryInfo(long after, long timestamp, long requestId, int nodeId, int responseCode, string responseMessage)
        {
            After = after;
            Timestamp = timestamp;
            RequestId = requestId;
            NodeId = nodeId;
            ResponseCode = responseCode;
            ResponseMessage = responseMessage;
        }

        /// <inheritdoc />
        public override string ToString()
        {
            return $"After={After}/{SdkInfo.FromEpochTime(After)}, NodeId={NodeId}, Timestamp={Timestamp}/{SdkInfo.FromEpochTime(Timestamp)}, RequestId={RequestId}, Response={ResponseCode}-{ResponseMessage}";
        }
    }
}
