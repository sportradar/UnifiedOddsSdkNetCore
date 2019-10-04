/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using Sportradar.OddsFeed.SDK.API.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Test.Shared;

namespace Sportradar.OddsFeed.SDK.API.Test
{
    [TestClass]
    public class TimestampTrackerTests
    {
        private static FakeTimeProvider _timeProvider;

        private static readonly Producer PremiumCricketProducer = new Producer(5, "PremiumCricket", "Premium Cricket", "https://api.betradar.com/v1/premium_cricket/", true, 20, 1800, "prematch|live");

        private static readonly FeedMessageBuilder MessageBuilder = new FeedMessageBuilder(PremiumCricketProducer);

        private static readonly MessageInterest[] Interests = {MessageInterest.AllMessages};

        [ClassInitialize]
        public static void Init(TestContext context)
        {
            _timeProvider = new FakeTimeProvider();
            TimeProviderAccessor.SetTimeProvider(_timeProvider);
        }

        [TestInitialize]
        public void Setup()
        {
            _timeProvider.Now = DateTime.Now;
        }

        [TestMethod]
        public void system_alive_timestamp_returns_correct_value()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, Interests, 20, 20);
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.SystemAliveTimestamp);

            _timeProvider.AddSeconds(4);
            tracker.ProcessSystemAlive(MessageBuilder.BuildAlive());
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.SystemAliveTimestamp);
        }

        [TestMethod]
        public void oldest_user_alive_timestamp_returns_correct_value()
        {
            // initially a time of instance creation must be returned
            var tracker = new TimestampTracker(PremiumCricketProducer, new [] {MessageInterest.HighPriorityMessages, MessageInterest.LowPriorityMessages}, 20, 20);
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.OldestUserAliveTimestamp);

            var alive1 = MessageBuilder.BuildAlive();
            // let move the time forward, so the alive above is 4 seconds old
            _timeProvider.AddSeconds(4);
            tracker.ProcessUserMessage(MessageInterest.HighPriorityMessages, alive1);
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(4)), tracker.OldestUserAliveTimestamp);

            // lets create an alive 8 seconds old
            var alive2 = MessageBuilder.BuildAlive(null, _timeProvider.Now - TimeSpan.FromSeconds(8));
            tracker.ProcessUserMessage(MessageInterest.LowPriorityMessages, alive2);
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(8)), tracker.OldestUserAliveTimestamp);

            // lets override the alive above (8 seconds old) with a newer message
            var alive3 = MessageBuilder.BuildAlive();
            tracker.ProcessUserMessage(MessageInterest.LowPriorityMessages, alive3);
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(4)), tracker.OldestUserAliveTimestamp);
        }

        [TestMethod]
        public void violated_gives_correct_value_before_first_alive()
        {
            _timeProvider.Now = DateTime.Now;
            var tracker = new TimestampTracker(PremiumCricketProducer, Interests, 20, 20);
            Assert.IsFalse(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(15);
            Assert.IsFalse(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(10);
            Assert.IsTrue(tracker.IsAliveViolated);
        }

        [TestMethod]
        public void behind_gives_correct_value_before_first_alive()
        {
            _timeProvider.Now = DateTime.Now;
            var tracker = new TimestampTracker(PremiumCricketProducer, Interests, 20, 20);
            Assert.IsFalse(tracker.IsBehind);
            _timeProvider.AddSeconds(10);
            Assert.IsFalse(tracker.IsBehind);
            _timeProvider.AddSeconds(15);
            Assert.IsTrue(tracker.IsBehind);
        }

        [TestMethod]
        public void missing_system_alive_sets_violation()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, Interests, 20, 20);
            _timeProvider.AddSeconds(10);
            tracker.ProcessSystemAlive(MessageBuilder.BuildAlive());
            _timeProvider.AddSeconds(25);
            Assert.IsTrue(tracker.IsAliveViolated);
        }

        [TestMethod]
        public void system_alive_resets_violation()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, Interests, 20, 20);
            _timeProvider.AddSeconds(25);
            Assert.IsTrue(tracker.IsAliveViolated);
            tracker.ProcessSystemAlive(MessageBuilder.BuildAlive());
            Assert.IsFalse(tracker.IsAliveViolated);
        }

        [TestMethod]
        public void violated_gives_correct_value()
        {
            var alive = new alive();
            _timeProvider.Now = DateTime.Now;
            var tracker = new TimestampTracker(PremiumCricketProducer, Interests, 20, 20);
            Assert.IsFalse(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(10);
            Assert.IsFalse(tracker.IsAliveViolated);

            alive.timestamp = SdkInfo.ToEpochTime(_timeProvider.Now);
            tracker.ProcessSystemAlive(alive);
            Assert.IsFalse(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(15);
            Assert.IsFalse(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(10);
            Assert.IsTrue(tracker.IsAliveViolated);

            alive.timestamp = SdkInfo.ToEpochTime(_timeProvider.Now);
            tracker.ProcessSystemAlive(alive);
            Assert.IsFalse(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(10);
            Assert.IsFalse(tracker.IsAliveViolated);
            _timeProvider.AddSeconds(15);
            Assert.IsTrue(tracker.IsAliveViolated);
        }

        [TestMethod]
        public void delayed_message_sets_is_behind()
        {
            var oddsChange = MessageBuilder.BuildOddsChange(null, _timeProvider.Now - TimeSpan.FromSeconds(25));
            var tracker = new TimestampTracker(PremiumCricketProducer, Interests, 20, 20);
            tracker.ProcessUserMessage(MessageInterest.AllMessages, oddsChange);
            Assert.IsTrue(tracker.IsBehind);
        }

        [TestMethod]
        public void not_delayed_message_resets_is_behind()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, Interests, 20, 20);
            var oddsChange = MessageBuilder.BuildOddsChange(null, _timeProvider.Now - TimeSpan.FromSeconds(25));
            tracker.ProcessUserMessage(MessageInterest.AllMessages, oddsChange);
            Assert.IsTrue(tracker.IsBehind);
            tracker.ProcessUserMessage(MessageInterest.AllMessages, MessageBuilder.BuildBetStop());
            Assert.IsFalse(tracker.IsBehind);
        }

        [TestMethod]
        public void user_alives_on_all_sessions_are_required_to_reset_is_behind()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, new [] {MessageInterest.LiveMessagesOnly, MessageInterest.PrematchMessagesOnly}, 20, 20);
            _timeProvider.AddSeconds(25);
            var alive = MessageBuilder.BuildAlive();
            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, alive);
            Assert.IsTrue(tracker.IsBehind);
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, alive);
            _timeProvider.AddSeconds(6);
            Assert.IsFalse(tracker.IsBehind);
        }

        [TestMethod]
        public void user_alive_resets_behind_caused_by_non_alive()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, Interests, 20, 20);
            var oddsChange = MessageBuilder.BuildOddsChange(null, _timeProvider.Now - TimeSpan.FromSeconds(25));
            tracker.ProcessUserMessage(MessageInterest.AllMessages, oddsChange);
            Assert.IsTrue(tracker.IsBehind);
            tracker.ProcessUserMessage(MessageInterest.AllMessages, MessageBuilder.BuildAlive());
            Assert.IsFalse(tracker.IsBehind);
        }

        [TestMethod]
        public void oldest_alive_timestamp_gives_correct_value()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, new [] {MessageInterest.LiveMessagesOnly,MessageInterest.PrematchMessagesOnly}, 20, 20);
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.OldestUserAliveTimestamp);

            _timeProvider.AddSeconds(5);
            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, MessageBuilder.BuildAlive());
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(5)), tracker.OldestUserAliveTimestamp);

            _timeProvider.AddSeconds(5);
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, MessageBuilder.BuildAlive());
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now - TimeSpan.FromSeconds(5)), tracker.OldestUserAliveTimestamp);
        }

        [TestMethod]
        public void tracker_with_no_interests_never_false_behind()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, new[] {MessageInterest.VirtualSportMessages }, 20, 20);
            Assert.IsFalse(tracker.IsBehind);
            _timeProvider.AddSeconds(30);
            Assert.IsFalse(tracker.IsBehind);

        }

        [TestMethod]
        public void tracker_with_no_interests_always_returns_current_time_for_oldest_alive_timestamp()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, new[] { MessageInterest.VirtualSportMessages }, 20, 20);
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.OldestUserAliveTimestamp);
            _timeProvider.AddSeconds(10);
            Assert.AreEqual(SdkInfo.ToEpochTime(_timeProvider.Now), tracker.OldestUserAliveTimestamp);
        }

        private static readonly Regex TimestampTrackerStatusRegex = new Regex(@"Ping\((?<ping_state>[a-zA-Z]{2,})\):age=(?<ping_age>\d{2}:\d{2}:\d{2}\.\d{3})\],\[IsBehind\((?<behind_state>[a-zA-Z]{4,5})\):Alive\(s\)\[(?<user_alives>[a-zA-Z0-9:.,=]{2,})\],NonAlives\[(?<non_alives>[a-zA-Z0-9:.,=]{2,})\]");
        private static readonly Regex TimingEntryRegex = new Regex(@"(?<interest>[a-zA-Z_]{2,}):[a-z]{2,}=(?<interval>\d{2}:\d{2}:\d{2}\.\d{3})");

        [TestMethod]
        public void internal_state_on_creation_is_correct()
        {
            var tracker = new TimestampTracker(PremiumCricketProducer, new [] {MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly}, 20, 20);
            var trackerState = ParseLogEntry(tracker.ToString());
            Assert.IsFalse(trackerState.IsAliveViolated);
            Assert.IsFalse(trackerState.IsBehind);
            Assert.AreEqual(TimeSpan.Zero, trackerState.SystemAliveAge);
            Assert.AreEqual(TimeSpan.Zero, trackerState.Alives[MessageInterest.PrematchMessagesOnly]);
            Assert.AreEqual(TimeSpan.Zero, trackerState.Alives[MessageInterest.LiveMessagesOnly]);
            Assert.AreEqual(TimeSpan.Zero, trackerState.NonAlives[MessageInterest.PrematchMessagesOnly]);
            Assert.AreEqual(TimeSpan.Zero, trackerState.NonAlives[MessageInterest.LiveMessagesOnly]);
        }

        [TestMethod]
        public void processed_value_are_correctly_stored()
        {
            //set to some round date to avoid rounding issues
            _timeProvider.Now = new DateTime(2000, 1, 1, 1, 1, 1, 0);
            var tracker = new TimestampTracker(PremiumCricketProducer, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly }, 20, 20);

            tracker.ProcessSystemAlive(MessageBuilder.BuildAlive());
            _timeProvider.AddMilliSeconds(2500);

            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, MessageBuilder.BuildAlive());
            _timeProvider.AddMilliSeconds(1000);

            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, MessageBuilder.BuildAlive());
            _timeProvider.AddMilliSeconds(1500);

            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, MessageBuilder.BuildBetStop(null, _timeProvider.Now - TimeSpan.FromMilliseconds(6500)));
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, MessageBuilder.BuildOddsChange(null, _timeProvider.Now - TimeSpan.FromMilliseconds(4300)));

            var trackerState = ParseLogEntry(tracker.ToString());
            Assert.IsFalse(trackerState.IsAliveViolated);
            Assert.IsFalse(trackerState.IsBehind);
            Assert.AreEqual(TimeSpan.FromMilliseconds(5000), trackerState.SystemAliveAge);
            Assert.AreEqual(TimeSpan.FromMilliseconds(2500), trackerState.Alives[MessageInterest.LiveMessagesOnly]);
            Assert.AreEqual(TimeSpan.FromMilliseconds(1500), trackerState.Alives[MessageInterest.PrematchMessagesOnly]);
            Assert.AreEqual(TimeSpan.FromMilliseconds(6500), trackerState.NonAlives[MessageInterest.LiveMessagesOnly]);
            Assert.AreEqual(TimeSpan.FromMilliseconds(4300), trackerState.NonAlives[MessageInterest.PrematchMessagesOnly]);
        }

        [TestMethod]
        public void alive_with_lower_latency_overrides_non_alive_with_greater_latency()
        {
            //set to some round date to avoid rounding issues
            _timeProvider.Now = new DateTime(2000, 1, 1, 1, 1, 1, 0);
            var tracker = new TimestampTracker(PremiumCricketProducer, new[] { MessageInterest.PrematchMessagesOnly, MessageInterest.LiveMessagesOnly }, 20, 20);

            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, MessageBuilder.BuildBetStop(null, _timeProvider.Now - TimeSpan.FromMilliseconds(5000)));
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, MessageBuilder.BuildOddsChange(null, _timeProvider.Now - TimeSpan.FromMilliseconds(7000)));

            tracker.ProcessUserMessage(MessageInterest.LiveMessagesOnly, MessageBuilder.BuildAlive(null, _timeProvider.Now - TimeSpan.FromSeconds(4)));
            tracker.ProcessUserMessage(MessageInterest.PrematchMessagesOnly, MessageBuilder.BuildAlive(null, _timeProvider.Now - TimeSpan.FromSeconds(8)));


            var trackerState = ParseLogEntry(tracker.ToString());
            Assert.IsFalse(trackerState.IsAliveViolated);
            Assert.IsFalse(trackerState.IsBehind);
            Assert.AreEqual(TimeSpan.FromSeconds(0), trackerState.SystemAliveAge);
            Assert.AreEqual(TimeSpan.FromSeconds(4), trackerState.Alives[MessageInterest.LiveMessagesOnly]);
            Assert.AreEqual(TimeSpan.FromSeconds(8), trackerState.Alives[MessageInterest.PrematchMessagesOnly]);
            Assert.AreEqual(TimeSpan.FromSeconds(4), trackerState.NonAlives[MessageInterest.LiveMessagesOnly]);
            Assert.AreEqual(TimeSpan.FromSeconds(7), trackerState.NonAlives[MessageInterest.PrematchMessagesOnly]);
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
