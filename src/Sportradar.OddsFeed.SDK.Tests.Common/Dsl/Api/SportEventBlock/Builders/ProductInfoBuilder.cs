// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <product_info>
//      <is_in_live_score/>
//     <is_in_hosted_statistics/>
//     <is_in_live_match_tracker/>
//     <links>
//     <link name="live_match_tracker" ref="https://widgets.sir.sportradar.com/sportradar/en/standalone/match.lmtPlus#matchId=58717423"/>
//     </links>
// </product_info>
public class ProductInfoBuilder
{
    private readonly productInfo _productInfo = new productInfo();

    public ProductInfoBuilder IsAutoTraded()
    {
        _productInfo.is_auto_traded = new productInfoItem();
        return this;
    }

    public ProductInfoBuilder IsInHostedStatistics()
    {
        _productInfo.is_in_hosted_statistics = new productInfoItem();
        return this;
    }

    public ProductInfoBuilder IsInLiveScore()
    {
        _productInfo.is_in_live_score = new productInfoItem();
        return this;
    }

    public ProductInfoBuilder IsInLiveMatchTracker()
    {
        _productInfo.is_in_live_match_tracker = new productInfoItem();
        return this;
    }

    public ProductInfoBuilder IsInLiveCenterSoccer()
    {
        _productInfo.is_in_live_center_soccer = new productInfoItem();
        return this;
    }

    public ProductInfoBuilder AddLink(string name, string href)
    {
        _productInfo.links ??= [];
        var list = _productInfo.links.ToList();
        list.Add(new productInfoLink { name = name, @ref = href });
        _productInfo.links = list.ToArray();
        return this;
    }

    public ProductInfoBuilder AddStreaming(int id, string name)
    {
        _productInfo.streaming ??= [];
        var list = _productInfo.streaming.ToList();
        list.Add(new streamingChannel { id = id, name = name });
        _productInfo.streaming = list.ToArray();
        return this;
    }

    public productInfo Build()
    {
        return _productInfo;
    }
}
