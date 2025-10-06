// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// A contract implemented by classes used to determine the type of the SDK entity used to represent the specific sport entity (tournament, race, match, ...)
    /// </summary>
    internal interface IEntityTypeMapper
    {
        /// <summary>
        /// Returns a <see cref="Type"/> used to represent the specified entity
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> representing the entity identifier.</param>
        /// <param name="sportId">A <see cref="int"/> representing the id of the sport to which the entity belongs</param>
        /// <returns>A <see cref="Type"/> used to represent the specified entity.</returns>
        Type Map(Urn id, int sportId);
    }
}
