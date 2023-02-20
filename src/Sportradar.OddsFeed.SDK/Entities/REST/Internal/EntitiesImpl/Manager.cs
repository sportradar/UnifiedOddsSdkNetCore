/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class Manager
    /// </summary>
    /// <seealso cref="BaseEntity" />
    /// <seealso cref="IManager" />
    internal class Manager : BaseEntity, IManager
    {
        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}" /> containing nationalities in different languages
        /// </summary>
        private IReadOnlyDictionary<CultureInfo, string> Nationalities { get; }

        /// <summary>
        /// Gets the country code
        /// </summary>
        /// <value>The country code</value>
        public string CountryCode { get; }

        /// <summary>
        /// Gets the nationality of the manager
        /// </summary>
        /// <param name="culture">The culture.</param>
        /// <returns>Return a nationality of the manager</returns>
        public string GetNationality(CultureInfo culture)
        {
            return Nationalities == null || !Nationalities.ContainsKey(culture)
                ? null
                : Nationalities[culture];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IManager"/> class
        /// </summary>
        /// <param name="item">The item</param>
        public Manager(ManagerCI item)
            : base(item.Id, item.Name as IReadOnlyDictionary<CultureInfo, string>)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            Nationalities = item.Nationality as IReadOnlyDictionary<CultureInfo, string>;
            CountryCode = item.CountryCode;
        }
    }
}
