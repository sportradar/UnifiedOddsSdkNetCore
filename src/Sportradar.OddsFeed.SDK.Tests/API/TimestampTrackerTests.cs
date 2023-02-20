/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/

using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using FluentAssertions.Execution;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Tests.Common;
using Xunit;

namespace Sportradar.OddsFeed.SDK.Tests.API
{
    public class TimestampTrackerTests
    {
        private readonly FakeTimeProvider _timeProvider;
        private readonly Producer _premiumCricketProducer;
        private readonly FeedMessageBuilder _messageBuilder;
        private readonly MessageInterest[] _interests;

        public TimestampTrackerTests()
        {
            _premiumCricketProducer = new Producer(5, "PremiumCricket", "Premium Cricket", "https://api.betradar.com/v1/premium_cricket/", true, 20, 1800, "prematch|live", 4320);
            _messageBuilder = new FeedMessageBuilder(_premiumCricketProducer.Id);
            _interests = new[] { MessageInterest.AllMessages };

            _timeProvider = new FakeTimeProvider();
            TimeProviderAccessor.SetTimeProvider(_timeProvider);
            _timeProvider.Now = DateTime.Now;
        }

        [Fact]
        public void System_alive_timestamp_returns_correct_value()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, _interests, 20, 20);
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.SystemAliveTimestamp);

            _timeProvider.AddSeconds(4);
            tracker.ProcessSystemAlive(_messageBuilder.BuildAlive());
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.SystemAliveTimestamp);
        }

        [Fact]
        public void Oldest_user_alive_timestamp_returns_correct_value()
        {
            // initially a time of instance creation must be returned
            var tracker = new TimestampTracker(_premiumCricketProducer, new[] { MessageInterest.HighPriorityMessages, MessageInterest.LowPriorityMessages }, 20, 20);
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.OldestUserAliveTimestamp);

            var alive1 = _messageBuilder.BuildAlive();
            // let move the time forward, so the alive above is 4 seconds old
            _timeProvider.AddSeconds(4);
            tracker.ProcessUserMessage(MessageInterest.HighPriorityMessages, alive1);
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(4)), tracker.OldestUserAliveTimestamp);

            // lets create an alive 8 seconds old
            var alive2 = _messageBuilder.BuildAlive(null, _timeProvider.Now - TimeSpan.FromSeconds(8));
            tracker.ProcessUserMessage(MessageInterest.LowPriorityMessages, alive2);
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(8)), tracker.OldestUserAliveTimestamp);

            // lets override the alive above (8 seconds old) with a newer message
            var alive3 = _messageBuilder.BuildAlive();
            tracker.ProcessUserMessage(MessageInterest.LowPriorityMessages, alive3);
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(4)), tracker.OldestUserAliveTimestamp);
        }

        [Fact]
        public void Violated_gives_correct_value_before_first_alive()
        {
            _timeProvider.Now = DateTime.Now;
            var tracker = new TimestampTracker(_premiumCricketProducer, _interests, 20, 20);
            Assert.False(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(15);
            Assert.False(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(10);
            Assert.True(tracker.IsAliveViolated);
        }

        [Fact]
        public void Behind_gives_correct_value_before_first_alive()
        {
            _timeProvider.Now = DateTime.Now;
            var tracker = new TimestampTracker(_premiumCricketProducer, _interests, 20, 20);
            Assert.False(tracker.IsBehind);
            _timeProvider.AddSeconds(10);
            Assert.False(tracker.IsBehind);
            _timeProvider.AddSeconds(15);
            Assert.True(tracker.IsBehind);
        }

        [Fact]
        public void Missing_system_alive_sets_violation()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, _interests, 20, 20);
            _timeProvider.AddSeconds(10);
            tracker.ProcessSystemAlive(_messageBuilder.BuildAlive());
            _timeProvider.AddSeconds(25);
            Assert.True(tracker.IsAliveViolated);
        }

        [Fact]
        public void System_alive_resets_violation()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, _interests, 20, 20);
            _timeProvider.AddSeconds(25);
            Assert.True(tracker.IsAliveViolated);
            tracker.ProcessSystemAlive(_messageBuilder.BuildAlive());
            Assert.False(tracker.IsAliveViolated);
        }

        [Fact]
        public void Violated_gives_correct_value()
        {
            var alive = new alive();
            _timeProvider.Now = DateTime.Now;
            var tracker = new TimestampTracker(_premiumCricketProducer, _interests, 20, 20);
            Assert.False(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(10);
            Assert.False(tracker.IsAliveViolated);

            alive.timestamp = SdkInfo.ToEpochTime(_timeProvider.Now);
            tracker.ProcessSystemAlive(alive);
            Assert.False(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(15);
            Assert.False(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(10);
            Assert.True(tracker.IsAliveViolated);

            alive.timestamp = SdkInfo.ToEpochTime(_timeProvider.Now);
            tracker.ProcessSystemAlive(alive);
            Assert.False(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(10);
            Assert.False(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(15);
            Assert.True(tracker.IsAliveViolated);
        }

        [Fact]
        public void Delayed_message_sets_is_behind()
        {
            var oddsChange = _messageBuilder.BuildOddsChange(null, null, null, _timeProvider.Now - TimeSpan.FromSeconds(25));
            var tracker = new TimestampTracker(_premiumCricketProducer, _interests, 20, 20);
            tracker.ProcessUserMessage(MessageInterest.AllMessages, oddsChange);
            Assert.True(tracker.IsBehind);
        }

        [Fact]
        public void Not_delayed_message_resets_is_behind()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, _interests, 20, 20);
            var oddsChange = _messageBuilder.BuildOddsChange(null, null, null, _timeProvider.Now - TimeSpan.FromSeconds(25));
            tracker.ProcessUserMessage(MessageInterest.AllMessages, oddsChange);
            Assert.True(tracker.IsBehind);
            tracker.ProcessUserMessage(MessageInterest.AllMessages, _messageBuilder.BuildBetStop());
            Assert.False(tracker.IsBehind);
        }

        [Fact]
        public void User_alives_on_all_sessions_are_required_to_reset_is_behind()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, new[] { MessageInterest.LiveMessagesOnly, MessageInterest.PrematchMessagesOnly }, 20, 20);
            _timeProvider.AddSeconds(25);
            var alive = _messageBuilder.BuildAlive();
            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, alive);
            Assert.True(tracker.IsBehind);
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, alive);
            _timeProvider.AddSeconds(6);
            Assert.False(tracker.IsBehind);
        }

        [Fact]
        public void User_alive_resets_behind_caused_by_non_alive()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, _interests, 20, 20);
            var oddsChange = _messageBuilder.BuildOddsChange(null, null, null, _timeProvider.Now - TimeSpan.FromSeconds(25));
            tracker.ProcessUserMessage(MessageInterest.AllMessages, oddsChange);
            Assert.True(tracker.IsBehind);
            tracker.ProcessUserMessage(MessageInterest.AllMessages, _messageBuilder.BuildAlive());
            Assert.False(tracker.IsBehind);
        }

        [Fact]
        public void Oldest_alive_timestamp_gives_correct_value()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, new[] { MessageInterest.LiveMessagesOnly, MessageInterest.PrematchMessagesOnly }, 20, 20);
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.OldestUserAliveTimestamp);

            _timeProvider.AddSeconds(5);
            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, _messageBuilder.BuildAlive());
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(5)), tracker.OldestUserAliveTimestamp);

            _timeProvider.AddSeconds(5);
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, _messageBuilder.BuildAlive());
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(5)), tracker.OldestUserAliveTimestamp);
        }

        [Fact]
        public void Tracker_with_no_interests_never_false_behind()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, new[] { MessageInterest.VirtualSportMessages }, 20, 20);
            Assert.False(tracker.IsBehind);
            _timeProvider.AddSeconds(30);
            Assert.False(tracker.IsBehind);

        }

        [Fact]
        public void Tracker_with_no_interests_always_returns_current_time_for_oldest_alive_timestamp()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, new[] { MessageInterest.VirtualSportMessages }, 20, 20);
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.OldestUserAliveTimestamp);
            _timeProvider.AddSeconds(10);
            Assert.Equal(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.OldestUserAliveTimestamp);
        }

        private static readonly Regex TimestampTrackerStatusRegex = new Regex(@"Ping\((?<ping_state>[a-zA-Z]{2,})\):age=(?<ping_age>\d{2}:\d{2}:\d{2}\.\d{3})\],\[IsBehind\((?<behind_state>[a-zA-Z]{4,5})\):Alive\(s\)\[(?<user_alives>[a-zA-Z0-9:.,=]{2,})\],NonAlives\[(?<non_alives>[a-zA-Z0-9:.,=]{2,})\]");
        private static readonly Regex TimingEntryRegex = new Regex(@"(?<interest>[a-zA-Z_]{2,}):[a-z]{2,}=(?<interval>\d{2}:\d{2}:\d{2}\.\d{3})");

        // this test seems to be a bit unreliable - sometimes fails
        [Fact]
        public void Internal_state_on_creation_is_correct()
        {
            var tracker = new TimestampTracker(_premiumCricketProducer, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly }, 20, 20);
            var trackerState = ParseLogEntry(tracker.ToString());
            Assert.False(trackerState.IsAliveViolated);
            Assert.False(trackerState.IsBehind);
            Assert.Equal(TimeSpan.Zero, trackerState.SystemAliveAge);
            Assert.Equal(TimeSpan.Zero, trackerState.Alives[MessageInterest.PrematchMessagesOnly]);
            Assert.Equal(TimeSpan.Zero, trackerState.Alives[MessageInterest.LiveMessagesOnly]);
            Assert.Equal(TimeSpan.Zero, trackerState.NonAlives[MessageInterest.PrematchMessagesOnly]);
            Assert.Equal(TimeSpan.Zero, trackerState.NonAlives[MessageInterest.LiveMessagesOnly]);
        }

        [Fact]
        public void Processed_value_are_correctly_stored()
        {
            //set to some round date to avoid rounding issues
            _timeProvider.Now = new DateTime(2000, 1, 1, 1, 1, 1, 0);
            var tracker = new TimestampTracker(_premiumCricketProducer, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly }, 20, 20);

            tracker.ProcessSystemAlive(_messageBuilder.BuildAlive());
            _timeProvider.AddMilliSeconds(2500);

            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, _messageBuilder.BuildAlive());
            _timeProvider.AddMilliSeconds(1000);

            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, _messageBuilder.BuildAlive());
            _timeProvider.AddMilliSeconds(1500);

            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, _messageBuilder.BuildBetStop(null, null, null, _timeProvider.Now - TimeSpan.FromMilliseconds(6500)));
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, _messageBuilder.BuildOddsChange(null, null, null, _timeProvider.Now - TimeSpan.FromMilliseconds(4300)));

            var trackerState = ParseLogEntry(tracker.ToString());
            Assert.False(trackerState.IsAliveViolated);
            Assert.False(trackerState.IsBehind);
            Assert.Equal(TimeSpan.FromMilliseconds(5000), trackerState.SystemAliveAge);
            Assert.Equal(TimeSpan.FromMilliseconds(2500), trackerState.Alives[MessageInterest.LiveMessagesOnly]);
            Assert.Equal(TimeSpan.FromMilliseconds(1500), trackerState.Alives[MessageInterest.PrematchMessagesOnly]);
            Assert.Equal(TimeSpan.FromMilliseconds(6500), trackerState.NonAlives[MessageInterest.LiveMessagesOnly]);
            Assert.Equal(TimeSpan.FromMilliseconds(4300), trackerState.NonAlives[MessageInterest.PrematchMessagesOnly]);
        }

        [Fact]
        public void Alive_with_lower_latency_overrides_non_alive_with_greater_latency()
        {
            //set to some round date to avoid rounding issues
            _timeProvider.Now = new DateTime(2000, 1, 1, 1, 1, 1, 0);
            var tracker = new TimestampTracker(_premiumCricketProducer, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly }, 20, 20);

            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, _messageBuilder.BuildBetStop(null, null, null, _timeProvider.Now - TimeSpan.FromMilliseconds(5000)));
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, _messageBuilder.BuildOddsChange(null, null, null, _timeProvider.Now - TimeSpan.FromMilliseconds(7000)));

            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, _messageBuilder.BuildAlive(null, _timeProvider.Now - TimeSpan.FromSeconds(4)));
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, _messageBuilder.BuildAlive(null, _timeProvider.Now - TimeSpan.FromSeconds(8)));

            var trackerState = ParseLogEntry(tracker.ToString());

            using (new AssertionScope())
            {
                Assert.False(trackerState.IsAliveViolated);
                Assert.False(trackerState.IsBehind);
                Assert.Equal(TimeSpan.FromSeconds(0), trackerState.SystemAliveAge);
                Assert.Equal(TimeSpan.FromSeconds(4), trackerState.Alives[MessageInterest.LiveMessagesOnly]);
                Assert.Equal(TimeSpan.FromSeconds(8), trackerState.Alives[MessageInterest.PrematchMessagesOnly]);
                Assert.Equal(TimeSpan.FromSeconds(4), trackerState.NonAlives[MessageInterest.LiveMessagesOnly]);
                Assert.Equal(TimeSpan.FromSeconds(7), trackerState.NonAlives[MessageInterest.PrematchMessagesOnly]);
            }
        }

        private static TimestampTrackerState ParseLogEntry(string entry)
        {
            var match = TimestampTrackerStatusRegex.Match(entry);
            if (!match.Success)
            {
                throw new FormatException($"Format of the {nameof(entry)} argument is not correct");
            }

            return new TimestampTrackerState(
                match.Groups["ping_state"].Value == "Failed",
                match.Groups["behind_state"].Value == "True",
                DateTime.ParseExact(match.Groups["ping_age"].Value, "hh:mm:ss.fff", CultureInfo.InvariantCulture).TimeOfDay,
                GetTimingEntries(match.Groups["user_alives"].Value),
                GetTimingEntries(match.Groups["non_alives"].Value));
        }

        private static IDictionary<MessageInterest, TimeSpan> GetTimingEntries(string timingEntriesString)
        {
            var itemList = from Match aliveEntryMatch
                    in TimingEntryRegex.Matches(timingEntriesString)
                           select new
                           {
                               Interest = MessageInterest.DefinedInterests.FirstOrDefault(i => i.Name == aliveEntryMatch.Groups["interest"].Value),
                               Interval = DateTime.ParseExact(aliveEntryMatch.Groups["interval"].Value, "hh:mm:ss.fff", CultureInfo.InvariantCulture).TimeOfDay
                           };
            return itemList.ToDictionary(item => item.Interest, item => item.Interval);
        }
    }

    public class TimestampTrackerState
    {
        public bool IsAliveViolated { get; }

        public bool IsBehind { get; }

        public TimeSpan SystemAliveAge { get; }

        public IDictionary<MessageInterest, TimeSpan> Alives { get; }

        public IDictionary<MessageInterest, TimeSpan> NonAlives { get; }

        public TimestampTrackerState(bool isAliveViolated, bool isBehind, TimeSpan systemAliveAge, IDictionary<MessageInterest, TimeSpan> alives, IDictionary<MessageInterest, TimeSpan> nonAlives)
        {
            IsAliveViolated = isAliveViolated;
            IsBehind = isBehind;
            SystemAliveAge = systemAliveAge;
            Alives = alives;
            NonAlives = nonAlives;
        }
    }
}
