/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Sports
{
    /// <summary>
    /// Contains basic information about a sport (soccer, basketball, ...)
    /// </summary>
    /// <seealso cref="SportEntityData" />
    public class SportData : SportEntityData
    {
        /// <summary>
        /// Gets the <see cref="IEnumerable{CategoryData}"/> representing the categories, which belong to the sport
        /// represented by the current instance
        /// </summary>
        public readonly IEnumerable<CategoryData> Categories;

        /// <summary>
        /// Initializes a new instance of the <see cref="SportData"/> class.
        /// </summary>
        /// <param name="id">a <see cref="URN"/> representing the id of the associated sport</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated sport name</param>
        /// <param name="categories">the <see cref="IEnumerable{CategoryData}"/> representing the categories, which belong to the sport</param>
        public SportData(URN id, IReadOnlyDictionary<CultureInfo, string> names, IEnumerable<CategoryData> categories)
            :base(id, names)
        {
            if (categories != null)
            {
                Categories = categories as ReadOnlyCollection<CategoryData> ?? new ReadOnlyCollection<CategoryData>(categories.ToList());
            }
        }
    }
}
