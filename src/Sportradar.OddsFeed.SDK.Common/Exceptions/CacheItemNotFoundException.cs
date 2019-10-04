/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Runtime.Serialization;
using System.Text;

namespace Sportradar.OddsFeed.SDK.Common.Exceptions
{
    /// <summary>
    /// An exception thrown by the SDK cache components when the requested key was not found in the cache
    /// </summary>
    /// <seealso cref="Sportradar.OddsFeed.SDK.Common.Exceptions.FeedSdkException" />
    /// <seealso cref="FeedSdkException" />
    [Serializable]
    public class CacheItemNotFoundException : FeedSdkException
    {
        /// <summary>
        /// Gets the key requested key
        /// </summary>
        /// <value>The key.</value>
        public string Key { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemNotFoundException"/> class.
        /// </summary>
        public CacheItemNotFoundException()
        {
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemNotFoundException"/> class.
        /// </summary>
        /// <param name="message">The error message that explains the reason for the exception.</param>
        /// <param name="key">The key requested key</param>
        /// <param name="innerException">The exception that is the cause of the current exception, or a null reference if no inner exception is specified.</param>
        public CacheItemNotFoundException(string message, string key, Exception innerException)
            : base(message, innerException)
        {
            Key = key;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CacheItemNotFoundException"/> class.
        /// </summary>
        /// <param name="info">The <see cref="SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="StreamingContext" /> that contains contextual information about the source or destination.</param>
        public CacheItemNotFoundException(SerializationInfo info, StreamingContext context) : base(info, context)
        {
            Key = info.GetString("sdkKey");
        }

        /// <summary>
        /// When overridden in a derived class, sets the <see cref="T:System.Runtime.Serialization.SerializationInfo" /> with information about the exception.
        /// </summary>
        /// <param name="info">The <see cref="T:System.Runtime.Serialization.SerializationInfo" /> that holds the serialized object data about the exception being thrown.</param>
        /// <param name="context">The <see cref="T:System.Runtime.Serialization.StreamingContext" /> that contains contextual information about the source or destination.</param>
        public override void GetObjectData(SerializationInfo info, StreamingContext context)
        {
            info.AddValue("sdkKey", Key);
            base.GetObjectData(info, context);
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            var sb = new StringBuilder(base.ToString());

            sb.Append(" Key=").Append(Key);
            return sb.ToString();
        }
    }
}
