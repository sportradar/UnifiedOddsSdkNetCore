// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

public class CourseBuilder
{
    private readonly course _course;

    public static CourseBuilder Create(Urn id, string name)
    {
        return new CourseBuilder(id, name);
    }

    private CourseBuilder(Urn id, string name)
    {
        _course = new course
        {
            id = id.ToString(),
            name = name
        };
    }

    public CourseBuilder AddHole(int number, int par)
    {
        _course.hole ??= [];
        var list = _course.hole.ToList();
        list.Add(new hole
        {
            number = number,
            par = par
        });
        _course.hole = list.ToArray();
        return this;
    }

    public course Build()
    {
        return _course;
    }
}
