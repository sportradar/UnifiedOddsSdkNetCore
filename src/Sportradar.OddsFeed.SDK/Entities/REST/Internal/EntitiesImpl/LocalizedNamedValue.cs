/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Castle.Core.Internal;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Implementation of the <see cref="ILocalizedNamedValue"/>
    /// </summary>
    internal class LocalizedNamedValue : NamedValue, ILocalizedNamedValue
    {
        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}" /> containing translated descriptions
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Descriptions { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="LocalizedNamedValue"/> class.
        /// </summary>
        /// <param name="id">The identifier.</param>
        /// <param name="descriptions">The descriptions.</param>
        /// <param name="defaultCulture">A <see cref="CultureInfo"/> specifying the default language</param>
        public LocalizedNamedValue(int id, IDictionary<CultureInfo, string> descriptions, CultureInfo defaultCulture)
            : base(id)
        {
            if ((descriptions == null && defaultCulture != null) || (descriptions != null && defaultCulture == null))
            {
                throw new ArgumentException("Either both 'descriptions' and 'defaultCulture' have to be null or both have to be not null");
            }

            if (descriptions != null)
            {
                Descriptions = descriptions as IReadOnlyDictionary<CultureInfo, string>
                    ?? new ReadOnlyDictionary<CultureInfo, string>(descriptions);
                Description = descriptions[defaultCulture];
            }
            else
            {
                Descriptions = new ReadOnlyDictionary<CultureInfo, string>(new Dictionary<CultureInfo, string>());
            }
        }

        /// <summary>
        /// Gets the description for specific locale
        /// </summary>
        /// <returns>Return the Description if exists, or null.</returns>
        public string GetDescription(CultureInfo culture)
        {
            return Descriptions.ContainsKey(culture) ? Descriptions[culture] : null;
        }

        /// <summary>Returns a string that represents the current object.</summary>
        /// <returns>A string that represents the current object.</returns>
        public override string ToString()
        {
            var desc = Descriptions.IsNullOrEmpty()
                ? string.Empty
                : string.Join(";", Descriptions.Select(s => $"{s.Key}={s.Value}"));
            return $"{Id}:{desc}";
        }
    }
}
