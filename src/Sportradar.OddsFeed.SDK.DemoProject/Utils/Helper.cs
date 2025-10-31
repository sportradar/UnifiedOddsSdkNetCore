using Sportradar.OddsFeed.SDK.Api;

namespace Sportradar.OddsFeed.SDK.DemoProject.Utils;

public static class Helper
{
    public static string GetProducerInfo(IProducer producer)
    {
        if (producer == null)
        {
            return string.Empty;
        }

        var producerName = producer.Id == 5 ? "PC" : producer.Name;
        return $"{producer.Id}-{producerName}";
    }

    public static string GetRequestInfo(long? requestId)
    {
        return requestId.HasValue ? $" ({requestId.Value})" : string.Empty;
    }
}