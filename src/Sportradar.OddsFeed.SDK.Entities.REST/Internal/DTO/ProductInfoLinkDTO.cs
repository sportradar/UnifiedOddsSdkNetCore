/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO
{
    /// <summary>
    /// A data-transfer-object representation for product info link
    /// </summary>
    internal class ProductInfoLinkDTO
    {
        internal string Name { get; }

        internal string Reference { get; }

        internal ProductInfoLinkDTO(productInfoLink productInfoLink)
        {
            Contract.Requires(productInfoLink != null);
            Contract.Requires(!string.IsNullOrEmpty(productInfoLink.name));
            Contract.Requires(!string.IsNullOrEmpty(productInfoLink.@ref));

            Name = productInfoLink.name;
            Reference = productInfoLink.@ref;
        }
    }
}