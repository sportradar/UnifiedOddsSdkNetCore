/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object representing a named value
    /// </summary>
    public class NamedValueDTO
    {
        /// <summary>
        /// Gets the id of the match status
        /// </summary>
        public int Id { get; }

        /// <summary>
        /// Gets the description
        /// </summary>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="NamedValueDTO"/> class.
        /// </summary>
        internal NamedValueDTO(int id, string description)
        {
            Guard.Argument(id, nameof(id)).NotNegative();

            Id = id;
            Description = description;
        }
    }
}
