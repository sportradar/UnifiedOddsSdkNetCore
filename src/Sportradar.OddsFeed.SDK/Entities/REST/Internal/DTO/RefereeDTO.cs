/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-access-object representing a sport event referee
    /// </summary>
    /// <seealso cref="SportEntityDTO" />
    internal class RefereeDTO : SportEntityDTO
    {
        /// <summary>
        /// Gets the nationality of the represented referee
        /// </summary>
        /// <value>The nationality.</value>
        internal string Nationality { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefereeDTO"/> class.
        /// </summary>
        /// <param name="referee">A <see cref="referee"/> containing information about a referee.</param>
        internal RefereeDTO(referee referee)
            : base(referee.id, referee.name)
        {
            Nationality = referee.nationality;
        }
    }
}
