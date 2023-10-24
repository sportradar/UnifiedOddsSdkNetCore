/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System.Collections.Generic;
using System.Threading.Tasks;
using Microsoft.Extensions.Logging.Abstractions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class IncrementalSequenceGeneratorTests
{
    [Fact]
    public void TestIfValueIsIncrementedByOne()
    {
        var generator = new IncrementalSequenceGenerator(new NullLogger<IncrementalSequenceGenerator>());
        Assert.Equal(1, generator.GetNext());
        Assert.Equal(2, generator.GetNext());
        Assert.Equal(3, generator.GetNext());
    }

    [Fact]
    public void TestMinAndMaxAreRespected()
    {
        const int min = 3;
        const int max = 11;
        var generator = new IncrementalSequenceGenerator(new NullLogger<IncrementalSequenceGenerator>(), min, max);

        long value = 0;
        for (var i = min; i < max; i++)
        {
            value = generator.GetNext();
        }
        Assert.Equal(min, value);
    }

    [Fact]
    public void GetNextIsSequential()
    {
        const int min = 10;
        const int max = 100;
        var generator = new IncrementalSequenceGenerator(new NullLogger<IncrementalSequenceGenerator>(), min, max);

        for (var i = min + 1; i < max; i++)
        {
            var value = generator.GetNext();
            Assert.Equal(i, value);
        }
    }

    [Fact]
    public async Task TestGetNextIsThreadSafe()
    {
        const int min = -11;
        const int max = 543;
        const int iterations = 8791;

        var generator = new IncrementalSequenceGenerator(new NullLogger<IncrementalSequenceGenerator>(), min, max);
        var tasks = new List<Task>();
        for (var i = 0; i < iterations; i++)
        {
            tasks.Add(Task.Run(() => generator.GetNext()));
        }

        await Task.WhenAll(tasks).ConfigureAwait(false);

        const int range = max - min;
        const int reminder = iterations % range;
        var value = generator.GetNext();
        Assert.Equal(reminder + min + 1, value);
    }
}
