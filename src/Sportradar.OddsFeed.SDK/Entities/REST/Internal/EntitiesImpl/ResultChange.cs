// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Provides information about result changes
    /// </summary>
    /// <seealso cref="IResultChange" />
    internal class ResultChange : EntityPrinter, IResultChange
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
        /// Initializes a new instance of the <see cref="ResultChange"/> class
        /// </summary>
        /// <param name="dto">A <see cref="ResultChangeDto"/> used to create new instance</param>
        internal ResultChange(ResultChangeDto dto)
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
