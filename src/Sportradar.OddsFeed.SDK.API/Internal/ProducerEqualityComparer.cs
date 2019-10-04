/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A <see cref="IEqualityComparer{T}"/> used to compare <see cref="IProducer"/> instances
    /// </summary>
    public class ProducerEqualityComparer : IEqualityComparer<IProducer>
    {
        /// <summary>
        /// Determines whether the passed <see cref="IProducer"/> instances are equal
        /// </summary>
        /// <param name="x">The first <see cref="IProducer"/> instance</param>
        /// <param name="y">The second <see cref="IProducer"/> instance</param>
        /// <returns>True if passed <see cref="IProducer"/> instances are equal; False otherwise</returns>
        public bool Equals(IProducer x, IProducer y)
        {
            if (ReferenceEquals(x, y))
            {
                return true;
            }

            return x != null && y != null && x.Id == y.Id;
        }

        /// <summary>
        /// Calculates and returns a hash code of the passed <see cref="IProducer"/> instance
        /// </summary>
        /// <param name="obj">The <see cref="IProducer"/> for which to calculate the hash code</param>
        /// <returns>The hash code of the passed <see cref="IProducer"/></returns>
        public int GetHashCode(IProducer obj)
        {
            return obj.Id.GetHashCode();
        }
    }
}
