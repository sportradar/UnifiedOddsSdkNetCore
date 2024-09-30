// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.IO;
using System.Text;
using System.Xml.Serialization;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common;

public static class DeserializerHelper
{
    public static T GetDeserializedApiMessage<T>(string xml) where T : RestMessage
    {
        var deserializer = new Deserializer<T>();
        return deserializer.Deserialize(FileHelper.GetStreamFromString(xml));
    }

    public static string SerializeApiMessageToXml<T>(T inputApiObject) where T : RestMessage
    {
        var xmlSerializer = new XmlSerializer(typeof(T));
        using var stringWriter = new Utf8StringWriter();
        xmlSerializer.Serialize(stringWriter, inputApiObject);
        return stringWriter.ToString();
    }

    private class Utf8StringWriter : StringWriter
    {
        public override Encoding Encoding => Encoding.UTF8;
    }
}
