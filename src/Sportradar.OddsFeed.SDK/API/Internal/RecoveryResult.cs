/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Contains information about a single recovery operation
    /// </summary>
    public class RecoveryResult
    {
        /// <summary>
        /// Gets a value indicating whether the recovery successfully completed. If the recovery was interrupted it is still considered successful
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the requestId of the associated recovery operation
        /// </summary>
        public long RequestId { get; }

        /// <summary>
        /// Gets the <see cref="DateTime"/> specifying when the recovery started
        /// </summary>
        public DateTime StartTime { get; }

        /// <summary>
        /// Gets a <see cref="DateTime"/> of the first interruption, or a null reference if recovery was not interrupted
        /// </summary>
        public DateTime? InterruptedAt { get; }

        /// <summary>
        /// Gets a value indicating whether the operation has timed-out
        /// </summary>
        public bool TimedOut { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryResult"/> instance
        /// </summary>
        /// <param name="success">a <see cref="DateTime"/> specifying when the recovery started</param>
        /// <param name="requestId">a requestId of the associated recovery operation</param>
        /// <param name="startTime">a <see cref="DateTime"/> specifying when the recovery started</param>
        /// <param name="interruptedAt">a <see cref="DateTime"/> of the first interruption</param>
        /// <param name="timedOut">a value indicating whether the operation has timed-out</param>
        protected RecoveryResult(bool success, long requestId, DateTime startTime, DateTime? interruptedAt, bool timedOut)
        {
            Guard.Argument(requestId, nameof(requestId)).Positive();

            Success = success;
            RequestId = requestId;
            StartTime = startTime;
            InterruptedAt = interruptedAt;
            TimedOut = timedOut;
        }

        /// <summary>
        /// Constructs and returns a <see cref="RecoveryResult"/> indicating successful recovery operation
        /// </summary>
        /// <param name="requestId">a requestId of the associated recovery operation</param>
        /// <param name="startTime">a <see cref="DateTime"/> specifying when the recovery started</param>
        /// <param name="interruptedAt">a <see cref="DateTime"/> of the first interruption</param>
        /// <returns>The constructed <see cref="RecoveryResult"/> instance</returns>
        public static RecoveryResult ForSuccess(long requestId, DateTime startTime, DateTime? interruptedAt)
        {
            return new RecoveryResult(true, requestId, startTime, interruptedAt, false);
        }

        /// <summary>
        /// Constructs and returns a <see cref="RecoveryResult"/> indicating timed-out recovery operation
        /// </summary>
        /// <param name="requestId">a requestId of the associated recovery operation</param>
        /// <param name="startTime">a <see cref="DateTime"/> specifying when the recovery started</param>
        /// <returns>The constructed <see cref="RecoveryResult"/> instance</returns>
        public static RecoveryResult ForTimeOut(long requestId, DateTime startTime)
        {
            return new RecoveryResult(false, requestId, startTime, null, true);
        }
    }
}
