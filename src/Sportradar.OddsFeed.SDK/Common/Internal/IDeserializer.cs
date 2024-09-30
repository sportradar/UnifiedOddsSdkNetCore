// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.IO;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
    /// <summary>
    /// Defines a contract implemented by classes used to deserialize feed messages to
    /// <typeparam name="T">Defines the base that can be deserialized using the <see cref="IDeserializer{T}"/></typeparam>
    /// </summary>
    internal interface IDeserializer<T> where T : class
    {
        /// <summary>
        /// Deserialize the provided byte array to a instance of a given type
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> instance containing data to be deserialized </param>
        /// <returns>The <c>data</c> deserialized to instance of a given type</returns>
        /// <exception cref="Exceptions.DeserializationException">The deserialization failed</exception>
        T Deserialize(Stream stream);

        /// <summary>
        /// Deserialize the provided byte array to a instance of a given type
        /// </summary>
        /// <typeparam name="T1">Specifies the type to which to deserialize the data</typeparam>
        /// <param name="stream">A <see cref="Stream"/> instance containing data to be deserialized </param>
        /// <returns>The <c>data</c> deserialized to T1 instance</returns>
        /// <exception cref="Exceptions.DeserializationException">The deserialization failed</exception>
        T1 Deserialize<T1>(Stream stream) where T1 : T;
    }
}
