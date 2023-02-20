/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Class DrawResult
    /// </summary>
    /// <seealso cref="EntityPrinter" />
    /// <seealso cref="IDrawResult" />
    internal class DrawResult : EntityPrinter, IDrawResult
    {
        /// <summary>
        /// Gets the value of the draw
        /// </summary>
        /// <value>The value</value>
        public int? Value { get; }
        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}" /> containing translated names
        /// </summary>
        /// <value>The names</value>
        public IReadOnlyDictionary<CultureInfo, string> Names { get; }
        /// <summary>
        /// Gets the name in specified culture language
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>The name in specified language</returns>
        /// <exception cref="System.NotImplementedException"></exception>
        public string GetName(CultureInfo culture)
        {
            return Names == null || !Names.ContainsKey(culture)
                ? null
                : Names[culture];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DrawResult"/> class
        /// </summary>
        /// <param name="item">The item</param>
        public DrawResult(DrawResultCI item)
        {
            Guard.Argument(item, nameof(item)).NotNull();

            Value = item.Value;
            Names = item.Names as IReadOnlyDictionary<CultureInfo, string>;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"Value={Value}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            var defaultCulture = new CultureInfo("en");
            var name = Names == null
                ? null
                : Names.ContainsKey(defaultCulture)
                    ? Names[defaultCulture]
                    : Names.Values.FirstOrDefault();
            return $"Value={Value}, Names={name}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            var names = Names == null
                ? null
                : string.Join("; ", Names.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            return $"Value={Value}, Names=[{names}]";
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
