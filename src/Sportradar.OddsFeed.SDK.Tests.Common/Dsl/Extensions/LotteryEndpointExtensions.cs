// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Globalization;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Extensions;

internal static class LotteryEndpointExtensions
{
    public static Uri GetLotteriesUri(this lotteries lotteries, string baseUrl, CultureInfo language)
    {
        return new Uri($"{baseUrl}/v1/wns/{language.TwoLetterISOLanguageName}/lotteries.xml");
    }

    public static Uri GetLotteryScheduleUri(this lottery_schedule lotterySchedule, string baseUrl, CultureInfo language)
    {
        return new Uri($"{baseUrl}/v1/wns/{language.TwoLetterISOLanguageName}/lotteries/{lotterySchedule.lottery.id}/schedule.xml");
    }

    public static Uri GetDrawSummaryUri(this draw_summary drawSummary, string baseUrl, CultureInfo language)
    {
        return new Uri($"{baseUrl}/v1/wns/{language.TwoLetterISOLanguageName}/sport_events/{drawSummary.draw_fixture.id}/summary.xml");
    }
}
