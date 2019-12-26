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
    /// Represents a sport category
    /// </summary>
    internal class Category : CategorySummary, ICategory
    {
        /// <summary>
        /// Initializes a new instance of the <see cref="Category"/> class
        /// </summary>
        /// <param name="id">a <see cref="URN"/> uniquely identifying the current <see cref="ICategory"/> instance</param>
        /// <param name="names">a <see cref="IReadOnlyDictionary{CultureInfo, String}"/> containing translated category name</param>
        /// <param name="countryCode">a country code</param>
        /// <param name="tournaments">a <see cref="IEnumerable{ISportEvent}"/> representing the tournaments which belong to the category represented by the current instance </param>
        public Category(URN id, IReadOnlyDictionary<CultureInfo, string> names, string countryCode, IEnumerable<ISportEvent> tournaments)
            : base(id, names, countryCode)
        {
            Guard.Argument(tournaments, nameof(tournaments)).NotNull();

            Tournaments = tournaments as IReadOnlyCollection<ISportEvent> ?? new ReadOnlyCollection<ISportEvent>(tournaments.ToList());
        }

        /// <summary>
        /// Gets a <see cref="IEnumerable{ISportEvent}"/> representing the tournaments which belong to
        /// the category represented by the current instance
        /// </summary>
        public IEnumerable<ISportEvent> Tournaments { get; }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            var names = Names == null ? string.Empty : string.Join(", ", Names.Keys.Select(k => $"{k}={Names[k]}"));
            var tournaments = string.Join(", ", Tournaments.Select(k => k.Id.ToString()));
            return $"Id={Id}, Names=[{names}], Tournaments=[{tournaments}]";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            var names = Names == null ? string.Empty : string.Join(", ", Names.Keys.Select(k => $"{k}={Names[k]}"));
            //var tournaments = string.Join(" ,", Tournaments.Select(k => k.Id.ToString()));
            string tournaments = Tournaments.Aggregate(string.Empty, (current, t) => current + $"\t {((Tournament) t).ToString("f")}");
            return $"Id={Id}, Names=[{names}], Tournaments=[{tournaments}]";
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
