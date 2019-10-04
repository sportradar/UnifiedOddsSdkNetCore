/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.Contracts;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a type used to retrieve available producers on api
    /// </summary>
    [ContractClass(typeof(ProducersProviderContract))]
    public interface IProducersProvider
    {
        /// <summary>
        /// Gets the available producers from api
        /// </summary>
        /// <returns>A list of <see cref="IProducer"/></returns>
        IEnumerable<IProducer> GetProducers();
    }
}