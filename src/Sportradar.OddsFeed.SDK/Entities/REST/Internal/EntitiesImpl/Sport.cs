/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a sport
    /// </summary>
    /// <seealso cref="SportSummary" />
    /// <seealso cref="ISport" />
    internal class Sport : SportSummary, ISport
    {
        /// <summary>
        /// Gets a <see cref="IEnumerable{ICategory}"/> representing categories
        /// which belong to the sport represented by the current instance
        /// </summary>
        public IEnumerable<ICategory> Categories { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Sport"/> class
        /// </summary>
        /// <param name="id">a <see cref="URN"/> uniquely identifying the sport represented by the current instance</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated sport names</param>
        /// <param name="categories"> a <see cref="IEnumerable{ICategory}"/> representing categories
        /// which belong to the sport represented by the current instance</param>
        public Sport(URN id, IReadOnlyDictionary<CultureInfo, string> names, IEnumerable<ICategory> categories)
           : base(id, names)
        {
            Guard.Argument(names, nameof(names)).NotNull().NotEmpty();

            if (categories != null)
            {
                Categories = categories as IReadOnlyCollection<ICategory> ?? new ReadOnlyCollection<ICategory>(categories.ToList());
            }
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"Id={Id}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            var names = Names == null ? string.Empty : string.Join(", ", Names.Keys.Select(k => $"{k}={Names[k]}"));
            var cats = Categories == null ? string.Empty : string.Join(", ", Categories.Select(k => ((Category)k).ToString()));
            return $"Id={Id}, Name=[{names}], Categories=[{cats}]";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            var names = Names == null ? string.Empty : string.Join(", ", Names.Keys.Select(k => $"{k}={Names[k]}"));
            var cats = Categories == null ? string.Empty : string.Join("; ", Categories.Select(k => ((Category)k).ToString("f")));
            return $"Id={Id}, Names=[{names}], Categories=[{cats}]";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
