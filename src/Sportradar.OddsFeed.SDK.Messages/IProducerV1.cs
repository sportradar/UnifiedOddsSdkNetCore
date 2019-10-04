/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Messages
{
    /// <summary>
    /// Defines a contract for producer which use the feed to dispatch messages
    /// </summary>
    public interface IProducerV1 : IProducer
    {
        /// <summary>
        /// Gets the recovery info about last recovery attempt
        /// </summary>
        /// <value>The recovery info about last recovery attempt</value>
        IRecoveryInfo RecoveryInfo { get; }
    }
}