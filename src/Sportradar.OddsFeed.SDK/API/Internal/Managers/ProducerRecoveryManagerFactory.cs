// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Recovery;
using Sportradar.OddsFeed.SDK.Entities.Internal;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
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
        /// The <see cref="IUofConfiguration"/>
        /// </summary>
        private readonly IUofConfiguration _config;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerRecoveryManagerFactory"/> class.
        /// </summary>
        /// <param name="recoveryRequestIssuer">The <see cref="IRecoveryRequestIssuer"/> instance needed when creating <see cref="IProducerRecoveryManager"/> instances</param>
        /// <param name="messageMapper">The <see cref="IFeedMessageMapper"/> instance used to create <see cref="ISessionMessageManager"/> instances</param>
        /// <param name="config">The <see cref="IUofConfiguration"/> instance used to create <see cref="IProducerRecoveryManager"/> instances</param>
        public ProducerRecoveryManagerFactory(IRecoveryRequestIssuer recoveryRequestIssuer, IFeedMessageMapper messageMapper, IUofConfiguration config)
        {
            Guard.Argument(recoveryRequestIssuer, nameof(recoveryRequestIssuer)).NotNull();
            Guard.Argument(messageMapper, nameof(messageMapper)).NotNull();
            Guard.Argument(config, nameof(config)).NotNull();

            _recoveryRequestIssuer = recoveryRequestIssuer;
            _messageMapper = messageMapper;
            _config = config;
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
            var timestampTracker = new TimestampTracker((Producer)producer, allInterestsList, (int)_config.Producer.InactivitySeconds.TotalSeconds, (int)_config.Producer.InactivitySeconds.TotalSeconds);
            var recoveryOperation = new RecoveryOperation((Producer)producer, _recoveryRequestIssuer, allInterestsList, _config.NodeId);
            return new ProducerRecoveryManager(producer, recoveryOperation, timestampTracker, (int)_config.Producer.MinIntervalBetweenRecoveryRequests.TotalSeconds);
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
