// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Configuration;
using System.IO;
using System.Xml;
using Sportradar.OddsFeed.SDK.Api.Config;
using Sportradar.OddsFeed.SDK.Api.Internal.Config;
using Sportradar.OddsFeed.SDK.Tests.Common;

namespace Sportradar.OddsFeed.SDK.Tests.API.Config.ConfigurationBuilder;

public static class ConfigurationSectionExtensions
{
    internal static IUofConfigurationSection ToSection(this string xmlConfig)
    {
        CheckSectionRootName(xmlConfig);

        using var stringReader = new StringReader(xmlConfig);
        using var xmlReader = XmlReader.Create(stringReader);

        var section = new UofConfigurationSectionHelper();
        section.Deserialize(xmlReader);
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
        var section = new UofConfigurationSectionHelper();
        section.Deserialize(xmlReader);

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

    private class UofConfigurationSectionHelper : UofConfigurationSection
    {
        public void Deserialize(XmlReader reader)
        {
            base.DeserializeSection(reader);
        }
    }
}
