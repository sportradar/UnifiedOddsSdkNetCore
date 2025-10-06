// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representation for specifier
    /// </summary>
    internal class SpecifierDto
    {
        internal string Name { get; }

        internal string Type { get; }

        internal string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="SpecifierDto"/> class.
        /// </summary>
        /// <param name="specifier">The <see cref="desc_specifiersSpecifier"/> used for creating instance</param>
        internal SpecifierDto(desc_specifiersSpecifier specifier)
        {
            Guard.Argument(specifier, nameof(specifier)).NotNull();
            Guard.Argument(specifier.name, nameof(specifier.name)).NotNull().NotEmpty();
            Guard.Argument(specifier.type, nameof(specifier.type)).NotNull().NotEmpty();

            Name = specifier.name;
            Type = specifier.type;
            Description = specifier.description;
        }
    }
}
