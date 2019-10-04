/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for specifier
    /// </summary>
    public class SpecifierDTO
    {
        internal string Name { get; }

        internal string Type { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="SpecifierDTO"/> class.
        /// </summary>
        /// <param name="specifier">The <see cref="desc_specifiersSpecifier"/> used for creating instance</param>
        internal SpecifierDTO(desc_specifiersSpecifier specifier)
        {
            Contract.Requires(specifier != null);
            Contract.Requires(!string.IsNullOrEmpty(specifier.name));
            Contract.Requires(!string.IsNullOrEmpty(specifier.type));


            Name = specifier.name;
            Type = specifier.type;
        }
    }
}
