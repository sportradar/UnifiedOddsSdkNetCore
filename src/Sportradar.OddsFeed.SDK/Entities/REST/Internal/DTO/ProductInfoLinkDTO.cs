// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representation for product info link
    /// </summary>
    internal class ProductInfoLinkDto
    {
        internal string Name { get; }

        internal string Reference { get; }

        internal ProductInfoLinkDto(productInfoLink productInfoLink)
        {
            Guard.Argument(productInfoLink, nameof(productInfoLink)).NotNull();
            Guard.Argument(productInfoLink.name, nameof(productInfoLink.name)).NotNull().NotEmpty();
            Guard.Argument(productInfoLink.@ref, nameof(productInfoLink.@ref)).NotNull().NotEmpty();

            Name = productInfoLink.name;
            Reference = productInfoLink.@ref;
        }
    }
}
