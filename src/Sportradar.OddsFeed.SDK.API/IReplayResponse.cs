/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.API
{
    /// <summary>
    /// Defines a contract for responses from Replay Server
    /// </summary>
    public interface IReplayResponse
    {
        /// <summary>
        /// Gets the error message, if error occurred, otherwise empty
        /// </summary>
        string ErrorMessage { get; }

        /// <summary>
        /// Gets the message of the response
        /// </summary>
        string Message { get; }

        /// <summary>
        /// Gets a value indicating whether this <see cref="IReplayResponse"/> is success.
        /// </summary>
        bool Success { get; }
    }
}