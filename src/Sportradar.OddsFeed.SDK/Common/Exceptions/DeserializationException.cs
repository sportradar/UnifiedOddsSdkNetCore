/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Runtime.Serialization;
using System.Text;

namespace Sportradar.OddsFeed.SDK.Common.Exceptions
{
    /// <summary>
    /// An exception thrown by the SDK when a deserialization of the xml received from the feed fails
    /// </summary>
    /// <seealso cref="FeedSdkException" />
    [Serializable]
    public class DeserializationException : FeedSdkException
    {
        /// <summary>
        /// Gets the data which could not be deserialized
        /// </summary>
        /// <value>The XML.</value>
        public string Xml { get; }

        /// <summary>
        /// Gets the name of the root xml element associated with the exception or a null reference if element name could not be determined
        /// </summary>
        /// <value>The name of the root element.</value>
        public string RootElementName { get; }


        /// <summary>
        /// Initializes a new instance of the <see cref="DeserializationException"/> class.
        /// </summary>
        public DeserializationException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeserializationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public DeserializationException(string message, Exception innerException)
            : base(message, innerException)
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeserializationException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="xml">The data which could not be deserialized</param>
        /// <param name="rootElementName">The name of the root xml element associated with the exception or a null reference if element name could not be determined</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public DeserializationException(string message, string xml, string rootElementName, Exception innerException)
            : base(message, innerException)
        {
            Xml = xml;
            RootElementName = rootElementName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="DeserializationException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        public DeserializationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            Xml = info.GetString("sdkXml");
            RootElementName = info.GetString("sdkRootElementName");
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sdkXml", Xml);
            info.AddValue("sdkRootElementName", RootElementName);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());
            sb.Append(" RootElementName=").Append(RootElementName)
                .Append(" Xml=").Append(Xml);

            return sb.ToString();
        }
    }
}
