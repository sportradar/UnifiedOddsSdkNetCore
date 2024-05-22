// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Diagnostics.CodeAnalysis;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// Represents a delayed info for sport event
    /// </summary>
    [SuppressMessage("Minor Code Smell", "S101:Types should be named in PascalCase", Justification = "Dto is allowed")]
    internal class DelayedInfoDto
    {
        /// <summary>
        /// Gets a identifier
        /// </summary>
        /// <value>The identifier</value>
        public int Id { get; }

        /// <summary>
        /// Gets the description of delayed info
        /// </summary>
        /// <value>The description</value>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="DelayedInfoDto"/> class
        /// </summary>
        /// <param name="id">a identifier</param>
        /// <param name="description">the description of the represented sport entity</param>
        internal DelayedInfoDto(int id, string description)
        {
            Id = id;
            Description = description;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table</returns>
        public override int GetHashCode()
        {
            return Id.GetHashCode();
        }

        /// <inheritdoc />
        public override bool Equals(object obj)
        {
            if (!(obj is DelayedInfoDto other))
            {
                return false;
            }
            return Id == other.Id;
        }
    }
}
