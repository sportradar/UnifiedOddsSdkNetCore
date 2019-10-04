/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;

namespace Sportradar.OddsFeed.SDK.Messages.Internal
{
    /// <summary>
    /// Class defining extension methods for <see cref="IProducer"/>
    /// </summary>
    public static class ProducerExtensions
    {
        ///// <summary>
        ///// Gets the maximum allowed duration of the recovery operation in seconds for the specified producer
        ///// </summary>
        ///// <param name="producer">A <see cref="IProducer"/></param>
        ///// <returns>The maximum allowed duration of the recovery operation in seconds for the specified producer</returns>
        //public static int MaxRecoveryDurationSec(this IProducer producer)
        //{
        //    return producer.Id == 3
        //        ? 1800
        //        : 900;
        //}

        /// <summary>
        /// Returns a <see cref="TimeSpan"/> specifying the max age of after param when doing a after timestamp recovery
        /// </summary>
        /// <param name="producer">The <see cref="IProducer"/> associated with the after timestamp recovery.</param>
        /// <returns>A <see cref="TimeSpan"/> specifying the max age of after param when doing a after timestamp recovery.</returns>
        public static TimeSpan MaxAfterAge(this IProducer producer)
        {
            Contract.Ensures(Contract.Result<TimeSpan>() > TimeSpan.Zero);

            return TimeSpan.FromMinutes(producer.MaxRecoveryTime);
        }
    }
}
