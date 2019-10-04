/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// A implementation of <see cref="ISportSummary"/>
    /// </summary>
    internal class SportSummary : BaseEntity, ISportSummary
    {
        /// <summary>
        /// Creates new instance of sport summary
        /// </summary>
        /// <param name="id">a <see cref="URN"/> uniquely identifying the sport represented by the constructed instance</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated sport names</param>
        public SportSummary(URN id, IReadOnlyDictionary<CultureInfo, string> names)
            : base(id, names)
        {
            Contract.Requires(id != null);
            Contract.Requires(names != null && names.Any());
        }
    }
}