/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.IO;
using Sportradar.OddsFeed.SDK.Common.Contracts;

namespace Sportradar.OddsFeed.SDK.Common.Internal
{
#pragma warning disable CS1723
    /// <summary>
    /// Defines a contract implemented by classes used to deserialize feed messages to
    /// <typeparam name="T">Defines the base that can be deserialized using the <see cref="IDeserializer{T}"/></typeparam>
    /// </summary>
    [ContractClass(typeof(DeserializerContract<>))]

    public interface IDeserializer<T> where T : class
    {
        /// <summary>
        /// Deserialize the provided <see cref="byte"/> array to a <see cref="T"/> instance
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> instance containing data to be deserialized </param>
        /// <returns>The <code>data</code> deserialized to <see cref="T"/> instance</returns>
        /// <exception cref="Sportradar.OddsFeed.SDK.Common.Exceptions.DeserializationException">The deserialization failed</exception>
        T Deserialize(Stream stream);

#pragma warning disable CS1574
        /// <summary>
        /// Deserialize the provided <see cref="byte"/> array to a <see cref="T"/> instance
        /// </summary>
        /// <typeparam name="T1">Specifies the type to which to deserialize the data</typeparam>
        /// <param name="stream">A <see cref="Stream"/> instance containing data to be deserialized </param>
        /// <returns>The <code>data</code> deserialized to <see cref="T1"/> instance</returns>
        /// <exception cref="Sportradar.OddsFeed.SDK.Common.Exceptions.DeserializationException">The deserialization failed</exception>
#pragma warning restore CS1574
        T1 Deserialize<T1>(Stream stream) where T1 : T;
    }
#pragma warning restore CS1723
}