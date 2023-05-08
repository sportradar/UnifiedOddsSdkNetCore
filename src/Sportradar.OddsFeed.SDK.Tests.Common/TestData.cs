/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.Events;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.Common
{
    public static class TestData
    {
        public static readonly string RestXmlPath = Directory.GetCurrentDirectory() + "/REST XMLs/";
        public static readonly string FeedXmlPath = Directory.GetCurrentDirectory() + "/XMLs/";

        public static readonly int BookmakerId = 1;
        public static readonly string AccessToken = "token";
        public static readonly string VirtualHost = "/virtualhost";

        public static readonly Uri BadUri = new Uri("http://www.unexisting-url.com");
        public static readonly Uri GetUri = new Uri("http://test.domain.com/get");
        public static readonly Uri PostUri = new Uri("http://test.domain.com/post");
        public static readonly string DelayedUrl = "https://app.requestly.io/delay/{0}/https://httpbin.org/get";

        public static readonly URN SportId = URN.Parse("sr:sport:1");
        public static readonly URN CategoryId = URN.Parse("sr:category:1");
        public static readonly URN TournamentId = URN.Parse("sr:tournament:1");
        public static readonly URN SimpleTournamentId = URN.Parse("sr:simple_tournament:1");
        public static readonly URN SeasonId = URN.Parse("sr:season:1");

        public static readonly URN EventMatchId = URN.Parse("sr:match:9210275");
        public static readonly URN EventStageId = URN.Parse("sr:stage:940265");
        public static readonly URN SimpleTournamentId11111 = URN.Parse("sr:simple_tournament:11111");

        public static readonly CultureInfo Culture = new CultureInfo("en");
        public static IReadOnlyCollection<CultureInfo> Cultures => Cultures3;
        public static IReadOnlyCollection<CultureInfo> Cultures1 => new Collection<CultureInfo> { Cultures3.First() };
        public static readonly IReadOnlyCollection<CultureInfo> Cultures3 = new Collection<CultureInfo>(new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("hu") });
        public static readonly IReadOnlyCollection<CultureInfo> Cultures4 = new Collection<CultureInfo>(new[] { new CultureInfo("en"), new CultureInfo("de"), new CultureInfo("hu"), new CultureInfo("nl") });
        public static readonly CultureInfo CultureNl = new CultureInfo("nl");

        public const int CacheSportCount = 136;
        public const int CacheCategoryCount = 391;
        public const int CacheCategoryCountPlus = 408;
        public const int CacheTournamentCount = 8455;

        public const int InvariantListCacheCount = 1080;
        public const int VariantListCacheCount = 110;

        public const string SdkTestLogRepositoryName = "SdkTestLogRepositoryName";
        public const ExceptionHandlingStrategy ThrowingStrategy = ExceptionHandlingStrategy.THROW;

        internal static void ValidateTestEventId(MatchCI ci, IEnumerable<CultureInfo> cultures, bool canHaveOtherLanguage)
        {
            Assert.NotNull(ci);
            Assert.Equal(EventMatchId, ci.Id);

            if (cultures == null)
            {
                Assert.Null(ci);
            }

            var date = new DateTime?();
            RoundCI round = null;
            SeasonCI season = null;

            var cultureInfos = cultures.ToList();
            var cStr = string.Join(",", cultureInfos);
            var lStr = string.Join(",", ci.LoadedSummaries);
            Assert.True(ci.HasTranslationsFor(cultureInfos), $"Culture missing: {cStr}, loaded summaries: {lStr}");
            if (!canHaveOtherLanguage)
            {
                Assert.True(ci.LoadedSummaries.Count >= cultureInfos.Count || ci.LoadedFixtures.Count >= cultureInfos.Count);
            }
            foreach (var culture in cultureInfos)
            {
                var checkCulture = new[] { culture };
                Task.Run(async () =>
                         {
                             date = await ci.GetScheduledAsync();
                             round = await ci.GetTournamentRoundAsync(checkCulture);
                             season = await ci.GetSeasonAsync(checkCulture);
                         }).GetAwaiter().GetResult();

                Assert.NotNull(date);
                Assert.Equal(new DateTime(2016, 08, 10), new DateTime(date.Value.Year, date.Value.Month, date.Value.Day));

                Assert.True(string.IsNullOrEmpty(round.GetName(culture)));

                Assert.True(Math.Max(ci.LoadedSummaries.Count, ci.LoadedFixtures.Count) >= season.Names.Count);
            }
        }

        public static void FillMessageTimestamp(FeedMessage message)
        {
            if (message != null)
            {
                var currentTimestamp = SdkInfo.ToEpochTime(DateTime.Now);

                message.SentAt = message.ReceivedAt = currentTimestamp;
            }
        }

        public static string GetDelayedUrl(TimeSpan delay)
        {
            return string.Format(DelayedUrl, delay.TotalMilliseconds);
        }

        public static string GetDelayedUrl(int delayMs)
        {
            return string.Format(DelayedUrl, delayMs);
        }
    }
}
