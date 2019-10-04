/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Entities.Internal;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Factory for creating <see cref="IProducerRecoveryManager"/>
    /// </summary>
    /// <seealso cref="IProducerRecoveryManagerFactory" />
    internal class ProducerRecoveryManagerFactory : IProducerRecoveryManagerFactory
    {
        /// <summary>
        /// A <see cref="IRecoveryRequestIssuer"/> instance needed when creating <see cref="IProducerRecoveryManager"/> instances
        /// </summary>
        private readonly IRecoveryRequestIssuer _recoveryRequestIssuer;

        /// <summary>
        /// The <see cref="IFeedMessageMapper"/> instance used to <see cref="ISessionMessageManager"/> instances
        /// </summary>
        private readonly IFeedMessageMapper _messageMapper;

        /// <summary>
        /// The <see cref="IOddsFeedConfiguration"/>
        /// </summary>
        private readonly IOddsFeedConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerRecoveryManagerFactory"/> class.
        /// </summary>
        /// <param name="recoveryRequestIssuer">The <see cref="IRecoveryRequestIssuer"/> instance needed when creating <see cref="IProducerRecoveryManager"/> instances</param>
        /// <param name="messageMapper">The <see cref="IFeedMessageMapper"/> instance used to create <see cref="ISessionMessageManager"/> instances</param>
        /// <param name="config">The <see cref="IOddsFeedConfiguration"/> instance used to create <see cref="IProducerRecoveryManager"/> instances</param>
        public ProducerRecoveryManagerFactory(IRecoveryRequestIssuer recoveryRequestIssuer, IFeedMessageMapper messageMapper, IOddsFeedConfiguration config)
        {
            Contract.Requires(recoveryRequestIssuer != null);
            Contract.Requires(messageMapper != null);
            Contract.Requires(config != null);

            _recoveryRequestIssuer = recoveryRequestIssuer;
            _messageMapper = messageMapper;
            _config = config;
        }

        /// <summary>
        /// Lists the object invariants as required by code-contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_recoveryRequestIssuer != null);
            Contract.Invariant(_messageMapper != null);
            Contract.Invariant(_config != null);
        }

        /// <summary>
        /// Creates new <see cref="IProducerRecoveryManager" /> based on <see cref="IProducer" />
        /// </summary>
        /// <param name="producer">An <see cref="IProducer" /> for which to get the recovery tracker</param>
        /// <param name="allInterests">The list of all MessageInterests</param>
        /// <returns>Returns new <see cref="IProducerRecoveryManager" /></returns>
        public IProducerRecoveryManager GetRecoveryTracker(IProducer producer, IEnumerable<MessageInterest> allInterests)
        {
            var allInterestsList = allInterests as IList<MessageInterest> ?? allInterests.ToList();
            var timestampTracker = new TimestampTracker((Producer) producer, allInterestsList, _config.InactivitySeconds, _config.InactivitySeconds);
            var recoveryOperation = new RecoveryOperation((Producer)producer, _recoveryRequestIssuer, allInterestsList, _config.NodeId, _config.AdjustAfterAge);
            return new ProducerRecoveryManager(producer, recoveryOperation, timestampTracker);
        }

        /// <summary>
        /// Creates new <see cref="ISessionMessageManager" /> feed message processor
        /// </summary>
        /// <returns>Newly created <see cref="ISessionMessageManager" /></returns>
        public ISessionMessageManager CreateSessionMessageManager()
        {
            return new SessionMessageManager(_messageMapper);
        }
    }
}
