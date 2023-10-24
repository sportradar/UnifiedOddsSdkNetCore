using System.IO;
using System.Reflection;
using System.Xml;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Tests.Common;

namespace Sportradar.OddsFeed.SDK.Tests.Api.Config;

public static class ConfigurationSectionExtensions
{
    internal static IUofConfigurationSection ToSection(this string xmlConfig)
    {
        using var stringReader = new StringReader(xmlConfig);
        using var xmlReader = XmlReader.Create(stringReader);
        var section = new UofConfigurationSection();
        var deserializer = section.GetType().GetMethod("DeserializeSection",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(XmlReader) },
            null);
        deserializer?.Invoke(section, new object[] { xmlReader });
        return section;
    }

    public static IUofConfiguration ToSdkConfiguration(this string xmlConfig)
    {
        using var stringReader = new StringReader(xmlConfig);
        using var xmlReader = XmlReader.Create(stringReader);
        return xmlReader.ToSdkConfiguration();
    }

    private static IUofConfiguration ToSdkConfiguration(this XmlReader reader)
    {
        var section = new UofConfigurationSection();
        var deserializer = section.GetType().GetMethod("DeserializeSection",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(XmlReader) },
            null);
        deserializer?.Invoke(section, new object[] { reader });

        var ufConfig = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider()).BuildFromConfigFile();

        return ufConfig;
    }
}
