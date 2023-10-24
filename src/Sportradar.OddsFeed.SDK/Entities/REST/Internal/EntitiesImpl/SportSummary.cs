/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// A implementation of <see cref="ISportSummary"/>
    /// </summary>
    internal class SportSummary : BaseEntity, ISportSummary
    {
        /// <summary>
        /// Creates new instance of sport summary
        /// </summary>
        /// <param name="id">a <see cref="Urn"/> uniquely identifying the sport represented by the constructed instance</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated sport names</param>
        public SportSummary(Urn id, IReadOnlyDictionary<CultureInfo, string> names)
            : base(id, names)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(names, nameof(names)).NotNull().NotEmpty();
        }
    }
}
