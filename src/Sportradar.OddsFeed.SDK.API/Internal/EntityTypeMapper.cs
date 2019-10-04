/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;
// ReSharper disable RedundantCaseLabel

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A class used to determine the <see cref="Type"/> of the SDK entity used to represent a specific sport entity
    /// </summary>
    internal class EntityTypeMapper : IEntityTypeMapper
    {
        /// <summary>
        /// Returns a <see cref="Type" /> used to represent the specified entity
        /// </summary>
        /// <param name="id">A <see cref="URN" /> representing the entity identifier.</param>
        /// <param name="sportId">A <see cref="int" /> representing the id of the sport to which the entity belongs</param>
        /// <returns>A <see cref="Type" /> used to represent the specified entity.</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public virtual Type Map(URN id, int sportId)
        {
            switch (id.TypeGroup)
            {
                case ResourceTypeGroup.MATCH:
                {
                    return typeof(IMatch);
                }
                case ResourceTypeGroup.STAGE:
                {
                    return typeof(IStage);
                }
                case ResourceTypeGroup.BASIC_TOURNAMENT:
                {
                    return typeof(IBasicTournament);
                }
                case ResourceTypeGroup.TOURNAMENT:
                {
                    return typeof(ITournament);
                }
                case ResourceTypeGroup.SEASON:
                {
                    return typeof(ISeason);
                }
                case ResourceTypeGroup.DRAW:
                {
                    return typeof(IDraw);
                }
                case ResourceTypeGroup.LOTTERY:
                {
                    return typeof(ILottery);
                }
                case ResourceTypeGroup.OTHER:
                case ResourceTypeGroup.UNKNOWN:
                default:
                    throw new ArgumentException($"ResourceTypeGroup:{id.TypeGroup} is not supported", nameof(id));
            }
        }
    }
}