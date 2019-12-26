/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class SeasonInfo
    /// </summary>
    /// <seealso cref="ISeasonInfo" />
    internal class SeasonInfo : BaseEntity, ISeasonInfo
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="SeasonInfo"/> class
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <param name="names">The names</param>
        public SeasonInfo(URN id, IDictionary<CultureInfo, string> names)
            : base(id, names as IReadOnlyDictionary<CultureInfo, string>)
        {
        }
    }
}
