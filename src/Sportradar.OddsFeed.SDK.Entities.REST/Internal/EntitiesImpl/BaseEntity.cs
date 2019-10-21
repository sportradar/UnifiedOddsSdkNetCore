/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using Dawn;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Implementation of <see cref="IBaseEntity"/>, containing Id as <see cref="URN"/> and translatable Name
    /// </summary>
    /// <remarks>Id is required, name(s) can be null</remarks>
    /// <seealso cref="IBaseEntity" />
    internal class BaseEntity : EntityPrinter, IBaseEntity
    {
        /// <summary>
        /// Gets the <see cref="URN" /> identifying the current instance
        /// </summary>
        /// <value>The <see cref="URN" /> identifying the current instance</value>
        public URN Id { get; }

        /// <summary>
        /// Gets the list of translated names
        /// </summary>
        /// <value>The list of translated names</value>
        public virtual IReadOnlyDictionary<CultureInfo, string> Names { get; }

        /// <summary>
        /// Gets the name
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>System.String</returns>
        public virtual string GetName(CultureInfo culture)
        {
            return Names == null || !Names.ContainsKey(culture)
                ? null
                : Names[culture];
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="IBaseEntity"/> class
        /// </summary>
        /// <param name="id">The identifier</param>
        /// <param name="names">The names</param>
        public BaseEntity(URN id, IReadOnlyDictionary<CultureInfo, string> names)
        {
            Guard.Argument(id).NotNull();

            Id = id;
            Names = names;
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
            var name = Names == null || !Names.Any()
                ? null
                : Names.ContainsKey(defaultCulture)
                        ? Names[defaultCulture]
                        : Names.Values.FirstOrDefault();
            return $"Id={Id}, Name={name}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            var names = Names == null || !Names.Any()
                ? null
                : string.Join("; ", Names.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            return $"Id={Id}, Names=[{names}]";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
