// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Configuration;
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
        CheckSectionRootName(xmlConfig);

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
        CheckSectionRootName(xmlConfig);

        using var stringReader = new StringReader(xmlConfig);
        using var xmlReader = XmlReader.Create(stringReader);

        return xmlReader.ToSdkConfiguration();
    }

    private static IUofConfiguration ToSdkConfiguration(this XmlReader xmlReader)
    {
        var section = new UofConfigurationSection();
        var deserializer = section.GetType().GetMethod("DeserializeSection",
            BindingFlags.NonPublic | BindingFlags.Instance | BindingFlags.Public, null, new[] { typeof(XmlReader) },
            null);
        deserializer?.Invoke(section, new object[] { xmlReader });

        var ufConfig = new TokenSetter(new TestSectionProvider(section), new TestBookmakerDetailsProvider(), new TestProducersProvider()).BuildFromConfigFile();

        return ufConfig;
    }

    private static void CheckSectionRootName(string xmlConfig)
    {
        using var stringReader = new StringReader(xmlConfig);
        using var xmlReader = XmlReader.Create(stringReader);

        // Move to the first element
        xmlReader.MoveToContent();

        // Check if the current node is the expected one
        if (xmlReader.Name != UofConfigurationSection.SectionName)
        {
            throw new ConfigurationErrorsException($"Invalid configuration section name. Expected '{UofConfigurationSection.SectionName}'.");
        }
    }
}
