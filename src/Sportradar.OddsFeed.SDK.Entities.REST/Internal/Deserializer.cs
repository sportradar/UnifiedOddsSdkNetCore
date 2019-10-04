/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Diagnostics.Contracts;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Xml;
using System.Xml.Serialization;
using Sportradar.OddsFeed.SDK.Common.Exceptions;
using Sportradar.OddsFeed.SDK.Common.Internal;
using Sportradar.OddsFeed.SDK.Messages.Feed;
using Sportradar.OddsFeed.SDK.Messages.Internal;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal
{
    /// <summary>
    /// A <see cref="IDeserializer{T}" /> implementation which uses <see cref="XmlElement.LocalName" /> property to determine to
    /// which type the data should be deserialized
    /// </summary>
    /// <typeparam name="T">Specifies the type that can be deserialized</typeparam>
    public class Deserializer<T> : IDeserializer<T> where T : class
    {
        /// <summary>
        /// A list of <see cref="Type"/> specifying base types which are supported by the deserializer. All subclasses
        /// of the specified types can be deserialized by the deserializer
        /// </summary>
        // ReSharper disable StaticFieldInGenericType
        private static readonly Type[] BaseTypes = {
                                                       typeof(RestMessage),
                                                       typeof(FeedMessage)
                                                   };
        // ReSharper restore StaticFieldInGenericType

        /// <summary>
        /// A <see cref="IReadOnlyDictionary{String, XmlSerializer}"/> containing serializers for all supported types
        /// </summary>
        // ReSharper disable StaticFieldInGenericType
        private static readonly IReadOnlyDictionary<string, SerializerWithInfo> Serializers;
        // ReSharper restore StaticFieldInGenericType

        /// <summary>
        /// Initializes the <code>Serializers</code> static field
        /// </summary>
        static Deserializer()
        {
            var serializers = new Dictionary<string, SerializerWithInfo>();

            foreach (var baseType in BaseTypes)
            {
                foreach (var feedMessagesType in baseType.Assembly.GetTypes().Where(t => t != baseType && baseType.IsAssignableFrom(t)))
                {
                    var xmlRootAttribute = feedMessagesType.GetCustomAttribute<XmlRootAttribute>(false);
                    var ignoreNamespaceAttribute = feedMessagesType.GetCustomAttribute<OverrideXmlNamespaceAttribute>(false);

                    var rootElementName = xmlRootAttribute == null || string.IsNullOrWhiteSpace(xmlRootAttribute.ElementName)
                        ? ignoreNamespaceAttribute == null
                            ? null
                            : ignoreNamespaceAttribute.RootElementName
                        : xmlRootAttribute.ElementName;

                    if (string.IsNullOrWhiteSpace(rootElementName))
                    {
                        throw new InvalidOperationException($"Type {feedMessagesType.FullName} cannot be deserialized with {typeof(Deserializer<>).FullName} because the name of RootXmlElement is not specified");
                    }
                    if (serializers.ContainsKey(rootElementName))
                    {
                        throw new InvalidOperationException($"Deserializer associated with name {rootElementName} already exists");
                    }

                    var ignoreNamespace = ignoreNamespaceAttribute?.IgnoreNamespace ?? false;

                    serializers.Add(rootElementName, new SerializerWithInfo(new XmlSerializer(feedMessagesType), ignoreNamespace));
                }
            }

            Serializers = new ReadOnlyDictionary<string, SerializerWithInfo>(serializers);
        }

        /// <summary>
        /// Deserialize the provided<see cref="byte"/> array to a <see cref="FeedMessage"/> derived instance
        /// </summary>
        /// <param name="stream">A <see cref="Stream"/> instance containing data to be deserialized </param>
        /// <returns>The <code>data</code> deserialized to <see cref="FeedMessage"/> instance</returns>
        /// <exception cref="DeserializationException">The deserialization failed</exception>
        public T Deserialize(Stream stream)
        {
            return Deserialize<T>(stream);
        }

        /// <summary>
        /// Deserialize the provided <see cref="byte" /> array to a <typeparamref name="T1" /> instance
        /// </summary>
        /// <typeparam name="T1">A <typeparamref name="T" /> derived type specifying the target of deserialization</typeparam>
        /// <param name="stream">A <see cref="Stream" /> instance containing data to be deserialized</param>
        /// <returns>The <code>data</code> deserialized to <typeparamref name="T1" /> instance</returns>
        /// <exception cref="DeserializationException">The deserialization failed</exception>
        public T1 Deserialize<T1>(Stream stream) where T1 : T
        {
            using (var reader = new NamespaceIgnorantXmlTextReader(stream))
            {
                bool startElementFound;
                try
                {
                    startElementFound = reader.IsStartElement();
                }
                catch (XmlException ex)
                {
                    throw new DeserializationException("The format of the xml is not correct", stream.GetData(), null, ex);
                }

                if (!startElementFound)
                {
                    throw new DeserializationException("Could not retrieve the name of the root element", stream.GetData(), null, null);
                }

                var localName = reader.LocalName;
                SerializerWithInfo serializerWithInfo;
                if (!Serializers.TryGetValue(reader.LocalName, out serializerWithInfo))
                {
                    throw new DeserializationException("Specified root element is not supported", stream.GetData(), localName, null);
                }
                reader.IgnoreNamespace = serializerWithInfo.IgnoreNamespace;

                try
                {
                    return (T1)serializerWithInfo.Serializer.Deserialize(reader);
                }
                catch (InvalidOperationException ex)
                {
                    throw new DeserializationException("Deserialization failed", stream.GetData(), localName, ex.InnerException ?? ex);
                }
            }
        }

        /// <summary>
        /// A <see cref="XmlReader"/> derived class, which is capable of deserializing Odds Feed REST messages. Those messages have schema issues
        /// which this class handles. Once the schema issues are fixed this class will be removed
        /// </summary>
        private class NamespaceIgnorantXmlTextReader : XmlTextReader
        {
            private const string DefaultNamespaceUri = "http://schemas.sportradar.com/sportsapi/v1/unified";

            public bool IgnoreNamespace { private get; set; }

            public NamespaceIgnorantXmlTextReader(Stream stream)
                : base(stream)
            {
            }

            public override string NamespaceURI => IgnoreNamespace
                ? string.IsNullOrWhiteSpace(base.NamespaceURI)
                    ? string.Empty
                    : DefaultNamespaceUri
                : base.NamespaceURI;
        }

        private class SerializerWithInfo
        {
            public XmlSerializer Serializer { get; }

            public bool IgnoreNamespace { get; }

            public SerializerWithInfo(XmlSerializer serializer, bool ignoreNamespace)
            {
                Contract.Requires(serializer != null);

                Serializer = serializer;
                IgnoreNamespace = ignoreNamespace;
            }
        }
    }
}
