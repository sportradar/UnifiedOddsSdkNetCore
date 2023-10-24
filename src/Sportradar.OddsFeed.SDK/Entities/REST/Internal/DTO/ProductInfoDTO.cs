/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    /// <summary>
    /// A data-transfer-object representation for product info
    /// </summary>
    internal class ProductInfoDto
    {
        internal bool IsAutoTraded { get; }

        internal bool IsInHostedStatistics { get; }

        internal bool IsInLiveCenterSoccer { get; }

        internal bool IsInLiveMatchTracker { get; }

        internal bool IsInLiveScore { get; }

        internal IEnumerable<ProductInfoLinkDto> ProductInfoLinks { get; }

        internal IEnumerable<StreamingChannelDto> StreamingChannels { get; }

        internal ProductInfoDto(productInfo productInfo)
        {
            Guard.Argument(productInfo, nameof(productInfo)).NotNull();

            IsAutoTraded = productInfo.is_auto_traded != null;
            IsInHostedStatistics = productInfo.is_in_hosted_statistics != null;
            IsInLiveCenterSoccer = productInfo.is_in_live_center_soccer != null;
            IsInLiveMatchTracker = productInfo.is_in_live_match_tracker != null;
            IsInLiveScore = productInfo.is_in_live_score != null;

            ProductInfoLinks = productInfo.links != null && productInfo.links.Any()
                ? new ReadOnlyCollection<ProductInfoLinkDto>(productInfo.links.Select(p => new ProductInfoLinkDto(p)).ToList())
                : null;

            StreamingChannels = productInfo.streaming != null && productInfo.streaming.Any()
                ? new ReadOnlyCollection<StreamingChannelDto>(productInfo.streaming.Select(s => new StreamingChannelDto(s)).ToList())
                : null;
        }
    }
}
