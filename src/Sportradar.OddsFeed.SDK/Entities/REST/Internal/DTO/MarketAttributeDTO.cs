// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representing a market description attributes
    /// </summary>
    internal class MarketAttributeDto
    {
        /// <summary>
        /// Gets the attribute name
        /// </summary>
        /// <value>The name.</value>
        public string Name { get; }

        /// <summary>
        /// Gets the attribute description
        /// </summary>
        /// <value>The description.</value>
        public string Description { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MarketAttributeDto"/> class.
        /// </summary>
        /// <param name="record">A <see cref="attributesAttribute"/> representing attribute object obtained by parsing the xml.</param>
        public MarketAttributeDto(attributesAttribute record)
        {
            Guard.Argument(record, nameof(record)).NotNull();

            Name = record.name;
            Description = record.description;
        }
    }
}
