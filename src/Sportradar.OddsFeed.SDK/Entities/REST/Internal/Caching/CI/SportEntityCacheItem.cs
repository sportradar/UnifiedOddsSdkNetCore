/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Entities.Rest.Caching.Exportable;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI
{
    /// <summary>
    /// A representation of the sport entity used by the cache
    /// </summary>
    internal class SportEntityCacheItem
    {
        /// <summary>
        /// Gets the id of the represented sport entity
        /// </summary>
        public Urn Id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEntityCacheItem"/> class
        /// </summary>
        /// <param name="dto">A <see cref="SportEntityDto"/> containing information about the sport entity</param>
        internal SportEntityCacheItem(SportEntityDto dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            Id = dto.Id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportableBase"/> class
        /// </summary>
        /// <param name="exportable">A <see cref="ExportableBase"/> containing information about the sport entity</param>
        internal SportEntityCacheItem(ExportableBase exportable)
        {
            if (exportable == null)
            {
                throw new ArgumentNullException(nameof(exportable));
            }

            Id = Urn.Parse(exportable.Id);
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="ExportableBase"/> class
        /// </summary>
        /// <param name="id">A <see cref="Urn"/> containing the sport entity id</param>
        internal SportEntityCacheItem(Urn id)
        {
            Id = id ?? throw new ArgumentNullException(nameof(id));
        }

        /// <summary>Determines whether the specified object is equal to the current object</summary>
        /// <returns>true if the specified object  is equal to the current object; otherwise, false</returns>
        /// <param name="obj">The object to compare with the current object</param>
        public override bool Equals(object obj)
        {
            var other = obj as SportEntityCacheItem;
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
