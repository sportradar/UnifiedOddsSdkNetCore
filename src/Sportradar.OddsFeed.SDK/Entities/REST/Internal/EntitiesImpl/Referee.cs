/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a sport event referee
    /// </summary>
    /// <seealso cref="Sportradar.OddsFeed.SDK.Entities.REST.IReferee" />
    internal class Referee : EntityPrinter, IReferee
    {
        /// <summary>
        /// Gets a value used to uniquely identify the current <see cref="IReferee" /> instance
        /// </summary>
        public URN Id { get; }

        /// <summary>
        /// Gets the name of the referee represented by the current <see cref="IReferee" /> instance
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}" /> containing referee nationality in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Nationalities { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Referee"/> class.
        /// </summary>
        /// <param name="ci">A <see cref="RefereeCI"/> used to create new instance</param>
        /// <param name="cultures">A cultures of the current instance of <see cref="Referee"/></param>
        internal Referee(RefereeCI ci, IEnumerable<CultureInfo> cultures)
        {
            var cultureList = cultures as IList<CultureInfo> ?? cultures.ToList();

            Id = ci.Id;
            Name = ci.Name;
            Nationalities = new ReadOnlyDictionary<CultureInfo, string>(
                cultureList.Where(c => ci.GetNationality(c) != null).ToDictionary(c => c, ci.GetNationality));
        }

        protected override string PrintI()
        {
            return $"Id={Id}";
        }

        protected override string PrintC()
        {
            var defaultCulture = new CultureInfo("en");
            var nationality = Nationalities.ContainsKey(defaultCulture)
                                  ? Nationalities[defaultCulture]
                                  : Nationalities.Values.FirstOrDefault();
            return $"Id={Id}, Name={Name}, Nationality={nationality}";
        }

        protected override string PrintF()
        {
            var nationalities = string.Join(" ", Nationalities.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            return $"Id={Id}, Name={Name}, Nationalities=[{nationalities}]";
        }

        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }

        /// <summary>
        /// Gets the referee nationality in the specified language
        /// </summary>
        /// <param name="culture">A <see cref="CultureInfo" /> specifying the language.</param>
        /// <returns>The referee nationality in the specified language.</returns>
        public string GetNationality(CultureInfo culture)
        {
            return Nationalities.ContainsKey(culture)
                ? Nationalities[culture]
                : null;
        }
    }
}
