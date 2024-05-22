// Copyright (C) Sportradar AG.See LICENSE for full license governing this code
namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto
{
    internal class EventClockDto
    {
        public string EventTime { get; }

        public string StoppageTime { get; }

        public string StoppageTimeAnnounced { get; }

        public string RemainingTime { get; }

        public string RemainingTimeInPeriod { get; }

        public bool? IsStopped { get; }

        public EventClockDto(string eventTime,
                            string stoppageTime,
                            string stoppageTimeAnnounced,
                            string remainingTime,
                            string remainingTimeInPeriod,
                            bool isStoppedSpecified,
                            bool isStopped)
        {
            EventTime = eventTime;
            StoppageTime = stoppageTime;
            StoppageTimeAnnounced = stoppageTimeAnnounced;
            RemainingTime = remainingTime;
            RemainingTimeInPeriod = remainingTimeInPeriod;
            IsStopped = isStoppedSpecified ? (bool?)isStopped : null;
        }
    }
}
