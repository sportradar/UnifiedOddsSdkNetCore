using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class ListExtensionsTests
{
    [Fact]
    public void IsNullOrEmpty_NullList_ReturnsTrue()
    {
        Assert.True(((List<int>)null).IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_EmptyList_ReturnsTrue()
    {
        var emptyList = new List<int>();
        Assert.True(emptyList.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_NonEmptyList_ReturnsFalse()
    {
        var emptyList = new List<int> { 1 };
        Assert.False(emptyList.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_NullIEnumerable_ReturnsTrue()
    {
        Assert.True(((IEnumerable<int>)null).IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_EmptyIEnumerable_ReturnsTrue()
    {
        var emptyList = new Collection<int>();
        Assert.True(emptyList.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmpty_NonEmptyIEnumerable_ReturnsFalse()
    {
        var emptyList = new Collection<int> { 1 };
        Assert.False(emptyList.IsNullOrEmpty());
    }
}
