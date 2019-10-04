/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Net;
using System.Runtime.Serialization;
using System.Text;

namespace Sportradar.OddsFeed.SDK.Common.Exceptions
{
    /// <summary>
    /// An exception thrown by the SDK when an error occurred while communicating with external source (Feed REST-ful API)
    /// </summary>
    /// <seealso cref="FeedSdkException" />
    [Serializable]
    public class CommunicationException : FeedSdkException
    {
        /// <summary>
        /// Gets the <see cref="string"/> representation of the url specifying the resource which was being accessed
        /// </summary>
        /// <value>The URL.</value>
        public string Url { get; }

        /// <summary>
        /// Gets the <see cref="HttpStatusCode"/> specifying the response's status code
        /// </summary>
        public readonly HttpStatusCode ResponseCode;

        /// <summary>
        /// Gets the <see cref="string"/> representation of the response received from the external source
        /// </summary>
        public readonly string Response;

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        public CommunicationException()
        {
            Url = null;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="url">The <see cref="string"/> representation of the url specifying the resource which was being accessed .</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public CommunicationException(string message, string url, Exception innerException)
            : base(message, innerException)
        {
            Url = url;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="url">The <see cref="string"/> representation of the url specifying the resource which was being accessed .</param>
        /// <param name="responseCode">A <see cref="HttpStatusCode"/> specifying the response code</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public CommunicationException(string message, string url, HttpStatusCode responseCode, Exception innerException)
            : base(message, innerException)
        {
            Url = url;
            ResponseCode = responseCode;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="url">The <see cref="string"/> representation of the url specifying the resource which was being accessed .</param>
        /// <param name="responseCode">A <see cref="HttpStatusCode"/> specifying the response code</param>
        /// <param name="response">A <see cref="string"/> representation of the response received from the external source</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public CommunicationException(string message, string url, HttpStatusCode responseCode, string response, Exception innerException)
            : base(message, innerException)
        {
            Url = url;
            ResponseCode = responseCode;
            Response = response;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CommunicationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        public CommunicationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Url = info.GetString("sdkUrl");
            ResponseCode = (HttpStatusCode) info.GetValue("sdkResponseCode", typeof(HttpStatusCode));
            Response = info.GetString("sdkResponse");
        }


        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sdkUrl", Url);
            info.AddValue("sdkResponseCode", ResponseCode, typeof(HttpStatusCode));
            info.AddValue("sdkResponse", Response);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append(" Url=").Append(Url);
            sb.Append(", Code=").Append(ResponseCode);
            sb.Append(", Response=").Append(Response);
            return sb.ToString();
        }
    }
}
