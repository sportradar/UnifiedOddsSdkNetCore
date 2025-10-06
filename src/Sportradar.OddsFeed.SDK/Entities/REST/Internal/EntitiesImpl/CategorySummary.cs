// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Common;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Implementation of <see cref="ICategorySummary"/>
    /// </summary>
    internal class CategorySummary : BaseEntity, ICategorySummary
    {
        /// <inheritdoc />
        public string CountryCode { get; }

        /// <summary>
        /// Creates new instance of category summary
        /// </summary>
        /// <param name="id"> a <see cref="Urn"/> uniquely identifying the category represented by the current instance</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo,String}"/> containing translated category name</param>
        /// <param name="countryCode">a country code</param>
        public CategorySummary(Urn id, IReadOnlyDictionary<CultureInfo, string> names, string countryCode)
            : base(id, names)
        {
            Guard.Argument(id, nameof(id)).NotNull();
            Guard.Argument(names, nameof(names)).NotNull().NotEmpty();

            CountryCode = countryCode;
        }
        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            return $"{base.PrintC()}, CountryCode={CountryCode}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            return $"{base.PrintF()}, CountryCode={CountryCode}";
        }
    }
}
