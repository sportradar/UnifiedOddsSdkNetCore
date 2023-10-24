/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-access-object representing a sport event referee
    /// </summary>
    /// <seealso cref="SportEntityDto" />
    internal class RefereeDto : SportEntityDto
    {
        /// <summary>
        /// Gets the nationality of the represented referee
        /// </summary>
        /// <value>The nationality.</value>
        internal string Nationality { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RefereeDto"/> class.
        /// </summary>
        /// <param name="referee">A <see cref="referee"/> containing information about a referee.</param>
        internal RefereeDto(referee referee)
            : base(referee.id, referee.name)
        {
            Nationality = referee.nationality;
        }
    }
}
