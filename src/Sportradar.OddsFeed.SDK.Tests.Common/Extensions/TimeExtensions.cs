// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Extensions;

public static class TimeExtensions
{
    public static DateTime AddTime(this DateTime dateTime, TimeOnly time)
    {
        return dateTime.AddHours(time.Hour).AddMinutes(time.Minute).AddSeconds(time.Second);
    }

    public static DateTime AddTime(this DateTime? dateTime, TimeOnly time)
    {
        return dateTime?.AddTime(time) ?? DateTime.MinValue.AddTime(time);
    }

    public static string ToDetailedTime(this DateTime dateTime)
    {
        return $"{dateTime.Hour}:{dateTime.Minute}:{dateTime.Second}.{dateTime.Millisecond}";
    }
}
