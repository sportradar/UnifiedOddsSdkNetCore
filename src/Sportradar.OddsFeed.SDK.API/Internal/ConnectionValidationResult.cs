/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.API.Internal
{
    /// <summary>
    /// Defines possible reasons for connection failure
    /// </summary>
    internal class ConnectionValidationResult
    {
        /// <summary>
        /// Connection was successfully validated
        /// </summary>
        public static readonly ConnectionValidationResult Success = new ConnectionValidationResult("Connection successfully established");

        /// <summary>
        /// The validation failed because the computer is not connected to the internet
        /// </summary>
        public static readonly ConnectionValidationResult NoInternetConnection = new ConnectionValidationResult("The computer is not connected to the internet");

        /// <summary>
        /// The validation failed because the target computer refueled the connection
        /// </summary>
        public static readonly ConnectionValidationResult ConnectionRefused = new ConnectionValidationResult("Access was denied - probably a firewall issue");

        /// <summary>
        /// The reason for failed validation is not known
        /// </summary>
        public static readonly ConnectionValidationResult Unknown = new ConnectionValidationResult("The reason could not be determined");

        /// <summary>
        /// Gets the message associated with the current instance
        /// </summary>
        /// <value>The message.</value>
        internal string Message { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="ConnectionValidationResult"/> class.
        /// </summary>
        /// <param name="message">The message associated with the current instance.</param>
        protected ConnectionValidationResult(string message)
        {
            Message = message;
        }
    }
}