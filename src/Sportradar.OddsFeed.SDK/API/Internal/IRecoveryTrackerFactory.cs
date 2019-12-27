/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Contract for creating factory for creating new <see cref="IProducerRecoveryManager"/>
    /// </summary>
    internal  interface IProducerRecoveryManagerFactory
    {
        /// <summary>
        /// Creates new <see cref="IProducerRecoveryManager" /> based on <see cref="IProducer" />
        /// </summary>
        /// <param name="producer">An <see cref="IProducer"/> for which to get the recovery tracker</param>
        /// <param name="allInterests">The list of all <see cref="MessageInterest"/> in all opened sessions</param>
        /// <returns>Returns new <see cref="IProducerRecoveryManager" /></returns>
        IProducerRecoveryManager GetRecoveryTracker(IProducer producer, IEnumerable<MessageInterest> allInterests);

        /// <summary>
        /// Creates new <see cref="ISessionMessageManager"/>
        /// </summary>
        /// <returns>Newly created <see cref="ISessionMessageManager"/></returns>
        ISessionMessageManager CreateSessionMessageManager();
    }
}
