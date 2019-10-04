/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// Represents a delayed info for sport event
    /// </summary>
    public class DelayedInfoDTO
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
        /// Initializes a new instance of the <see cref="DelayedInfoDTO"/> class
        /// </summary>
        /// <param name="id">a identifier</param>
        /// <param name="description">the description of the represented sport entity</param>
        internal DelayedInfoDTO(int id, string description)
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
    }
}
