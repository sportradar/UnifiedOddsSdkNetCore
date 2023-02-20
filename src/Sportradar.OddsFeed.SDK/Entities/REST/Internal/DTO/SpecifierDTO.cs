/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for specifier
    /// </summary>
    internal class SpecifierDTO
    {
        internal string Name { get; }

        internal string Type { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecifierDTO"/> class.
        /// </summary>
        /// <param name="specifier">The <see cref="desc_specifiersSpecifier"/> used for creating instance</param>
        internal SpecifierDTO(desc_specifiersSpecifier specifier)
        {
            Guard.Argument(specifier, nameof(specifier)).NotNull();
            Guard.Argument(specifier.name, nameof(specifier.name)).NotNull().NotEmpty();
            Guard.Argument(specifier.type, nameof(specifier.type)).NotNull().NotEmpty();

            Name = specifier.name;
            Type = specifier.type;
        }
    }
}
