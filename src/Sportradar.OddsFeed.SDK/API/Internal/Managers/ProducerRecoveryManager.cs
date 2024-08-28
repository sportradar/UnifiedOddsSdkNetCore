// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Diagnostics.CodeAnalysis;
using System.Threading;
using System.Threading.Tasks;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.EventArguments;
using Sportradar.OddsFeed.SDK.Api.Internal.Recovery;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Managers
{
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    internal class ProducerRecoveryManager : IProducerRecoveryManager
    {
        /// <summary>
        /// A <see cref="ILogger"/> used for execution logging
        /// </summary>
        private static readonly ILogger ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(ProducerRecoveryManager));

        /// <summary>
        /// The producer
        /// </summary>
        private readonly Producer _producer;

        /// <summary>
        /// A <see cref="IRecoveryOperation"/> used to issue and track recovery operations
        /// </summary>
        private readonly IRecoveryOperation _recoveryOperation;

        /// <summary>
        /// A <see cref="ITimestampTracker"/> used to track message timings
        /// </summary>
        private readonly ITimestampTracker _timestampTracker;

        /// <summary>
        /// A <see cref="object"/> used to ensure synchronous access to critical regions
        /// </summary>
        private readonly object _syncLock = new object();

        /// <summary>
        /// A <see cref="SemaphoreSlim"/> used to synchronize status changes
        /// </summary>
        private readonly SemaphoreSlim _semaphoreStatusChange = new SemaphoreSlim(1);

        /// <summary>
        /// The minimal interval between recovery requests initiated by alive messages (seconds)
        /// </summary>
        private readonly int _minIntervalBetweenRecoveryRequests;

        /// <summary>
        /// A <see cref="IProducer"/> for which status is tracked
        /// </summary>
        public IProducer Producer { get; }

        /// <summary>
        /// Gets the status of the recovery manager
        /// </summary>
        /// <value>The status</value>
        public ProducerRecoveryStatus Status { get; private set; }

        /// <summary>
        /// Occurs when status of the associated manager has changed
        /// </summary>
        public event EventHandler<TrackerStatusChangeEventArgs> StatusChanged;

        /// <summary>
        /// Occurs when a requested event recovery completes
        /// </summary>
        public event EventHandler<EventRecoveryCompletedEventArgs> EventRecoveryCompleted;

        /// <summary>
        /// The time of last feed message associated with recovery
        /// </summary>
        private DateTime _lastRecoveryMessage;

        /// <summary>
        /// The timestamp when the connection went down (if down)
        /// </summary>
        private DateTime _connectionDownTimestamp;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducerRecoveryManager"/> class
        /// </summary>
        /// <param name="producer">A <see cref="IProducer"/> for which status is tracked</param>
        /// <param name="recoveryOperation">A <see cref="IRecoveryOperation"/> used to issue and track recovery operations</param>
        /// <param name="timestampTracker">A <see cref="ITimestampTracker"/> used to track message timings</param>
        /// <param name="minIntervalBetweenRecoveryRequests">The minimal interval between recovery requests initiated by alive messages (seconds)</param>
        internal ProducerRecoveryManager(IProducer producer, IRecoveryOperation recoveryOperation, ITimestampTracker timestampTracker, int minIntervalBetweenRecoveryRequests)
        {
            Guard.Argument(producer, nameof(producer)).NotNull();
            Guard.Argument(recoveryOperation, nameof(recoveryOperation)).NotNull();
            Guard.Argument(timestampTracker, nameof(timestampTracker)).NotNull();

            Producer = producer;
            _producer = (Producer)producer;
            _recoveryOperation = recoveryOperation;
            _timestampTracker = timestampTracker;
            _minIntervalBetweenRecoveryRequests = minIntervalBetweenRecoveryRequests;

            Status = ProducerRecoveryStatus.NotStarted;
            _lastRecoveryMessage = DateTime.Now;
            _connectionDownTimestamp = DateTime.MinValue;
        }

        /// <summary>
        /// Sets the <see cref="Status"/> property and raised the <see cref="StatusChanged"/> event
        /// </summary>
        /// <param name="requestId">The requestId of the recovery request which caused the change or a null reference</param>
        /// <param name="newStatus">The <see cref="ProducerRecoveryStatus"/> specifying the new status</param>
        /// <remarks>Should be used only within locking</remarks>
        private void SetStatusAndRaiseEvent(long? requestId, ProducerRecoveryStatus newStatus)
        {
            var oldStatus = Status;

            if (Status == newStatus)
            {
                ExecutionLog.LogDebug("{ProducerName}: Attempting to set status to an existing value {Status}. Aborting ...", Producer.Name, Status);
                return;
            }

            ExecutionLog.LogInformation("{ProducerName} Status changed from {OldStatus} to {NewStatus}", Producer.Name, oldStatus, newStatus);

            Status = newStatus;
            _producer.SetProducerDown(newStatus != ProducerRecoveryStatus.Completed);

            _ = Task.Run(() =>
                         {
                             try
                             {
                                 _semaphoreStatusChange.Wait();
                                 StatusChanged?.Invoke(this, new TrackerStatusChangeEventArgs(requestId, oldStatus, newStatus));
                             }
                             catch (Exception ex)
                             {
                                 ExecutionLog.LogError(ex, "{ProducerName} Status change event failed", Producer.Name);
                             }
                             finally
                             {
                                 _semaphoreStatusChange.ReleaseSafe();
                             }
                         });
        }

        /// <summary>
        /// Processes the <see cref="snapshot_complete"/> message. Only snapshot_complete(s) from user's sessions should be processed
        /// </summary>
        /// <param name="snapshotCompleted">The <see cref="snapshot_complete"/> message received on the system user's session</param>
        /// <param name="interest">The <see cref="MessageInterest"/> associated with the session which received the message</param>
        /// <returns>A <see cref="ProducerRecoveryStatus"/> specifying the new status of the manager or null reference if no change is needed</returns>
        private ProducerRecoveryStatus? ProcessSnapshotCompleteMessage(snapshot_complete snapshotCompleted, MessageInterest interest)
        {
            if (_producer.EventRecoveries.TryRemove(snapshotCompleted.request_id, out var eventId))
            {
                ExecutionLog.LogInformation("Recovery with requestId={SnapshotCompletedRequestId} for producer={ProducerId}, eventId={SportEventId} completed", snapshotCompleted.request_id, Producer.Id, eventId);
                EventRecoveryCompleted?.Invoke(this, new EventRecoveryCompletedEventArgs(snapshotCompleted.request_id, eventId));
                return null;
            }

            //The snapshot message not for us
            if (!_recoveryOperation.IsRunning || !_recoveryOperation.RequestId.HasValue || _recoveryOperation.RequestId.Value != snapshotCompleted.RequestId)
            {
                ExecutionLog.LogInformation("Received unknown SnapshotComplete[requestId={RequestId}] for producer {ProducerId} on session {SessionName}", snapshotCompleted.request_id, Producer.Id, interest.Name);
                return null;
            }

            ExecutionLog.LogDebug("SnapshotComplete[{SnapshotCompletedRequestId}] for producer=[{ProducerId}] on session {MessageInterest} received", snapshotCompleted.request_id, Producer.Id, interest.Name);
            if (!_recoveryOperation.TryComplete(interest, out var recoveryResult))
            {
                //The recovery is not complete, nothing to do.
                ExecutionLog.LogDebug("Recovery with requestId={SnapshotCompletedRequestId} for producer={ProducerId} is not yet completed. Waiting for snapshots from other sessions", snapshotCompleted.request_id, Producer.Id);
                return null;
            }

            // The recovery operation completed. Check the result and act accordingly
            if (recoveryResult.Success)
            {
                // the recovery was interrupted
                if (recoveryResult.InterruptedAt.HasValue)
                {
                    ExecutionLog.LogWarning("Recovery with requestId={SnapshotCompletedRequestId} for producer={ProducerId} completed with interruption at {RecoveryResultInterruptedAt}",
                                            snapshotCompleted.request_id,
                                            Producer.Id,
                                            recoveryResult.InterruptedAt.Value);
                    _producer.SetLastTimestampBeforeDisconnect(recoveryResult.InterruptedAt.Value);
                    return ProducerRecoveryStatus.Error;
                }
                // the recovery was not interrupted
                var recoveryDuration = TimeProviderAccessor.Current.Now - recoveryResult.StartTime;
                ExecutionLog.LogInformation("Recovery with requestId={SnapshotCompletedRequestId} for producer={ProducerId} completed in {RecoveryDurationTotalMilliseconds} ms",
                                            snapshotCompleted.request_id,
                                            Producer.Id,
                                            (int)recoveryDuration.TotalMilliseconds);
                _producer.SetLastTimestampBeforeDisconnect(SdkInfo.FromEpochTime(snapshotCompleted.timestamp));
                return _timestampTracker.IsBehind
                           ? ProducerRecoveryStatus.Delayed
                           : ProducerRecoveryStatus.Completed;
            }

            // The recovery operation timed-out
            var timeOutDuration = TimeProviderAccessor.Current.Now - recoveryResult.StartTime;
            ExecutionLog.LogWarning("Recovery with requestId={SnapshotCompletedRequestId} timed out after:{Duration}", snapshotCompleted.RequestId, timeOutDuration);
            return ProducerRecoveryStatus.Error;
        }

        /// <summary>
        /// Executes the steps required when the connection to the message broker is shutdown.
        /// </summary>
        public void ConnectionShutdown()
        {
            if (!_producer.IsAvailable || _producer.IsDisabled || _producer.IgnoreRecovery)
            {
                return;
            }

            lock (_syncLock)
            {
                _connectionDownTimestamp = DateTime.Now;

                ProducerRecoveryStatus? newStatus = null;

                if (Status == ProducerRecoveryStatus.Completed || Status == ProducerRecoveryStatus.Delayed)
                {
                    ExecutionLog.LogWarning("Connection shutdown detected. Producer={Producer}, Status={RecoveryStatus}, Action: Changing the status to Error", Producer, Enum.GetName(typeof(ProducerRecoveryStatus), Status));
                    newStatus = ProducerRecoveryStatus.Error;
                }
                else if (Status == ProducerRecoveryStatus.Started)
                {
                    ExecutionLog.LogWarning("Connection shutdown detected. Producer={Producer}, Status={RecoveryStatus}, Action: Resetting current recovery operation", Producer, Enum.GetName(typeof(ProducerRecoveryStatus), Status));
                    _recoveryOperation.Reset();
                    newStatus = ProducerRecoveryStatus.Error;
                }
                else
                {
                    ExecutionLog.LogWarning("Connection shutdown detected. Producer={Producer}, Status={RecoveryStatus}, Action: No action required", Producer, Enum.GetName(typeof(ProducerRecoveryStatus), Status));
                }

                if (newStatus != null && newStatus.Value != Status)
                {
                    SetStatusAndRaiseEvent(null, newStatus.Value);
                }
            }
        }

        /// <summary>
        /// Executes the steps required when the connection to the message broker is recovered after shutdown.
        /// </summary>
        public void ConnectionRecovered()
        {
            if (!_producer.IsAvailable || _producer.IsDisabled || _producer.IgnoreRecovery)
            {
                return;
            }

            lock (_syncLock)
            {
                _connectionDownTimestamp = DateTime.MinValue;

                ProducerRecoveryStatus? newStatus = null;

                if (Status == ProducerRecoveryStatus.Started)
                {
                    ExecutionLog.LogWarning("Connection recovery detected after previous recovery request started. Producer={Producer}, Status={RecoveryStatus}, Action: Resetting current recovery operation",
                                            Producer,
                                            Enum.GetName(typeof(ProducerRecoveryStatus), Status));
                    _recoveryOperation.Reset();
                    newStatus = ProducerRecoveryStatus.Error;
                }

                if (newStatus != null && newStatus.Value != Status)
                {
                    SetStatusAndRaiseEvent(null, newStatus.Value);
                }
            }
        }

        /// <summary>
        /// Processes <see cref="IMessage" /> received on the user's session(s)
        /// </summary>
        /// <remarks>
        /// This method does:
        /// - collect timing info on odds_change(s), bet_stop(s) and alive(s)
        /// - attempt to complete running recoveries with snapshot_complete(s)
        /// This method does not:
        /// - determine if the user is behind (or not) with message processing - this is done in CheckStatus(..) method
        /// - attempt to determine whether the recovery has timed-out - this is done in CheckStatus(..) method
        /// - determine weather is alive violated. This should be only done based on system alive(s) and is done in ProcessSystemMessage(...) method
        /// - start recoveries - This should only be done based on system alive(s) and is done in ProcessSystemMessage(...) method
        /// </remarks>
        /// <param name="message">The <see cref="IMessage" /> message to be processed</param>
        /// <param name="interest">The <see cref="MessageInterest"/> describing the session from which the message originates </param>
        /// <exception cref="ArgumentException">The Producer.Id of the message and the Producer associated with this manager do not match</exception>
        public void ProcessUserMessage(FeedMessage message, MessageInterest interest)
        {
            Guard.Argument(message, nameof(message)).NotNull();
            Guard.Argument(interest, nameof(interest)).NotNull();

            if (message.ProducerId != Producer.Id)
            {
                ExecutionLog.LogError("The producer ({MessageProducerId}) of the message and the Producer {ProducerName} ({ProducerId}) associated with this manager do not match", message.ProducerId, Producer.Name, Producer.Id);
                return;
            }

            if (!Producer.IsAvailable || Producer.IsDisabled)
            {
                ExecutionLog.LogDebug("{ProducerName} Producer is not available or user disabled, however we still receive {FeedMessageType} message. Can be ignored", Producer.Name, message.GetType());
                return;
            }

            lock (_syncLock)
            {
                ProducerRecoveryStatus? newStatus = null;

                try
                {
                    if (message is odds_change || message is bet_stop)
                    {
                        _timestampTracker.ProcessUserMessage(interest, message);
                    }

                    var alive = message as alive;
                    if (alive != null && alive.subscribed != 0)
                    {
                        // the alive message from user's session should only be used for tracking of processing delay
                        _timestampTracker.ProcessUserMessage(interest, message);
                        if (Status == ProducerRecoveryStatus.Completed || Status == ProducerRecoveryStatus.Delayed)
                        {
                            _producer.SetLastTimestampBeforeDisconnect(SdkInfo.FromEpochTime(_timestampTracker.OldestUserAliveTimestamp));
                        }
                    }

                    var snapshotComplete = message as snapshot_complete;
                    if (snapshotComplete != null)
                    {
                        newStatus = ProcessSnapshotCompleteMessage(snapshotComplete, interest);
                    }

                    // reset connection down timestamp if messages arrived after new connection was done
                    if (_connectionDownTimestamp > DateTime.MinValue && SdkInfo.FromEpochTime(_timestampTracker.SystemAliveTimestamp) > _connectionDownTimestamp)
                    {
                        ConnectionRecovered();
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "An unexpected exception occurred while processing user message. Producer={ProducerId}, Session={MessageInterest}, Message={Message}", _producer.Id, interest.Name, message);
                }

                if (newStatus != null && newStatus.Value != Status)
                {
                    SetStatusAndRaiseEvent(null, newStatus.Value);
                }
            }
        }

        /// <summary>
        /// Processes a message received on the system's session
        /// </summary>
        /// <remarks>
        /// This method does:
        /// - starts recovery operations if needed
        /// - interrupt running recoveries on non-subscribed alive(s) and alive violation(s)
        /// - set LastTimestampBeforeDisconnect property on the producer.
        /// This method does not:
        /// - determine if the user is behind (or not) with message processing - this is done in CheckStatus(..) method
        /// - attempt to determine whether the recovery has timed-out - this is done in CheckStatus(..) method
        /// </remarks>
        /// <param name="message">A <see cref="FeedMessage"/> received on the system session</param>
        public void ProcessSystemMessage(FeedMessage message)
        {
            Guard.Argument(message, nameof(message)).NotNull();

            var alive = message as alive;

            if (alive?.ProducerId != Producer.Id)
            {
                return;
            }

            if (Status == ProducerRecoveryStatus.FatalError || _producer.IgnoreRecovery)
            {
                return;
            }

            lock (_syncLock)
            {
                ProducerRecoveryStatus? newStatus = null;

                try
                {
                    // store the timestamp of most recent system alive before it is overridden by
                    // _timestampTracker.ProcessSystemAlive(alive); in case the current alive
                    // is not subscribed and ongoing recovery operation must be interrupted
                    var previousAliveTimestamp = _timestampTracker.SystemAliveTimestamp;
                    _timestampTracker.ProcessSystemAlive(alive);

                    // if current status is NotStarted or Error just start the recovery
                    if (Status == ProducerRecoveryStatus.NotStarted || Status == ProducerRecoveryStatus.Error)
                    {
                        try
                        {
                            var started = StartRecovery();
                            if (started.HasValue && started.Value)
                            {
                                ExecutionLog.LogInformation("Producer={ProducerId}: Recovery operation started. Current status={RecoveryStatus}, After={After}",
                                                            _producer.Id,
                                                            Enum.GetName(typeof(ProducerRecoveryStatus), Status),
                                                            SdkInfo.ToEpochTime(_producer.LastTimestampBeforeDisconnect));
                                newStatus = ProducerRecoveryStatus.Started;
                            }
                            else if (started.HasValue)
                            {
                                ExecutionLog.LogWarning("Producer={ProducerId}: An error occurred while starting recovery operation with after={After}. Retry will be made at next system alive",
                                                        _producer.Id,
                                                        SdkInfo.ToEpochTime(_producer.LastTimestampBeforeDisconnect));
                            }
                        }
                        catch (RecoveryInitiationException ex)
                        {
                            ExecutionLog.LogCritical("Producer id={ProducerId} failed to execute recovery because \'after\' is to much in the past. After={After}. Stopping the feed", Producer.Id, ex.After);
                            newStatus = ProducerRecoveryStatus.FatalError;
                        }
                    }
                    else
                    {
                        // we are no longer in sync with the feed
                        if (alive.subscribed == 0)
                        {
                            if (_recoveryOperation.IsRunning)
                            {
                                var timestamp = SdkInfo.FromEpochTime(previousAliveTimestamp);
                                ExecutionLog.LogInformation("Producer={ProducerId}: Recovery operation interrupted. Current status={RecoveryStatus}, Timestamp={LastTimestamp}",
                                                            _producer.Id,
                                                            Enum.GetName(typeof(ProducerRecoveryStatus), Status),
                                                            timestamp);
                                _recoveryOperation.Interrupt(timestamp);
                            }
                            else
                            {
                                try
                                {
                                    var started = StartRecovery();
                                    if (started.HasValue && started.Value)
                                    {
                                        ExecutionLog.LogInformation("Producer={ProducerId}: Recovery operation started due to unsubscribed alive. Current status={RecoveryStatus}, After={After}",
                                                                    _producer.Id,
                                                                    Enum.GetName(typeof(ProducerRecoveryStatus), Status),
                                                                    SdkInfo.ToEpochTime(_producer.LastTimestampBeforeDisconnect));
                                        newStatus = ProducerRecoveryStatus.Started;
                                    }
                                    else if (started.HasValue)
                                    {
                                        ExecutionLog.LogWarning("Producer={ProducerId}: An error occurred while starting recovery operation with after={After}. Retry will be made at next system alive",
                                                                _producer.Id,
                                                                SdkInfo.ToEpochTime(_producer.LastTimestampBeforeDisconnect));
                                        newStatus = ProducerRecoveryStatus.Error;
                                    }
                                }
                                catch (RecoveryInitiationException ex)
                                {
                                    ExecutionLog.LogCritical("Producer id={ProducerId} failed to execute recovery because after is to much in the past. After={After}. Stopping the feed", Producer.Id, ex.After);
                                    newStatus = ProducerRecoveryStatus.FatalError;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "An unexpected exception occurred while processing system message. Producer={ProducerId}, message={FeedMessage}", _producer.Id, message);
                }

                if (newStatus != null && newStatus.Value != Status)
                {
                    SetStatusAndRaiseEvent(null, newStatus.Value);
                }
            }
        }

        /// <summary>
        /// Checks the status of the current recovery manager
        /// </summary>
        /// <remarks>
        /// The method must:
        /// - Check whether current recovery is running and has expired
        /// - Whether there is an alive violation
        /// - Whether the user is behind with processing
        /// The method should not:
        /// - Update the processing delay - this is done on a message from a user's session
        /// - Start the recovery - this is done on alive from the system session
        /// - Complete non timed-out recovery - this is done on the snapshot_complete from user's session
        /// </remarks>
        public void CheckStatus()
        {
            //if the producer is disabled in SDK or not available for current user - nothing to do
            if (!Producer.IsAvailable || Producer.IsDisabled)
            {
                return;
            }

            // recovery must not be done (replay server)
            if (_producer.IgnoreRecovery)
            {
                return;
            }

            // multiple class fields can be accessed from multiple threads (messages from user session(s), system session, here, ...)
            lock (_syncLock)
            {
                ProducerRecoveryStatus? newStatus = null;

                try
                {
                    // check whether the user is falling behind with processing
                    if (Status == ProducerRecoveryStatus.Completed && _timestampTracker.IsBehind)
                    {
                        newStatus = ProducerRecoveryStatus.Delayed;
                    }

                    // check whether the user was behind with processing but is no longer
                    if (Status == ProducerRecoveryStatus.Delayed && !_timestampTracker.IsBehind)
                    {
                        newStatus = ProducerRecoveryStatus.Completed;
                    }

                    // Check whether there is an alive violation during normal processing
                    if ((Status == ProducerRecoveryStatus.Completed || Status == ProducerRecoveryStatus.Delayed) && _timestampTracker.IsAliveViolated)
                    {
                        ExecutionLog.LogWarning("Producer id={ProducerId}: alive violation detected. Recovery will be done on next system alive", Producer.Id);
                        newStatus = ProducerRecoveryStatus.Error;
                        //TODO: do we need new recovery here - or just Delayed status
                    }

                    // Check whether there is an alive violation during recovery
                    if (Status == ProducerRecoveryStatus.Started && _timestampTracker.IsAliveViolated)
                    {
                        ExecutionLog.LogWarning("Producer id={ProducerId}: alive violation detected during recovery. Additional recovery from {TrackerTimestamp} will be done once the current is completed",
                                                Producer.Id,
                                                _timestampTracker.SystemAliveTimestamp);
                        _recoveryOperation.Interrupt(SdkInfo.FromEpochTime(_timestampTracker.SystemAliveTimestamp));
                    }

                    if ((Status == ProducerRecoveryStatus.Started && !_recoveryOperation.IsRunning)
                        || (Status != ProducerRecoveryStatus.Started && _recoveryOperation.IsRunning))
                    {
                        ExecutionLog.LogWarning("Producer id={ProducerId}: internal recovery status problem ({RecoveryStatus}-{RecoveryOperationIsRunning}). Recovery will be done on next system alive",
                                                Producer.Id,
                                                Status,
                                                _recoveryOperation.IsRunning);
                        newStatus = ProducerRecoveryStatus.Error;
                    }

                    // Check whether the recovery is running and has timed-out
                    if (Status == ProducerRecoveryStatus.Started && _recoveryOperation.HasTimedOut())
                    {
                        _recoveryOperation.CompleteTimedOut();
                        ExecutionLog.LogWarning("Producer id={ProducerId}: recovery timeout. New recovery from {TrackerTimestamp} will be done", Producer.Id, _timestampTracker.SystemAliveTimestamp);
                        newStatus = ProducerRecoveryStatus.Error;
                    }

                    // check if any message arrived for this producer in the last X seconds; if not, start recovery
                    if ((Status == ProducerRecoveryStatus.NotStarted || Status == ProducerRecoveryStatus.Error)
                        && newStatus != ProducerRecoveryStatus.Started
                        && DateTime.Now - SdkInfo.FromEpochTime(_timestampTracker.SystemAliveTimestamp) > TimeSpan.FromSeconds(60)
                        && SdkInfo.FromEpochTime(_timestampTracker.SystemAliveTimestamp) > _connectionDownTimestamp)
                    {
                        ExecutionLog.LogWarning("Producer id={ProducerId}: no alive messages arrived since {TrackerTimestamp}. New recovery will be done", Producer.Id, SdkInfo.FromEpochTime(_timestampTracker.SystemAliveTimestamp));
                        if (_recoveryOperation.IsRunning)
                        {
                            var timestamp = SdkInfo.FromEpochTime(_timestampTracker.OldestUserAliveTimestamp);
                            ExecutionLog.LogWarning("Producer={ProducerId}: Recovery operation interrupted. Current status={RecoveryStatus}, Timestamp={Timestamp}", _producer.Id, Enum.GetName(typeof(ProducerRecoveryStatus), Status), timestamp);
                            _recoveryOperation.Interrupt(timestamp);
                            _recoveryOperation.Reset();
                        }
                        var recoveryStarted = StartRecovery();
                        if (recoveryStarted.HasValue && recoveryStarted.Value)
                        {
                            newStatus = ProducerRecoveryStatus.Started;
                        }
                    }

                    // recovery is called and we check if any recovery message arrived in last X time; or restart recovery
                    if (Status == ProducerRecoveryStatus.Started && _recoveryOperation.IsRunning && DateTime.Now - _lastRecoveryMessage > TimeSpan.FromSeconds(300))
                    {
                        ExecutionLog.LogWarning("Producer id={ProducerId}: no recovery message arrived since {LastRecoveryMessage}. New recovery will be done", Producer.Id, _lastRecoveryMessage);
                        if (_recoveryOperation.IsRunning)
                        {
                            var timestamp = SdkInfo.FromEpochTime(_timestampTracker.OldestUserAliveTimestamp);
                            ExecutionLog.LogWarning("Producer={ProducerId}: Recovery operation interrupted. Current status={RecoveryStatus}, Timestamp={Timestamp}", _producer.Id, Enum.GetName(typeof(ProducerRecoveryStatus), Status), timestamp);
                            _recoveryOperation.Interrupt(timestamp);
                            _recoveryOperation.Reset();
                        }
                        var recoveryStarted = StartRecovery();
                        if (recoveryStarted.HasValue && recoveryStarted.Value)
                        {
                            newStatus = ProducerRecoveryStatus.Started;
                        }
                    }

                    ExecutionLog.LogInformation("Status check: Producer={ProducerId} ({RecoveryStatus}), Timing Info={TrackerTimestamp}", _producer.Id, Enum.GetName(typeof(ProducerRecoveryStatus), Status), _timestampTracker);
                }
                catch (Exception ex)
                {
                    ExecutionLog.LogError(ex, "An unexpected exception occurred while checking status. Producer={ProducerId}. Status={RecoveryStatus}, IsRunning={RecoveryOperationIsRunning}", _producer.Id, Status, _recoveryOperation.IsRunning);
                }

                if (newStatus != null && newStatus.Value != Status)
                {
                    SetStatusAndRaiseEvent(null, newStatus.Value);
                }
            }
        }

        private bool? StartRecovery()
        {
            if (!_connectionDownTimestamp.Equals(DateTime.MinValue))
            {
                ExecutionLog.LogWarning("Producer={ProducerId}: Recovery operation skipped (feed connection is down)", _producer.Id);
                return null;
            }

            var duration = TimeProviderAccessor.Current.Now - _recoveryOperation.LastAttemptTime;

            if (duration.TotalSeconds > _minIntervalBetweenRecoveryRequests)
            {
                _lastRecoveryMessage = DateTime.Now;
                return _recoveryOperation.Start();
            }

            ExecutionLog.LogInformation("Producer={ProducerId}: Recovery operation skipped. Last done at {RecoveryLastAttemptTime}", _producer.Id, _recoveryOperation.LastAttemptTime);
            return null;
        }
    }
}
