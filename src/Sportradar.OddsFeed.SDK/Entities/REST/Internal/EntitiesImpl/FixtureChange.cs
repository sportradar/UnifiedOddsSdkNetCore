// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Provides information about fixture changes
    /// </summary>
    /// <seealso cref="IFixtureChange" />
    internal class FixtureChange : EntityPrinter, IFixtureChange
    {
        /// <summary>
        /// Gets the <see cref="Urn"/> specifying the sport event
        /// </summary>
        public Urn SportEventId { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying the last update time
        /// </summary>
        public DateTime UpdateTime { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="FixtureChange"/> class
        /// </summary>
        /// <param name="dto">A <see cref="FixtureChangeDto"/> used to create new instance</param>
        internal FixtureChange(FixtureChangeDto dto)
        {
            SportEventId = dto.SportEventId;
            UpdateTime = dto.UpdateTime;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"SportEventId={SportEventId}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            return $"SportEventId={SportEventId}, UpdateTime={UpdateTime}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
