// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Config
{
    /// <summary>
    /// Defines a type used to retrieve available producers on api
    /// </summary>
    internal interface IProducersProvider
    {
        /// <summary>
        /// Gets the available producers from api
        /// </summary>
        /// <returns>A list of <see cref="IProducer"/></returns>
        IReadOnlyCollection<IProducer> GetProducers();
    }
}
