/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class DelayedInfo
    /// </summary>
    /// <seealso cref="BaseEntity" />
    /// <seealso cref="IDelayedInfo" />
    internal class DelayedInfo : EntityPrinter, IDelayedInfo
    {
        /// <summary>
        /// Gets the id identifying the current instance
        /// </summary>
        /// <value>The id identifying the current instance</value>
        public int Id { get; }

        /// <summary>
        /// Gets the list of translated names
        /// </summary>
        /// <value>The list of translated names</value>
        public IReadOnlyDictionary<CultureInfo, string> Descriptions { get; }

        /// <summary>
        /// Gets the description associated with this instance in specific language
        /// </summary>
        /// <param name="culture">The language used to get the description</param>
        /// <returns>Description if available in specified language or null</returns>
        public string GetDescription(CultureInfo culture)
        {
            return Descriptions == null || !Descriptions.ContainsKey(culture)
                ? null
                : Descriptions[culture];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IDelayedInfo"/>
        /// </summary>
        /// <param name="delayedInfoCI">The delayed info cache item</param>
        public DelayedInfo(DelayedInfoCI delayedInfoCI)
        {
            Guard.Argument(delayedInfoCI, nameof(delayedInfoCI)).NotNull();

            Id = delayedInfoCI.Id;
            Descriptions = delayedInfoCI.Descriptions as IReadOnlyDictionary<CultureInfo, string>;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"Id={Id}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            var defaultCulture = new CultureInfo("en");
            var name = Descriptions == null
                ? null
                : Descriptions.ContainsKey(defaultCulture)
                        ? Descriptions[defaultCulture]
                        : Descriptions.Values.FirstOrDefault();
            return $"Id={Id}, Description={name}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            var names = Descriptions == null
                ? null
                : string.Join("; ", Descriptions.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            return $"Id={Id}, Descriptions=[{names}]";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
