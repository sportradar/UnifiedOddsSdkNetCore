// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Collections.Generic;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <draw_result>
//   <draws chronological="false">
//     <draw name="draw_1" value="3"/>
//   </draws>
// </draw_result>
public class DrawResultBuilder
{
    private readonly List<draw_resultDrawsDraw> _draws = new List<draw_resultDrawsDraw>();
    private bool _chronological;
    private bool _chronologicalSpecified;

    public DrawResultBuilder WithChronological(bool chronological)
    {
        _chronological = chronological;
        _chronologicalSpecified = true;
        return this;
    }

    public DrawResultBuilder AddDraw(string name, int value)
    {
        _draws.Add(new draw_resultDrawsDraw
        {
            name = name,
            value = value,
            valueSpecified = true
        });
        return this;
    }

    public draw_result Build()
    {
        return new draw_result
        {
            draws = new draw_resultDraws
            {
                chronological = _chronological,
                chronologicalSpecified = _chronologicalSpecified,
                draw = _draws.ToArray()
            }
        };
    }
}
