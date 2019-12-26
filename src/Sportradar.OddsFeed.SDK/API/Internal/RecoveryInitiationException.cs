/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Runtime.Serialization;
using System.Text;

namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// An exception thrown when the recovery operation cannot be initiated due to to after param to far in the past
    /// </summary>
    public class RecoveryInitiationException : Exception
    {
        /// <summary>
        /// Gets a <see cref="DateTime"/> specifying the after parameter of the failed recovery
        /// </summary>
        public DateTime After
        {
            get;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryInitiationException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="after">The after parameter of the recovery operation that caused the exception</param>
        public RecoveryInitiationException(string message, DateTime after)
            : base(message)
        {
            After = after;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryInitiationException"/> class
        /// </summary>
        /// <param name="message">The exception message</param>
        /// <param name="after">The after parameter of the recovery operation that caused the exception</param>
        /// <param name="inner">The <see cref="Exception"/> that caused the current exception to be thrown</param>
        public RecoveryInitiationException(string message, DateTime after, Exception inner)
            : base(message, inner)
        {
            After = after;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="RecoveryInitiationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        public RecoveryInitiationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            After = info.GetDateTime("sdkAfter");

        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sdkAfter", After);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append(" After=").Append(After);
            return sb.ToString();
        }
    }
}
