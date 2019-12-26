/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Status of the producer recovery
    /// </summary>
    internal enum ProducerRecoveryStatus
    {
        /// <summary>
        /// Producer is created, but recovery not yet started
        /// </summary>
        NotStarted = 0,

        /// <summary>
        /// Recovery is started
        /// </summary>
        Started = 1,

        /// <summary>
        /// Recovery is successfully completed
        /// </summary>
        Completed = 2,

        /// <summary>
        /// During recovery request error occurred
        /// </summary>
        Error = 3,

        /// <summary>
        /// The fatal error occurred (feed needs to be closed)
        /// </summary>
        FatalError = 6,

        /// <summary>
        /// The processing of feed messages is delayed
        /// </summary>
        Delayed = 7
    }
}