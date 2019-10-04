/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.CodeAnalysis;
using System.Diagnostics.Contracts;
using System.Linq;
using Common.Logging;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Entities;
using Sportradar.OddsFeed.SDK.Messages.Internal;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Class used to track the progress of a single recovery operation
    /// </summary>
    /// <remarks>
    /// The class is not thread safe
    /// </remarks>
    public class RecoveryOperation : IRecoveryOperation
    {
        /// <summary>
        /// A <see cref="ILog"/> used for execution logging
        /// </summary>
        private static readonly ILog ExecutionLog = SdkLoggerFactory.GetLoggerForExecution(typeof(RecoveryOperation));

        /// <summary>
        /// The producer whose recovery is being tracked by current instance
        /// </summary>
        private readonly Producer _producer;

        /// <summary>
        /// A <see cref="IRecoveryRequestIssuer"/> used to start recovery requests
        /// </summary>
        private readonly IRecoveryRequestIssuer _recoveryRequestIssuer;

        /// <summary>
        /// The node id of the current feed instance or a null reference
        /// </summary>
        private readonly int _nodeId;

        /// <summary>
        /// A <see cref="MessageInterest"/> containing interests on all sessions
        /// </summary>
        private readonly List<MessageInterest> _allInterests;

        /// <summary>
        /// A <see cref="MessageInterest"/> of all sessions on which the snapshot was received
        /// </summary>
        private readonly List<MessageInterest> _snapshotReceivedSessions = new List<MessageInterest>();

        /// <summary>
        /// The current request id
        /// </summary>
        private long _requestId;

        /// <summary>
        /// The <see cref="DateTime"/> specifying when the current operation started
        /// </summary>
        private DateTime _startTime;

        /// <summary>
        /// The <see cref="DateTime"/> specifying the first time the recovery was interrupted
        /// </summary>
        private DateTime? _interruptionTime;

        /// <summary>
        /// Gets a value indication whether the recovery operation is currently running.
        /// </summary>
        public bool IsRunning { get; private set; }

        /// <summary>
        /// Gets a request Id of the current recovery operation or a null reference if recovery is not running
        /// </summary>
        public long? RequestId => IsRunning ? (long?)_requestId : null;

        /// <summary>
        /// Gets a value indicating whether [adjusted after age]
        /// </summary>
        /// <value><c>true</c> if [adjusted after age]; otherwise, <c>false</c></value>
        private readonly bool _adjustedAfterAge;

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryOperation"/> class
        /// </summary>
        /// <param name="producer">The producer whose recovery is being tracked by current instance</param>
        /// <param name="recoveryRequestIssuer">A <see cref="IRecoveryRequestIssuer"/> used to start recovery requests</param>
        /// <param name="allInterests">A <see cref="MessageInterest"/> containing interests on all sessions</param>
        /// <param name="nodeId">The node id of the current feed instance</param>
        /// <param name="adjustAfterAge">The value indicating whether the after age should be enforced before executing recovery request</param>
        [SuppressMessage("ReSharper", "PossibleMultipleEnumeration")]
        public RecoveryOperation(Producer producer, IRecoveryRequestIssuer recoveryRequestIssuer, IEnumerable<MessageInterest> allInterests, int nodeId, bool adjustAfterAge)
        {
            Contract.Requires(producer != null);
            Contract.Requires(recoveryRequestIssuer != null);
            Contract.Requires(allInterests != null && allInterests.Any());

            _producer = producer;
            _recoveryRequestIssuer = recoveryRequestIssuer;
            _allInterests = allInterests as List<MessageInterest> ?? new List<MessageInterest>(allInterests);
            _nodeId = nodeId;
            _adjustedAfterAge = adjustAfterAge;
        }

        /// <summary>
        /// Defined field invariants needed by code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_producer != null);
            Contract.Invariant(_allInterests != null);
            Contract.Invariant(_snapshotReceivedSessions != null);
        }

        /// <summary>
        /// Determines whether the snapshot was received on all required sessions
        /// </summary>
        /// <param name="requiredInterests"><see cref="MessageInterest"/>s on which the snapshot is expected</param>
        /// <returns>True if the snapshot was received on all required sessions; False otherwise</returns>
        private bool WereSnapshotsReceivedOnSessions(params MessageInterest[] requiredInterests)
        {
            return requiredInterests.Length <= _snapshotReceivedSessions.Count && requiredInterests.All(_snapshotReceivedSessions.Contains);
        }

        /// <summary>
        /// Determines whether the snapshot was received on all required sessions
        /// </summary>
        /// <returns>True if the snapshot was received on all required sessions; False otherwise</returns>
        private bool IsRecoveryDone()
        {
            bool done;

            // if there is only one session, only snapshot from that session is needed :)
            if (_allInterests.Count == 1)
            {
                done = WereSnapshotsReceivedOnSessions(_allInterests[0]);
            }
            // if there are hi & low priority sessions, the snapshot from high priority session is needed
            else if (_allInterests.Count == 2
                     && _allInterests.Contains(MessageInterest.LowPriorityMessages)
                     && _allInterests.Contains(MessageInterest.HighPriorityMessages))
            {
                done = WereSnapshotsReceivedOnSessions(MessageInterest.HighPriorityMessages);
            }
            // if all interests are a combination of different message scopes, use the producer
            // scopes to determine whether all snapshots were received
            else if (_allInterests.Count <= MessageInterest.MessageScopes.Length 
                     && _allInterests.All(MessageInterest.MessageScopes.Contains))
            {
                done = _producer.Scope
                    .Select(MessageInterest.FromScope)
                    .All(interest => _snapshotReceivedSessions.Contains(interest));
            }
            else
            {
                throw new InvalidOperationException("The combination of all interests is not supported");
            }

            if (done)
            {
                _snapshotReceivedSessions.Clear();
            }
            return done;
        }


        /// <summary>
        /// Attempts to start a recovery operation
        /// </summary>
        /// <returns>True if the operation was successfully started; False otherwise</returns>
        /// <exception cref="InvalidOperationException">The recovery operation is already running</exception>
        /// <exception cref="RecoveryInitiationException">The after parameter is to far in the past</exception>
        public bool Start()
        {
            if (IsRunning)
            {
                throw new InvalidOperationException("The recovery operation is already running");
            }

            var after = _producer.LastTimestampBeforeDisconnect;
            try
            {
                if (after == DateTime.MinValue)
                {
                    _requestId = _recoveryRequestIssuer.RequestFullOddsRecoveryAsync(_producer, _nodeId).Result;
                }
                else
                {
                    if (TimeProviderAccessor.Current.Now > after + _producer.MaxAfterAge())
                    {
                        if (!_adjustedAfterAge)
                        {
                            ExecutionLog.Info($"{_producer.Name}: After time {after} is adjusted.");
                            after = TimeProviderAccessor.Current.Now - _producer.MaxAfterAge() + TimeSpan.FromMinutes(1);
                        }
                        else
                        {
                            throw new RecoveryInitiationException("The after parameter is to far in the past", after);
                        }
                    }

                    _requestId = _recoveryRequestIssuer.RequestRecoveryAfterTimestampAsync(_producer, after, _nodeId).Result;
                }
            }
            catch (Exception ex)
            {
                var actualException = ex.InnerException ?? ex;
                ExecutionLog.Error($"{_producer.Name} There was an error requesting recovery. Exception: {actualException.Message}");
                if (ex is RecoveryInitiationException)
                {
                    throw;
                }
                return false;
            }

            IsRunning = true;
            _startTime = TimeProviderAccessor.Current.Now;
            _interruptionTime = null;
            return true;
        }

        /// <summary>
        /// Stores the time when the operation was interrupted if this is the fist interruption.
        /// Otherwise it does nothing
        /// </summary>
        /// <param name="interruptionTime">A <see cref="DateTime"/> specifying to when to set the interruption time</param>
        /// <exception cref="InvalidOperationException">The recovery operation is not running</exception>
        public void Interrupt(DateTime interruptionTime)
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("The recovery is not running");
            }
            if (_interruptionTime.HasValue)
            {
                return;
            }
            _interruptionTime = interruptionTime;
        }

        /// <summary>
        /// Determines whether the current operation has timed-out
        /// </summary>
        /// <returns>True if the operation timed-out; Otherwise false</returns>
        /// <exception cref="InvalidOperationException">The recovery operation is not running</exception>
        public bool HasTimedOut()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("The recovery operation is not running");
            }
            return (TimeProviderAccessor.Current.Now - _startTime).TotalSeconds > _producer.MaxRecoveryTime;
        }

        /// <summary>
        /// Stops the the recovery operation if all snapshots were received
        /// </summary>
        /// <param name="interest">The <see cref="MessageInterest"/> of the session which received the snapshot message</param>
        /// <param name="result">If the operation was successfully completed, it contains the results of the completed recovery</param>
        /// <returns>True if the recovery operation could be completed; False otherwise</returns>
        /// <exception cref="InvalidOperationException">The recovery operation is not running</exception>
        public bool TryComplete(MessageInterest interest, out RecoveryResult result)
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("The recovery operation is not running");
            }

            result = null;
            _snapshotReceivedSessions.Add(interest);
            if (IsRecoveryDone())
            {
                result = RecoveryResult.ForSuccess(_requestId, _startTime, _interruptionTime);
                IsRunning = false;
                return true;
            }

            if (HasTimedOut())
            {
                result = RecoveryResult.ForTimeOut(_requestId, _startTime);
                IsRunning = false;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Completes the timed-out recovery operation
        /// </summary>
        /// <returns>A <see cref="RecoveryResult"/> containing recovery info</returns>
        /// <exception cref="InvalidOperationException">The recovery operation is not running or it has not timed-out</exception>
        public RecoveryResult CompleteTimedOut()
        {
            if (!IsRunning)
            {
                throw new InvalidOperationException("The recovery is not running");
            }
            if (!HasTimedOut())
            {
                throw new InvalidOperationException("The recovery operation is not timed-out");
            }

            IsRunning = false;
            return RecoveryResult.ForTimeOut(_requestId, _startTime);
        }

        /// <summary>
        /// Resets the operation to it's default (not started) state. If operation is already not started, it does nothing.
        /// </summary>
        public void Reset()
        {
            IsRunning = false;
        }
    }
}
