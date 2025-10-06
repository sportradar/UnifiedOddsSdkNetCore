// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Sportradar.OddsFeed.SDK.Common.Extensions;
using Sportradar.OddsFeed.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public class ListExtensionsTests
{
    [Fact]
    public void IsNullOrEmptyWhenNullListThenReturnsTrue()
    {
        Assert.True(((List<int>)null).IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmptyWhenEmptyListThenReturnsTrue()
    {
        Assert.True(new List<int>().IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmptyWhenNonEmptyListThenReturnsFalse()
    {
        var emptyList = new List<int> { 1 };
        Assert.False(emptyList.IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmptyWhenNullIEnumerableThenReturnsTrue()
    {
        Assert.True(((IEnumerable<int>)null).IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmptyWhenEmptyIEnumerableThenReturnsTrue()
    {
        Assert.True(new Collection<int>().IsNullOrEmpty());
    }

    [Fact]
    public void IsNullOrEmptyWhenNonEmptyIEnumerableThenReturnsFalse()
    {
        var emptyList = new Collection<int> { 1 };
        Assert.False(emptyList.IsNullOrEmpty());
    }

    [Fact]
    public void AddUniqueWhenCollectionNullThenThrow()
    {
        _ = Assert.Throws<ArgumentNullException>(() => ((List<int>)null).AddUnique(1));
    }

    [Fact]
    public void AddUniqueWhenItemNullThenDoNotAdd()
    {
        var emptyList = new List<NonSdkClass>();

        emptyList.AddUnique(null);

        Assert.Empty(emptyList);
    }

    [Fact]
    public void AddUniqueWhenNewItemThenAddToCollection()
    {
        var emptyList = new List<int>();

        emptyList.AddUnique(1);

        _ = Assert.Single(emptyList);
        Assert.Equal(1, emptyList[0]);
    }

    [Fact]
    public void AddUniqueWhenItemExistsThenNoChange()
    {
        var initialList = new List<int> { 1 };

        initialList.AddUnique(1);

        _ = Assert.Single(initialList);
        Assert.Equal(1, initialList[0]);
    }

    [Fact]
    public void AddUniqueWhenNewItemThenAdd()
    {
        var initialList = new List<int> { 1, 2, 3 };

        initialList.AddUnique(10);

        Assert.Equal(4, initialList.Count);
        Assert.Equal(10, initialList[3]);
    }
}
