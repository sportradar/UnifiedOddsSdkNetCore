// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using System.Text.RegularExpressions;
using Dawn;
using Microsoft.Extensions.Logging;
using Sportradar.OddsFeed.SDK.Common.Enums;
using Sportradar.OddsFeed.SDK.Common.Internal.Extensions;
using Sportradar.OddsFeed.SDK.Common.Internal.Telemetry;

namespace Sportradar.OddsFeed.SDK.Common
{
    /// <summary>
    /// Represents a Uniform Resource Name
    /// </summary>
    public class Urn
    {
        /// <summary>
        /// A regex pattern used for parsing of Urn strings
        /// </summary>
        private static readonly string RegexPattern = $@"\A(?<{PrefixGroupName}>[a-zA-Z]+):(?<{TypeGroupName}>[a-zA-Z_]+):(?<{IdGroupName}>[-]?\d+)\z";

        /// <summary>
        /// The name of the regex group used to store the prefix
        /// </summary>
        private const string PrefixGroupName = "prefix";

        /// <summary>
        /// The name of the regex group used to store the type
        /// </summary>
        private const string TypeGroupName = "type";

        /// <summary>
        /// The name of the regex group used to store the id
        /// </summary>
        private const string IdGroupName = "id";

        /// <summary>
        /// Defines supported resource types
        /// </summary>
        private static readonly Tuple<string, ResourceTypeGroup>[] Types =
            {
                new Tuple<string, ResourceTypeGroup>("sport_event", ResourceTypeGroup.Match),
                new Tuple<string, ResourceTypeGroup>("race_event", ResourceTypeGroup.Stage),
                new Tuple<string, ResourceTypeGroup>("season", ResourceTypeGroup.Season),
                new Tuple<string, ResourceTypeGroup>("tournament", ResourceTypeGroup.Tournament),
                new Tuple<string, ResourceTypeGroup>("race_tournament", ResourceTypeGroup.Stage),
                new Tuple<string, ResourceTypeGroup>("stage", ResourceTypeGroup.Stage),
                new Tuple<string, ResourceTypeGroup>("simple_tournament", ResourceTypeGroup.BasicTournament),
                new Tuple<string, ResourceTypeGroup>("h2h_tournament", ResourceTypeGroup.Tournament),
                new Tuple<string, ResourceTypeGroup>("outright", ResourceTypeGroup.Tournament),
                new Tuple<string, ResourceTypeGroup>("sport", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("category", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("match", ResourceTypeGroup.Match),
                new Tuple<string, ResourceTypeGroup>("team", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("competitor", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("simpleteam", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("simple_team", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("venue", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("player", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("referee", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("market", ResourceTypeGroup.Other),
                new Tuple<string, ResourceTypeGroup>("lottery", ResourceTypeGroup.Lottery),
                new Tuple<string, ResourceTypeGroup>("draw", ResourceTypeGroup.Draw),
                new Tuple<string, ResourceTypeGroup>("competition_group", ResourceTypeGroup.Stage),
                new Tuple<string, ResourceTypeGroup>("group", ResourceTypeGroup.Other)
            };

        /// <summary>
        /// Gets the prefix of the current instance.
        /// </summary>
        /// <value>The prefix.</value>
        public string Prefix { get; }

        /// <summary>
        /// Gets a <see cref="string"/> specifying the type of the resource associated with the current instance
        /// </summary>
        public string Type { get; }

        /// <summary>
        /// Gets a <see cref="ResourceTypeGroup"/> enum member describing the group of the resource
        /// </summary>
        /// <seealso cref="ResourceTypeGroup"/>
        public ResourceTypeGroup TypeGroup { get; }

        /// <summary>
        /// Gets the numerical part of the identifier associated with the current instance
        /// </summary>
        public long Id { get; }

        /// <summary>
        /// Initializes a new instance of the <see cref="Urn"/> class.
        /// </summary>
        /// <param name="prefix">The prefix of the Urn</param>
        /// <param name="type">The type of the resource associated with the Urn</param>
        /// <param name="id">The numerical identifier of the resource associated with the Urn</param>
        public Urn(string prefix, string type, long id)
        {
            Guard.Argument(prefix, nameof(prefix)).NotNull().NotEmpty();
            Guard.Argument(type, nameof(type)).NotNull().NotEmpty();
            Guard.Argument(id, nameof(id)).NotZero();

            var tuple = Types.FirstOrDefault(t => t.Item1 == type);

            TypeGroup = tuple?.Item2 ?? ResourceTypeGroup.Unknown;
            Prefix = prefix;
            Type = type;
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Urn"/> class.
        /// </summary>
        /// <param name="urn">The urn to be used as a template</param>
        public Urn(Urn urn)
            : this(urn?.Prefix, urn?.Type, urn?.Id ?? 0)
        {
        }

        /// <summary>
        /// Constructs a <see cref="Urn"/> instance by parsing the provided <see cref="string"/>
        /// </summary>
        /// <param name="urnString">The <see cref="string"/> representation of the Urn</param>
        /// <returns>A <see cref="Urn"/> constructed by parsing the provided string representation</returns>
        /// <exception cref="FormatException">The format of the provided representation is not correct</exception>
        public static Urn Parse(string urnString)
        {
            return Parse(urnString, false);
        }

        /// <summary>
        /// Constructs a <see cref="Urn"/> instance by parsing the provided <see cref="string"/>
        /// </summary>
        /// <param name="urnString">The <see cref="string"/> representation of the Urn</param>
        /// <param name="isCustomType">Indicates if the urn type is for custom use</param>
        /// <returns>A <see cref="Urn"/> constructed by parsing the provided string representation</returns>
        /// <exception cref="FormatException">The format of the provided representation is not correct</exception>
        public static Urn Parse(string urnString, bool isCustomType)
        {
            Guard.Argument(urnString, nameof(urnString)).NotNull().NotEmpty();

            var match = Regex.Match(urnString, RegexPattern, RegexOptions.IgnoreCase, TimeSpan.FromSeconds(10));
            if (!match.Success)
            {
                throw new FormatException($"Value '{urnString}' is not a valid string representation of the Urn");
            }

            var type = match.Groups[TypeGroupName].Value;
            if (!isCustomType && Types.All(t => t.Item1 != type))
            {
                SdkLoggerFactory.GetLoggerForExecution(typeof(Urn)).LogDebug("Urn resource type name: {UrnType} is not supported. Input={Urn}", type, urnString);
            }

            return new Urn(match.Groups[PrefixGroupName].Value,
                           match.Groups[TypeGroupName].Value,
                           long.Parse(match.Groups[IdGroupName].Value));
        }

        /// <summary>
        /// Tries to construct a <see cref="Urn"/> instance by parsing the provided <see cref="string"/>
        /// </summary>
        /// <param name="urnString">The <see cref="string"/> representation of the Urn</param>
        /// <param name="urn">When the method returns it contains the <see cref="Urn"/> constructed by parsing the provided string if the parsing was successful, otherwise null</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool TryParse(string urnString, out Urn urn)
        {
            var success = false;

            try
            {
                urn = Parse(urnString);
                success = true;
            }
            catch (Exception)
            {
                urn = null;
            }
            return success;
        }

        /// <summary>
        /// Tries to construct a <see cref="Urn"/> instance by parsing the provided <see cref="string"/>
        /// </summary>
        /// <param name="urnString">The <see cref="string"/> representation of the Urn</param>
        /// <param name="isCustomType">Indicates if the urn type is for custom use</param>
        /// <param name="urn">When the method returns it contains the <see cref="Urn"/> constructed by parsing the provided string if the parsing was successful, otherwise null</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool TryParse(string urnString, bool isCustomType, out Urn urn)
        {
            var success = false;

            try
            {
                urn = Parse(urnString, isCustomType);
                success = true;
            }
            catch (Exception)
            {
                urn = null;
            }
            return success;
        }

        /// <summary>
        /// Returns a <see cref="string" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="string" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{Prefix}:{Type}:{Id.ToString()}";
        }

        /// <summary>
        /// Determines whether the specified <see cref="object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (!(obj is Urn other))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            return Prefix == other.Prefix && Type == other.Type && Id == other.Id;
        }

        /// <summary>
        /// Returns a hash code for this instance.
        /// </summary>
        /// <returns>A hash code for this instance, suitable for use in hashing algorithms and data structures like a hash table.</returns>
        public override int GetHashCode()
        {
            return ToString().GetHashCode();
        }

        /// <summary>
        /// Determines whether Urn represents a simple team
        /// </summary>
        /// <returns><c>true</c> if represents simple team; otherwise, <c>false</c>.</returns>
        public bool IsSimpleTeam()
        {
            return !Type.IsNullOrEmpty() && (Type.Equals("simpleteam", StringComparison.OrdinalIgnoreCase) || Type.Equals("simple_team", StringComparison.OrdinalIgnoreCase));
        }

        /// <summary>
        /// Determines whether Urn represents a simple team
        /// </summary>
        /// <param name="urn">The string urn to be checked</param>
        /// <returns><c>true</c> if represents simple team; otherwise, <c>false</c>.</returns>
        public static bool IsSimpleTeam(string urn)
        {
            return !urn.IsNullOrEmpty() && (urn.Contains("simpleteam", StringComparison.OrdinalIgnoreCase) || urn.Contains("simple_team", StringComparison.OrdinalIgnoreCase));
        }
    }
}
