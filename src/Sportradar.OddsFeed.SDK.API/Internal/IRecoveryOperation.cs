/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Sportradar.OddsFeed.SDK.Entities;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// A contract implemented by types used to perform recovery operation
    /// </summary>
    public interface IRecoveryOperation
    {
        /// <summary>
        /// Gets a value indication whether the recovery operation is currently running.
        /// </summary>
        bool IsRunning { get; }

        /// <summary>
        /// Gets a request Id of the current recovery operation or a null reference if recovery is not running
        /// </summary>
        long? RequestId { get; }

        /// <summary>
        /// Attempts to start a recovery operation
        /// </summary>
        /// <returns>True if the operation was successfully started; False otherwise</returns>
        /// <exception cref="InvalidOperationException">The recovery operation is already running</exception>
        /// <exception cref="RecoveryInitiationException">The after parameter is to far in the past</exception>
        bool Start();

        /// <summary>
        /// Stores the time when the operation was interrupted if this is the fist interruption.
        /// Otherwise it does nothing
        /// </summary>
        /// <param name="interruptionTime">A <see cref="DateTime"/> specifying to when to set the interruption time</param>
        /// <exception cref="InvalidOperationException">The recovery operation is not running</exception>
        void Interrupt(DateTime interruptionTime);

        /// <summary>
        /// Determines whether the current operation has timed-out
        /// </summary>
        /// <returns>True if the operation timed-out; Otherwise false.</returns>
        /// <exception cref="InvalidOperationException">The recovery operation is not running</exception>
        bool HasTimedOut();

        /// <summary>
        /// Stops the the recovery operation if all snapshots were received
        /// </summary>
        /// <param name="interest">The <see cref="MessageInterest"/> of the session which received the snapshot message</param>
        /// <param name="result">If the operation was successfully completed, it contains the results of the completed recovery</param>
        /// <returns>True if the recovery operation could be completed; False otherwise</returns>
        /// <exception cref="InvalidOperationException">The recovery operation is not running</exception>
        bool TryComplete(MessageInterest interest, out RecoveryResult result);

        /// <summary>
        /// Resets the operation to it's default (not started) state. If operation is already not started, it does nothing.
        /// </summary>
        void Reset();

        /// <summary>
        /// Completes the timed-out recovery operation
        /// </summary>
        /// <returns>A <see cref="RecoveryResult"/> containing recovery info</returns>
        /// <exception cref="InvalidOperationException">The recovery operation is not running or it has not timed-out</exception>
        RecoveryResult CompleteTimedOut();
    }
}