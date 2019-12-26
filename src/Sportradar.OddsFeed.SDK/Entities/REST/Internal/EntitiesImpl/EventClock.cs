/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents an event clock
    /// </summary>
    /// <seealso cref="IEventClock" />
    internal class EventClock : EntityPrinter, IEventClock
    {
        /// <summary>
        /// The <see cref="EventTime"/> property backing field
        /// </summary>
        private readonly string _eventTime;

        /// <summary>
        /// The <see cref="EventTime"/> property backing field
        /// </summary>
        private readonly string _stoppageTime;

        /// <summary>
        /// The <see cref="StoppageTimeAnnounced"/> property backing field
        /// </summary>
        private readonly string _stoppageTimeAnnounced;

        /// <summary>
        /// Initializes a new instance of the <see cref="EventClock"/> class.
        /// </summary>
        /// <param name="eventTime">the event time of the sport event associated with the current <see cref="IEventClock" /> instance</param>
        /// <param name="stoppageTime">the <see cref="string" /> representation of the stoppage time the event associated with the current</param>
        /// <param name="stoppageTimeAnnounced"></param>
        /// <param name="remainingDate">the remaining date</param>
        /// <param name="remainingTimeInPeriod">the remaining time in period</param>
        /// <param name="stopped">a value indicating if it is stopped</param>
        public EventClock(string eventTime, string stoppageTime, string stoppageTimeAnnounced, string remainingDate, string remainingTimeInPeriod, bool? stopped)
        {
            _eventTime = eventTime;
            _stoppageTime = stoppageTime;
            _stoppageTimeAnnounced = stoppageTimeAnnounced;
            RemainingDate = remainingDate;
            RemainingTimeInPeriod = remainingTimeInPeriod;
            Stopped = stopped;
        }

        public EventClock(EventClockDTO dto)
        {
            Guard.Argument(dto, nameof(dto)).NotNull();

            _eventTime = dto.EventTime;
            _stoppageTime = dto.StoppageTime;
            _stoppageTimeAnnounced = dto.StoppageTimeAnnounced;
            RemainingDate = dto.RemainingTime;
            RemainingTimeInPeriod = dto.RemainingTimeInPeriod;
            Stopped = dto.IsStopped;
        }

        /// <summary>
        /// Gets the event time of the sport event associated with the current <see cref="IEventClock" /> instance
        /// </summary>
        public string EventTime => _eventTime;

        /// <summary>
        /// Gets the <see cref="string" /> representation of the time the event associated with the current
        /// <see cref="IEventClock" /> has been stopped
        /// </summary>
        public string StoppageTime => _stoppageTime;

        /// <summary>
        /// Gets a value indicating whether the stoppage time has been announced.
        /// </summary>
        public string StoppageTimeAnnounced => _stoppageTimeAnnounced;

        public string RemainingDate { get; }
        public string RemainingTimeInPeriod { get; }
        public bool? Stopped { get; }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"EventTime={_eventTime}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            return $"EventTime={_eventTime}, StoppageTime={_stoppageTime}, StoppageTimeAnnounced={_stoppageTimeAnnounced}, RemainingDate={RemainingDate}, RemainingTimeInPeriod={RemainingTimeInPeriod}, Stopped={Stopped}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string"/> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            return PrintC();
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
