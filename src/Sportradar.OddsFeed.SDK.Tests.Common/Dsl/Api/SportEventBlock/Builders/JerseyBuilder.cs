// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <jersey type="home" base="e41e2c" sleeve="e41e2c" number="ffffff" stripes="false" horizontal_stripes="false" squares="false" split="false" shirt_type="short_sleeves" sleeve_detail="f20202"/>
public class JerseyBuilder
{
    private readonly jersey _jersey = new jersey();
    public JerseyBuilder WithType(string type)
    {
        _jersey.type = type;
        return this;
    }
    public JerseyBuilder WithBase(string baseColor)
    {
        _jersey.@base = baseColor;
        return this;
    }
    public JerseyBuilder WithSleeve(string sleeveColor)
    {
        _jersey.sleeve = sleeveColor;
        return this;
    }
    public JerseyBuilder WithNumber(string numberColor)
    {
        _jersey.number = numberColor;
        return this;
    }

    public JerseyBuilder WithStripes(bool stripes)
    {
        _jersey.stripes = stripes;
        _jersey.stripesSpecified = true;
        return this;
    }

    public JerseyBuilder WithHorizontalStripes(bool horizontalStripes)
    {
        _jersey.horizontal_stripes = horizontalStripes;
        _jersey.horizontal_stripesSpecified = true;
        return this;
    }

    public JerseyBuilder WithSquares(bool squares)
    {
        _jersey.squares = squares;
        _jersey.squaresSpecified = true;
        return this;
    }

    public JerseyBuilder WithSplit(bool split)
    {
        _jersey.split = split;
        _jersey.splitSpecified = true;
        return this;
    }

    public JerseyBuilder WithShirtType(string shirtType)
    {
        _jersey.shirt_type = shirtType;
        return this;
    }

    public JerseyBuilder WithSleeveDetail(string sleeveDetail)
    {
        _jersey.sleeve_detail = sleeveDetail;
        return this;
    }

    public jersey Build()
    {
        return _jersey;
    }
}
