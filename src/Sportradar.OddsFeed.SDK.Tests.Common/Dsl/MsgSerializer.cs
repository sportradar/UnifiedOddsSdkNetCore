// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.IO;
using System.Text;
using System.Xml;
using System.Xml.Serialization;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl;

public static class MsgSerializer
{
    public static string SerializeToXml<T>(T dataToSerialize)
    {
        var emptyNamespace = new XmlSerializerNamespaces(new[] { XmlQualifiedName.Empty });
        var serializer = new XmlSerializer(typeof(T));
        var settings = new XmlWriterSettings { Indent = false, OmitXmlDeclaration = false };

        using var stream = new Utf8StringWriter();
        using var writer = XmlWriter.Create(stream, settings);

        serializer.Serialize(writer, dataToSerialize, emptyNamespace);
        return stream.ToString();
    }

    private class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
