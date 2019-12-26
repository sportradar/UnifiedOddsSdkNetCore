/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Runtime.Serialization;
using System.Text;
// ReSharper disable MemberCanBePrivate.Global
// ReSharper disable MemberCanBeProtected.Global

namespace Sportradar.OddsFeed.SDK.Common.Exceptions
{
    /// <summary>
    /// An exception thrown by the SDK when the entity received from the feed could not be mapped to entity used by the SDK
    /// </summary>
    /// <seealso cref="FeedSdkException" />
    [Serializable]
    public class MappingException : FeedSdkException
    {
        /// <summary>
        /// Gets the name of the property which caused the exception.
        /// </summary>
        /// <value>The name of the property.</value>
        public string PropertyName { get; }

        /// <summary>
        /// Gets the <see cref="string"/> representation of the property value which caused the exception.
        /// </summary>
        /// <value>The property value.</value>
        public string PropertyValue { get; }

        /// <summary>
        /// Gets the <see cref="TargetTypeName"/> of the target entity.
        /// </summary>
        /// <value>The name of the target type.</value>
        public string TargetTypeName { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        public MappingException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="propertyName">The name of the property which caused the exception</param>
        /// <param name="propertyValue">The <see cref="string"/> representation of the property value which caused the exception</param>
        /// <param name="targetTypeName">The <see cref="TargetTypeName"/> of the target entity</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public MappingException(string message, string propertyName, string propertyValue, string targetTypeName,  Exception innerException)
            : base(message, innerException)
        {
            PropertyName = propertyName;
            PropertyValue = propertyValue;
            TargetTypeName = targetTypeName;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="MappingException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        public MappingException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
            PropertyName = info.GetString("sdkPropertyName");
            PropertyValue = info.GetString("sdkPropertyValue");
            TargetTypeName = info.GetString("sdkTargetTypeName");
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sdkPropertyName", PropertyName);
            info.AddValue("sdkPropertyValue", PropertyValue);
            info.AddValue("sdkTargetTypeName", TargetTypeName);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());

            sb.Append(" PropertyName=").Append(PropertyName)
                .Append(" PropertyValue=").Append(PropertyValue)
                .Append(" TargetTypeName=").Append(TargetTypeName);

            return sb.ToString();
        }
    }
}
