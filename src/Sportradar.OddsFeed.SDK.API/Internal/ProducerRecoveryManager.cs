/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Threading.Tasks;
using Common.Logging;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages;
using Sportradar.OddsFeed.SDK.Messages.Feed;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    [SuppressMessage("ReSharper", "InconsistentlySynchronizedField")]
    internal class ProducerRecoveryManager : IProducerRecoveryManager
    {
        /// <summary>
        /// A <see cref="ILog"/> used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(ProducerRecoveryManager));

        /// <summary>
        /// The producer
        /// </summary>
        private readonly Producer _producer;

        /// <summary>
        /// A <see cref="IRecoveryOperation"/> used to issue & track recovery operations
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
        /// Initializes a new instance of the <see cref="ProducerRecoveryManager"/> class
        /// </summary>
        /// <param name="producer">A <see cref="IProducer"/> for which status is tracked</param>
        /// <param name="recoveryOperation">A <see cref="IRecoveryOperation"/> used to issue and track recovery operations</param>
        /// <param name="timestampTracker">A <see cref="ITimestampTracker"/> used to track message timings</param>
        internal ProducerRecoveryManager(IProducer producer, IRecoveryOperation recoveryOperation, ITimestampTracker timestampTracker)
        {
            Contract.Requires(producer != null);
            Contract.Requires(recoveryOperation != null);
            Contract.Requires(timestampTracker != null);

            Producer = producer;
            _producer = (Producer)producer;
            _recoveryOperation = recoveryOperation;
            _timestampTracker = timestampTracker;

            Status = ProducerRecoveryStatus.NotStarted;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(Producer != null);
            Contract.Invariant(_producer != null);
            Contract.Invariant(_recoveryOperation != null);
            Contract.Invariant(_timestampTracker != null);
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
                ExecutionLog.Debug($"{Producer.Name}: Attempting to set status to an existing value {Status}. Aborting ...");
                return;
            }
            ExecutionLog.Info($"{Producer.Name} Status changed from {Status} to {newStatus}.");

            Status = newStatus;
            _producer.SetProducerDown(newStatus != ProducerRecoveryStatus.Completed);

            Task.Run(() =>
            {
                StatusChanged?.Invoke(this, new TrackerStatusChangeEventArgs(requestId, oldStatus, newStatus));
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
            URN eventId;
            if (_producer.EventRecoveries.TryRemove(snapshotCompleted.request_id, out eventId))
            {
                ExecutionLog.Info($"Recovery with requestId={snapshotCompleted.request_id} for producer={Producer.Id}, eventId={eventId} completed.");
                EventRecoveryCompleted?.Invoke(this, new EventRecoveryCompletedEventArgs(snapshotCompleted.request_id, eventId));
                return null;
            }

            //The snapshot message not for us
            if (!_recoveryOperation.IsRunning || !_recoveryOperation.RequestId.HasValue || _recoveryOperation.RequestId.Value != snapshotCompleted.RequestId)
            {
                return null;
            }

            //Debug.Assert(Status == ProducerRecoveryStatus.Started);

            RecoveryResult recoveryResult;
            ExecutionLog.Debug($"SnapshotComplete[{"requestId" + snapshotCompleted.request_id}] for producer=[{"id=" + Producer.Id}] on session {interest.Name} received");
            if (!_recoveryOperation.TryComplete(interest, out recoveryResult))
            {
                //The recovery is not complete, nothing to do.
                ExecutionLog.Debug($"Recovery with requestId={snapshotCompleted.request_id} for producer={Producer.Id} is not yet completed. Waiting for snapshots from other sessions");
                return null;
            }

            // The recovery operation completed. Check the result and act accordingly
            if (recoveryResult.Success)
            {
                // the recovery was interrupted
                if (recoveryResult.InterruptedAt.HasValue)
                {
                    ExecutionLog.Warn($"Recovery with requestId={snapshotCompleted.request_id} for producer={Producer.Id} completed with interruption at:{recoveryResult.InterruptedAt.Value}");
                    _producer.SetLastTimestampBeforeDisconnect(recoveryResult.InterruptedAt.Value);
                    return ProducerRecoveryStatus.Error;
                }
                // the recovery was not interrupted
                var recoveryDuration = TimeProviderAccessor.Current.Now - recoveryResult.StartTime;
                ExecutionLog.Info($"Recovery with requestId={snapshotCompleted.request_id} for producer={Producer.Id} completed in {recoveryDuration.TotalSeconds} sec.");
                _producer.SetLastTimestampBeforeDisconnect(SdkInfo.FromEpochTime(snapshotCompleted.timestamp));
                return _timestampTracker.IsBehind
                           ? ProducerRecoveryStatus.Delayed
                           : ProducerRecoveryStatus.Completed;
            }

            // The recovery operation timed-out
            var timeOutDuration = TimeProviderAccessor.Current.Now - recoveryResult.StartTime;
            ExecutionLog.Warn($"Recovery with requestId={snapshotCompleted.RequestId} timed out after:{timeOutDuration}");
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

            var newStatus = Status;
            lock (_syncLock)
            {
                if (Status == ProducerRecoveryStatus.Completed || Status == ProducerRecoveryStatus.Delayed)
                {
                    ExecutionLog.Warn($"Connection shutdown detected. Producer={Producer}, Status={Enum.GetName(typeof(ProducerRecoveryStatus), Status)}, Action: Changing the status to Error.");
                    newStatus = ProducerRecoveryStatus.Error;
                }
                else if (Status == ProducerRecoveryStatus.Started)
                {
                    ExecutionLog.Warn($"Connection shutdown detected. Producer={Producer}, Status={Enum.GetName(typeof(ProducerRecoveryStatus), Status)}, Action: Reseting current recovery operation.");
                    _recoveryOperation.Reset();
                    newStatus = ProducerRecoveryStatus.Error;
                }
                else
                {
                    ExecutionLog.Warn($"Connection shutdown detected. Producer={Producer}, Status={Enum.GetName(typeof(ProducerRecoveryStatus), Status)}, Action: No action required.");
                }
                if (newStatus != Status)
                {
                    SetStatusAndRaiseEvent(null, newStatus);
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
        /// <exception cref="System.ArgumentException">The Producer.Id of the message and the Producer associated with this manager do not match</exception>
        public void ProcessUserMessage(FeedMessage message, MessageInterest interest)
        {
            if (message.ProducerId != Producer.Id)
            {
                throw new ArgumentException("The producer.Id of the message and the Producer associated with this manager do not match", nameof(message));
            }

            if (!Producer.IsAvailable || Producer.IsDisabled)
            {
                ExecutionLog.Debug($"{Producer.Name} Producer is not available or user disabled, however we still receive {message.GetType()} message. Can be ignored.");
                return;
            }

            ProducerRecoveryStatus? newStatus = null;
            lock (_syncLock)
            {
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
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"An unexpected exception occurred while processing user message. Producer={_producer.Id}, Session={interest.Name}, Message={message}, Exception:", ex);
                }
                if (newStatus.HasValue && Status != newStatus)
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
            Contract.Requires(message != null);

            var alive = message as alive;

            if (alive?.ProducerId != Producer.Id)
            {
                return;
            }

            var newStatus = Status;

            if (Status == ProducerRecoveryStatus.FatalError || _producer.IgnoreRecovery)
            {
                return;
            }

            lock (_syncLock)
            {
                try
                {
                    // store the timestamp of most recent system alive before it is overridden by
                    // _timestampTracker.ProcessSystemAlive(alive); in case the current alive
                    // is not subscribed and ongoing recovery operation must be interrupted
                    var previousAliveTimestamp = _timestampTracker.SystemAliveTimestamp;
                    _timestampTracker.ProcessSystemAlive(alive);
                    _producer.SetTimeOfLastAlive(TimeProviderAccessor.Current.Now);

                    // if current status is NotStarted or Error just start the recovery
                    if (Status == ProducerRecoveryStatus.NotStarted || Status == ProducerRecoveryStatus.Error)
                    {
                        try
                        {
                            if (_recoveryOperation.Start())
                            {
                                ExecutionLog.Info($"Producer={_producer.Id}: Recovery operation started. Current status={Enum.GetName(typeof(ProducerRecoveryStatus), Status)}, After={SdkInfo.ToEpochTime(_producer.LastTimestampBeforeDisconnect)}.");
                                newStatus = ProducerRecoveryStatus.Started;
                            }
                            else
                            {
                                ExecutionLog.Warn($"Producer={_producer.Id}: An error occurred while starting recovery operation with after={SdkInfo.ToEpochTime(_producer.LastTimestampBeforeDisconnect)}. Retry will be made at next system alive.");
                            }
                        }
                        catch (RecoveryInitiationException ex)
                        {
                            ExecutionLog.Fatal($"Producer id={Producer.Id} failed to execute recovery because 'after' is to much in the past. After={ex.After}. Stopping the feed.");
                            newStatus = ProducerRecoveryStatus.FatalError;
                        }
                    }
                    else
                    {
                        Debug.Assert(Status == ProducerRecoveryStatus.Started
                                     || Status == ProducerRecoveryStatus.Completed
                                     || Status == ProducerRecoveryStatus.Delayed);

                        // we are no longer in sync with the feed
                        if (alive.subscribed == 0)
                        {
                            if (_recoveryOperation.IsRunning)
                            {
                                Debug.Assert(Status == ProducerRecoveryStatus.Started);
                                var timestamp = SdkInfo.FromEpochTime(previousAliveTimestamp);
                                ExecutionLog.Info($"Producer={_producer.Id}: Recovery operation interrupted. Current status={Enum.GetName(typeof(ProducerRecoveryStatus), Status)}, Timestamp={timestamp}.");
                                _recoveryOperation.Interrupt(timestamp);
                            }
                            else
                            {
                                Debug.Assert(Status == ProducerRecoveryStatus.Completed || Status == ProducerRecoveryStatus.Delayed);
                                try
                                {
                                    if (_recoveryOperation.Start())
                                    {
                                        ExecutionLog.Info($"Producer={_producer.Id}: Recovery operation started due to un-subscribed alive. Current status={Enum.GetName(typeof(ProducerRecoveryStatus), Status)}, After={SdkInfo.ToEpochTime(_producer.LastTimestampBeforeDisconnect)}.");
                                        newStatus = ProducerRecoveryStatus.Started;
                                    }
                                    else
                                    {
                                        ExecutionLog.Warn($"Producer={_producer.Id}: An error occurred while starting recovery operation with after={SdkInfo.ToEpochTime(_producer.LastTimestampBeforeDisconnect)}. Retry will be made at next system alive.");
                                        newStatus = ProducerRecoveryStatus.Error;
                                    }
                                }
                                catch (RecoveryInitiationException ex)
                                {
                                    ExecutionLog.Fatal($"Producer id={Producer.Id} failed to execute recovery because 'after' is to much in the past. After={ex.After}. Stopping the feed.");
                                    newStatus = ProducerRecoveryStatus.FatalError;
                                }
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"An unexpected exception occurred while processing system message. Producer={_producer.Id}, message={message}. Exception:", ex);
                }
                if (newStatus != Status)
                {
                    SetStatusAndRaiseEvent(null, newStatus);
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

            // multiple class fields can be accessed from multiple threads(messages from user session(s), system session, here, ...)
            lock (_syncLock)
            {
                var newStatus = Status;

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
                        ExecutionLog.Warn($"Producer id={Producer.Id}: alive violation detected. Recovery will be done on next system alive.");
                        newStatus = ProducerRecoveryStatus.Error;
                    }

                    // Check whether there is an alive violation during recovery
                    if (Status == ProducerRecoveryStatus.Started && _timestampTracker.IsAliveViolated)
                    {
                        Debug.Assert(_recoveryOperation.IsRunning);
                        ExecutionLog.Warn($"Producer id={Producer.Id}: alive violation detected during recovery. Additional recovery from {_timestampTracker.SystemAliveTimestamp} will be done once the current is completed.");
                        _recoveryOperation.Interrupt(SdkInfo.FromEpochTime(_timestampTracker.SystemAliveTimestamp));
                    }

                    // Check whether the recovery is running and has timed-out
                    if (Status == ProducerRecoveryStatus.Started && _recoveryOperation.HasTimedOut())
                    {
                        Debug.Assert(_recoveryOperation.IsRunning);
                        _recoveryOperation.CompleteTimedOut();
                        ExecutionLog.Warn($"Producer id={Producer.Id}: recovery timeout. New recovery from {_timestampTracker.SystemAliveTimestamp} will be done.");
                        newStatus = ProducerRecoveryStatus.Error;
                    }
                    ExecutionLog.Info($"Status check: Producer={_producer}({Enum.GetName(typeof(ProducerRecoveryStatus), Status)}), Timing Info={_timestampTracker}");
                }
                catch (Exception ex)
                {
                    ExecutionLog.Error($"An unexpected exception occurred while checking status. Producer={_producer.Id}. Status={Status}, IsRunning={_recoveryOperation.IsRunning}", ex);
                }
                if (newStatus != Status)
                {
                    SetStatusAndRaiseEvent(null, newStatus);
                }
            }
        }
    }
}
