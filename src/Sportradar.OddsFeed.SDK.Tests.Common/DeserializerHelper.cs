// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

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
}
