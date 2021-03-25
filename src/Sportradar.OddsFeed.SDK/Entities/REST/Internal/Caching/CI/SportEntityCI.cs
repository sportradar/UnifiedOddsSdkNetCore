/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI
{
    /// <summary>
    /// A representation of the sport entity used by the cache
    /// </summary>
    internal class SportEntityCI
    {
        /// <summary>
        /// Gets the id of the represented sport entity
        /// </summary>
        public URN Id { get; protected set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEntityCI"/> class
        /// </summary>
        /// <param name="dto">A <see cref="SportEntityDTO"/> containing information about the sport entity</param>
        internal SportEntityCI(SportEntityDTO dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Id = dto.Id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportableCI"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableCI"/> containing information about the sport entity</param>
        internal SportEntityCI(ExportableCI exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            Id = URN.Parse(exportable.Id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportableCI"/> class
        /// </summary>
        /// <param name="id">A <see cref="URN"/> containing the sport entity id</param>
        internal SportEntityCI(URN id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>Determines whether the specified object is equal to the current object</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false</returns>
        /// <param name="obj">The object to compare with the current object</param>
        public override bool Equals(object obj)
        {
            var other = obj as SportEntityCI;
            if (other == null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            return Id == other.Id;
        }

        /// <summary>Serves as the default hash function</summary>
        /// <returns>A hash code for the current object</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }
    }
}
