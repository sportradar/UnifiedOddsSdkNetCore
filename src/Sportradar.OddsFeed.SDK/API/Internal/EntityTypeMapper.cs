// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Entities.Rest;

// ReSharper disable RedundantCaseLabel

namespace Sportradar.OddsFeed.SDK.Api.Internal
{
    /// <summary>
    /// A class used to determine the <see cref="Type"/> of the SDK entity used to represent a specific sport entity
    /// </summary>
    internal class EntityTypeMapper : IEntityTypeMapper
    {
        /// <summary>
        /// Returns a <see cref="Type" /> used to represent the specified entity
        /// </summary>
        /// <param name="id">A <see cref="Urn" /> representing the entity identifier.</param>
        /// <param name="sportId">A <see cref="int" /> representing the id of the sport to which the entity belongs</param>
        /// <returns>A <see cref="Type" /> used to represent the specified entity.</returns>
        /// <exception cref="NotImplementedException"></exception>
        public virtual Type Map(Urn id, int sportId)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(sportId, nameof(sportId)).Positive();

            switch (id.TypeGroup)
            {
                case ResourceTypeGroup.Match:
                    {
                        return typeof(IMatch);
                    }
                case ResourceTypeGroup.Stage:
                    {
                        return typeof(IStage);
                    }
                case ResourceTypeGroup.BasicTournament:
                    {
                        return typeof(IBasicTournament);
                    }
                case ResourceTypeGroup.Tournament:
                    {
                        return typeof(ITournament);
                    }
                case ResourceTypeGroup.Season:
                    {
                        return typeof(ISeason);
                    }
                case ResourceTypeGroup.Draw:
                    {
                        return typeof(IDraw);
                    }
                case ResourceTypeGroup.Lottery:
                    {
                        return typeof(ILottery);
                    }
                case ResourceTypeGroup.Other:
                case ResourceTypeGroup.Unknown:
                default:
                    throw new ArgumentException($"ResourceTypeGroup:{id.TypeGroup} is not supported", nameof(id));
            }
        }
    }
}
