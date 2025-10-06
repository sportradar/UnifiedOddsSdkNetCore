// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using Sportradar.OddsFeed.SDK.Api.Replay;

namespace Sportradar.OddsFeed.SDK.Api.Internal.Replay
{
    /// <summary>
    /// Implementation of <see cref="IReplayResponse"/>
    /// </summary>
    /// <seealso cref="IReplayResponse" />
    internal class ReplayResponse : IReplayResponse
    {
        /// <summary>
        /// Gets a value indicating whether this <see cref="IReplayResponse" /> is success.
        /// </summary>
        public bool Success { get; }

        /// <summary>
        /// Gets the message of the response
        /// </summary>
        public string Message { get; }

        /// <summary>
        /// Gets the error message, if error occurred, otherwise empty
        /// </summary>
        public string ErrorMessage { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ReplayResponse"/> class.
        /// </summary>
        /// <param name="success">if set to <c>true</c> [success].</param>
        /// <param name="message">The message of the response</param>
        /// <param name="error">The error message if error happened</param>
        public ReplayResponse(bool success, string message, string error = null)
        {
            Success = success;
            Message = message;
            ErrorMessage = error;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"[Success={Success}, Message='{Message}', ErrorMessage='{ErrorMessage}']";
        }
    }
}
