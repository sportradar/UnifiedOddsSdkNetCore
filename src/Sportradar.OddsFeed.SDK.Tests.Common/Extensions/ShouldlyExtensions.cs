// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Shouldly;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Extensions;

public static class ShouldlyExtensions
{
    public static void ShouldContainKey(this IReadOnlyDictionary<string, string> dictionary, string key)
    {
        ((IDictionary<string, string>)dictionary).ShouldContainKey(key);
    }

    public static void ShouldContainKey<TKey, TVal>(this IReadOnlyDictionary<TKey, TVal> dictionary, TKey key)
    {
        ((IDictionary<TKey, TVal>)dictionary).ShouldContainKey(key);
    }

    public static void ShouldNotContainKey(this IReadOnlyDictionary<string, string> dictionary, string key)
    {
        ((IDictionary<string, string>)dictionary).ShouldNotContainKey(key);
    }

    public static void ShouldContainKeyValue(this IReadOnlyDictionary<string, string> dictionary, string key, string value)
    {
        var dic = (IDictionary<string, string>)dictionary;
        dic.ShouldContainKey(key);
        dic[key].ShouldBe(value);
    }

    public static void ShouldContainKeyNotValue(this IReadOnlyDictionary<string, string> dictionary, string key, string value)
    {
        var dic = (IDictionary<string, string>)dictionary;
        dic.ShouldContainKey(key);
        dic[key].ShouldNotBe(value);
    }

    public static void ShouldBeOfSize(this IReadOnlyDictionary<string, string> dictionary, int size)
    {
        var dicSize = dictionary.Count;
        dicSize.ShouldBe(size, $"Expected dictionary size {size} but was {dicSize}");
    }

    public static void ShouldBeOfSize<T>(this IEnumerable<T> list, int size)
    {
        var listSize = list.Count();
        listSize.ShouldBe(size, $"Expected list size {size} but was {listSize}");
    }

    public static void ShouldHaveSameTime(this DateTime actualTime, DateTime expectedTime)
    {
        actualTime.Hour.ShouldBe(expectedTime.Hour);
        actualTime.Minute.ShouldBe(expectedTime.Minute);
        actualTime.Second.ShouldBe(expectedTime.Second);
    }
}
