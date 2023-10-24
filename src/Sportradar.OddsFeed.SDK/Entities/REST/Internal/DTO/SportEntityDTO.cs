/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// Represents a base class for all data-access-objects representing sport related entities
    /// </summary>
    internal class SportEntityDto
    {
        /// <summary>
        /// Gets a <see cref="Urn"/> representing the ID of the represented sport entity
        /// </summary>
        /// <value>The identifier</value>
        public Urn Id { get; }

        /// <summary>
        /// Gets the name of the represented sport entity
        /// </summary>
        /// <value>The name</value>
        public string Name { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SportEntityDto"/> class
        /// </summary>
        /// <param name="id">a <see cref="string"/> representing the ID of the represented sport entity</param>
        /// <param name="name">the name of the represented sport entity</param>
        internal SportEntityDto(string id, string name)
        {
            Guard.Argument(id, nameof(id)).NotNull().NotEmpty();
            //Guard.Argument(name)); // it may happen to receive empty name

            Id = Urn.Parse(id);
            Name = name;
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c></returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as SportEntityDto;
            if (other == null)
            {
                return false;
            }
            return Id == other.Id;
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
