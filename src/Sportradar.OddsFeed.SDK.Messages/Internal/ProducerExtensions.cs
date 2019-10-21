/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;

namespace Sportradar.OddsFeed.SDK.Messages.Internal
{
    /// <summary>
    /// Class defining extension methods for <see cref="IProducer"/>
    /// </summary>
    public static class ProducerExtensions
    {
        /// <summary>
        /// Returns a <see cref="TimeSpan"/> specifying the max age of after param when doing a after timestamp recovery
        /// </summary>
        /// <param name="producer">The <see cref="IProducer"/> associated with the after timestamp recovery.</param>
        /// <returns>A <see cref="TimeSpan"/> specifying the max age of after param when doing a after timestamp recovery.</returns>
        public static TimeSpan MaxAfterAge(this IProducer producer)
        {
            return TimeSpan.FromMinutes(producer.MaxRecoveryTime);
        }
    }
}
