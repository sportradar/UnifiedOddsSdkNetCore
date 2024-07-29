// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal.Caching;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Api.Managers;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities.Rest;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Enums;
using DateTime = System.DateTime;
// ReSharper disable InconsistentlySynchronizedField

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    internal class EventChangeManager : IEventChangeManager, IDisposable
    {
        private static readonly ILogger LogInt = SdkLoggerFactory.GetLoggerForClientInteraction(typeof(EventChangeManager));
        private static readonly ILogger LogExec = SdkLoggerFactory.GetLoggerForExecution(typeof(EventChangeManager));

        public event EventHandler<EventChangeEventArgs> FixtureChange;

        public event EventHandler<EventChangeEventArgs> ResultChange;

        public DateTime LastFixtureChange { get; private set; }

        public DateTime LastResultChange { get; private set; }

        public TimeSpan FixtureChangeInterval { get; private set; }

        public TimeSpan ResultChangeInterval { get; private set; }

        public bool IsRunning { get; private set; }

        private readonly SdkTimer _fixtureTimer;
        private readonly SdkTimer _resultTimer;
        private readonly SportDataProvider _sportDataProvider;
        private readonly ISportEventCache _sportEventCache;
        private readonly IUofConfiguration _config;
        private readonly object _lockFixture = new object();
        private readonly object _lockResult = new object();
        private bool _isDispatching;
        private readonly ConcurrentDictionary<EventChangeEventArgs, bool> _eventUpdates; // boolean value indicating if this is fixture update

        public EventChangeManager(TimeSpan fixtureChangeInterval,
                                  TimeSpan resultChangeInterval,
                                  ISportDataProvider sportDataProvider,
                                  ISportEventCache sportEventCache,
                                  IUofConfiguration config)
        {
            Guard.Argument(config, nameof(config)).NotNull();
            Guard.Argument(sportDataProvider, nameof(sportDataProvider)).NotNull();
            Guard.Argument(sportEventCache, nameof(sportEventCache)).NotNull();

            _config = config;
            _sportDataProvider = (SportDataProvider)sportDataProvider;
            _sportEventCache = sportEventCache;

            LastFixtureChange = DateTime.MinValue;
            LastResultChange = DateTime.MinValue;
            FixtureChangeInterval = fixtureChangeInterval >= TimeSpan.FromMinutes(1) ? fixtureChangeInterval : TimeSpan.FromHours(1);
            ResultChangeInterval = resultChangeInterval >= TimeSpan.FromMinutes(1) ? resultChangeInterval : TimeSpan.FromHours(1);
            IsRunning = false;

            _fixtureTimer = new SdkTimer(UofSdkBootstrap.TimerForFixtureChange, TimeSpan.FromSeconds(1), FixtureChangeInterval);
            _resultTimer = new SdkTimer(UofSdkBootstrap.TimerForResultChange, TimeSpan.FromSeconds(1), ResultChangeInterval);
            _fixtureTimer.Elapsed += FixtureTimerOnElapsed;
            _resultTimer.Elapsed += ResultTimerOnElapsed;

            _eventUpdates = new ConcurrentDictionary<EventChangeEventArgs, bool>();

            _isDispatching = false;
        }

        public void SetFixtureChangeInterval(TimeSpan fixtureChangeInterval)
        {
            if (fixtureChangeInterval < TimeSpan.FromMinutes(1) || fixtureChangeInterval > TimeSpan.FromHours(12))
            {
                throw new ArgumentException("Interval must be between 1 minute and 12 hours", nameof(fixtureChangeInterval));
            }

            LogInt.LogInformation("Setting new fixture change interval to {Interval} min", fixtureChangeInterval.TotalMinutes.ToString(CultureInfo.InvariantCulture));
            FixtureChangeInterval = fixtureChangeInterval;
            if (IsRunning)
            {
                _fixtureTimer.Start(TimeSpan.FromSeconds(1), FixtureChangeInterval);
            }
        }

        public void SetResultChangeInterval(TimeSpan resultChangeInterval)
        {
            if (resultChangeInterval < TimeSpan.FromMinutes(1) || resultChangeInterval > TimeSpan.FromHours(12))
            {
                throw new ArgumentException("Interval must be between 1 minute and 12 hours", nameof(resultChangeInterval));
            }

            LogInt.LogInformation("Setting new result change interval to {Interval} min", resultChangeInterval.TotalMinutes.ToString(CultureInfo.InvariantCulture));
            ResultChangeInterval = resultChangeInterval;
            if (IsRunning)
            {
                _resultTimer.Start(TimeSpan.FromSeconds(1), ResultChangeInterval);
            }
        }

        public void SetFixtureChangeTimestamp(DateTime fixtureChangeTimestamp)
        {
            if (IsRunning)
            {
                throw new ArgumentException("Manager must first be stopped.", nameof(fixtureChangeTimestamp));
            }
            if (fixtureChangeTimestamp < DateTime.Now.AddDays(-1) || fixtureChangeTimestamp > DateTime.Now)
            {
                throw new ArgumentException("Timestamp must be in the last 24 hours.", nameof(fixtureChangeTimestamp));
            }

            LogInt.LogInformation("Set LastFixtureChange to {Timestamp}", fixtureChangeTimestamp);
            LastFixtureChange = fixtureChangeTimestamp;
        }

        public void SetResultChangeTimestamp(DateTime resultChangeTimestamp)
        {
            if (IsRunning)
            {
                throw new ArgumentException("Manager must first be stopped.", nameof(resultChangeTimestamp));
            }
            if (resultChangeTimestamp < DateTime.Now.AddDays(-1) || resultChangeTimestamp > DateTime.Now)
            {
                throw new ArgumentException("Timestamp must be in the last 24 hours.", nameof(resultChangeTimestamp));
            }

            LogInt.LogInformation("Set LastResultChange to {Timestamp}", resultChangeTimestamp);
            LastResultChange = resultChangeTimestamp;
        }

        public void Start()
        {
            //check if _sportEventCache is disposed
            if (!IsRunning)
            {
                LogInt.LogInformation("Starting periodical fetching of fixture and result changes");
                IsRunning = true;
                _fixtureTimer.Start(TimeSpan.FromSeconds(1), FixtureChangeInterval);
                _resultTimer.Start(TimeSpan.FromSeconds(1), ResultChangeInterval);
            }
            else
            {
                LogInt.LogInformation("Invoking Start of already started process");
            }
        }

        public void Stop()
        {
            if (IsRunning)
            {
                LogInt.LogInformation("Stopping periodical fetching of fixture and result changes");
                IsRunning = false;
            }
            _fixtureTimer.Stop();
            _resultTimer.Stop();
        }

        private void FixtureTimerOnElapsed(object sender, EventArgs e)
        {
            if (!IsRunning)
            {
                LogExec.LogDebug("Invoked fixture change fetch when IsRunning=false");
                return;
            }

            if (FixtureChange == null)
            {
                LogExec.LogDebug("Invoked fixture change fetch when no event handler specified for FixtureChange event. Aborting");
                return;
            }

            lock (_lockFixture)
            {
                if (!IsRunning)
                {
                    return;
                }

                using (var t = new TelemetryTracker(UofSdkTelemetry.EventCacheManagerFixtureChanges))
                {
                    try
                    {
                        IEnumerable<IFixtureChange> changes;
                        if (LastFixtureChange < DateTime.Now.AddDays(-1))
                        {
                            LogExec.LogInformation("Invoking GetFixtureChanges. After=null");
                            changes = _sportDataProvider.GetFixtureChangesAsync(_config.DefaultLanguage).GetAwaiter().GetResult();
                        }
                        else
                        {
                            LogExec.LogInformation("Invoking GetFixtureChanges. After={After}", LastFixtureChange);
                            changes = _sportDataProvider
                                .GetFixtureChangesAsync(LastFixtureChange, null, _config.DefaultLanguage).GetAwaiter()
                                .GetResult();
                        }

                        changes = changes.OrderBy(o => o.UpdateTime);

                        foreach (var fixtureChange in changes)
                        {
                            if (!IsRunning)
                            {
                                break;
                            }

                            var eventUpdate = _eventUpdates.FirstOrDefault(a =>
                                a.Key.SportEventId.Equals(fixtureChange.SportEventId));
                            if (eventUpdate.Key != null)
                            {
                                if (fixtureChange.UpdateTime > eventUpdate.Key.UpdateTime)
                                {
                                    _eventUpdates.TryRemove(eventUpdate.Key, out _);
                                }
                                else
                                {
                                    UpdateLastFixtureChange(fixtureChange.UpdateTime);
                                    continue;
                                }
                            }

                            _sportEventCache.CacheDeleteItem(fixtureChange.SportEventId, CacheItemType.All);
                            var sportEvent = _sportDataProvider.GetSportEventForEventChange(fixtureChange.SportEventId);
                            _eventUpdates.TryAdd(
                                new EventChangeEventArgs(fixtureChange.SportEventId, fixtureChange.UpdateTime,
                                    sportEvent), true);
                            UpdateLastFixtureChange(fixtureChange.UpdateTime);
                        }

                        LogExec.LogInformation("Invoking GetFixtureChanges took {Elapsed} ms", t.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception ex)
                    {
                        LogExec.LogError(ex, "Error fetching fixture changes");
                    }
                }
            }

            DispatchUpdateChangeMessages();
        }

        private void ResultTimerOnElapsed(object sender, EventArgs e)
        {
            if (!IsRunning)
            {
                LogExec.LogDebug("Invoked result change fetch when IsRunning=false");
                return;
            }

            if (ResultChange == null)
            {
                LogExec.LogDebug("Invoked result change fetch when no event handler specified for ResultChange event. Aborting");
                return;
            }

            lock (_lockResult)
            {
                if (!IsRunning)
                {
                    return;
                }

                using (var t = new TelemetryTracker(UofSdkTelemetry.EventCacheManagerResultChanges))
                {
                    try
                    {
                        IEnumerable<IResultChange> changes;
                        if (LastResultChange < DateTime.Now.AddDays(-1))
                        {
                            LogExec.LogInformation("Invoking GetResultChanges. After=null");
                            changes = _sportDataProvider.GetResultChangesAsync(_config.DefaultLanguage).GetAwaiter()
                                .GetResult();
                        }
                        else
                        {
                            LogExec.LogInformation("Invoking GetResultChanges. After={After}", LastResultChange);
                            changes = _sportDataProvider
                                .GetResultChangesAsync(LastResultChange, null, _config.DefaultLanguage).GetAwaiter()
                                .GetResult();
                        }

                        changes = changes.OrderBy(o => o.UpdateTime);

                        foreach (var resultChange in changes)
                        {
                            if (!IsRunning)
                            {
                                break;
                            }

                            var eventUpdate = _eventUpdates.FirstOrDefault(a =>
                                a.Key.SportEventId.Equals(resultChange.SportEventId));
                            if (eventUpdate.Key != null)
                            {
                                if (resultChange.UpdateTime > eventUpdate.Key.UpdateTime)
                                {
                                    _eventUpdates.TryRemove(eventUpdate.Key, out _);
                                }
                                else
                                {
                                    UpdateLastResultChange(resultChange.UpdateTime);
                                    continue;
                                }
                            }

                            _sportEventCache.CacheDeleteItem(resultChange.SportEventId, CacheItemType.All);
                            var sportEvent = _sportDataProvider.GetSportEventForEventChange(resultChange.SportEventId);
                            _eventUpdates.TryAdd(
                                new EventChangeEventArgs(resultChange.SportEventId, resultChange.UpdateTime,
                                    sportEvent), false);
                            UpdateLastResultChange(resultChange.UpdateTime);
                        }

                        LogExec.LogInformation("Invoking GetResultChanges took {Elapsed} ms", t.Elapsed.TotalMilliseconds.ToString(CultureInfo.InvariantCulture));
                    }
                    catch (Exception ex)
                    {
                        LogExec.LogError(ex, "Error fetching result changes");
                    }
                }
            }

            DispatchUpdateChangeMessages();
        }

        private void UpdateLastFixtureChange(DateTime newDate)
        {
            if (newDate > LastFixtureChange)
            {
                LastFixtureChange = newDate;
            }
        }

        private void UpdateLastResultChange(DateTime newDate)
        {
            if (newDate > LastResultChange)
            {
                LastResultChange = newDate;
            }
        }

        private void DispatchUpdateChangeMessages()
        {
            if (_isDispatching)
            {
                return;
            }

            _isDispatching = true;
            foreach (var eventUpdate in _eventUpdates)
            {
                var updateStr = eventUpdate.Value ? "fixture" : "result";
                try
                {
                    LogInt.LogDebug("Dispatching {ChangeType} change [{UpdatesCount}] for {EventId}. Updated={UpdateTime}",
                        updateStr,
                        _eventUpdates.Count.ToString(CultureInfo.InvariantCulture),
                        eventUpdate.Key.SportEventId,
                        eventUpdate.Key.UpdateTime.ToString(CultureInfo.InvariantCulture));
                    if (eventUpdate.Value)
                    {
                        FixtureChange?.Invoke(this, eventUpdate.Key);
                    }
                    else
                    {
                        ResultChange?.Invoke(this, eventUpdate.Key);
                    }

                    _eventUpdates.TryRemove(eventUpdate.Key, out _);
                }
                catch (Exception ex)
                {
                    LogExec.LogWarning(ex, "Error during user processing of event {ChangeType} change message", updateStr);
                }
            }
            _isDispatching = false;
        }

        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        protected virtual void Dispose(bool disposing)
        {
            _fixtureTimer?.Dispose();
            _resultTimer?.Dispose();
            _sportEventCache?.Dispose();
        }
    }
}
