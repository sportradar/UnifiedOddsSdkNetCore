/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using System.Linq;
using System.Text.RegularExpressions;

namespace Sportradar.OddsFeed.SDK.Messages
{
    /// <summary>
    /// Represents a Uniform Resource Name
    /// </summary>
    // ReSharper disable once InconsistentNaming
    public class URN
    {
        /// <summary>
        /// A regex pattern used for parsing of URN strings
        /// </summary>
        private static readonly string RegexPattern = $@"\A(?<{PrefixGroupName}>[a-zA-Z]+):(?<{TypeGroupName}>[a-zA-Z_]+):(?<{IdGroupName}>\d+)\z";

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
        private static readonly Tuple<string, ResourceTypeGroup>[] Types = {
            new Tuple<string, ResourceTypeGroup>("sport_event", ResourceTypeGroup.MATCH),
            new Tuple<string, ResourceTypeGroup>("race_event", ResourceTypeGroup.STAGE),
            new Tuple<string, ResourceTypeGroup>("season", ResourceTypeGroup.SEASON),
            new Tuple<string, ResourceTypeGroup>("tournament", ResourceTypeGroup.TOURNAMENT),
            new Tuple<string, ResourceTypeGroup>("race_tournament", ResourceTypeGroup.STAGE),
            new Tuple<string, ResourceTypeGroup>("stage", ResourceTypeGroup.STAGE),
            new Tuple<string, ResourceTypeGroup>("simple_tournament", ResourceTypeGroup.BASIC_TOURNAMENT),
            new Tuple<string, ResourceTypeGroup>("h2h_tournament", ResourceTypeGroup.TOURNAMENT),
            new Tuple<string, ResourceTypeGroup>("outright", ResourceTypeGroup.TOURNAMENT),
            new Tuple<string, ResourceTypeGroup>("sport", ResourceTypeGroup.OTHER),
            new Tuple<string, ResourceTypeGroup>("category", ResourceTypeGroup.OTHER),
            new Tuple<string, ResourceTypeGroup>("match", ResourceTypeGroup.MATCH),
            new Tuple<string, ResourceTypeGroup>("team", ResourceTypeGroup.OTHER),
            new Tuple<string, ResourceTypeGroup>("competitor", ResourceTypeGroup.OTHER),
            new Tuple<string, ResourceTypeGroup>("simpleteam", ResourceTypeGroup.OTHER),
            new Tuple<string, ResourceTypeGroup>("venue", ResourceTypeGroup.OTHER),
            new Tuple<string, ResourceTypeGroup>("player", ResourceTypeGroup.OTHER),
            new Tuple<string, ResourceTypeGroup>("referee", ResourceTypeGroup.OTHER),
            new Tuple<string, ResourceTypeGroup>("market", ResourceTypeGroup.OTHER),
            new Tuple<string, ResourceTypeGroup>("lottery", ResourceTypeGroup.LOTTERY),
            new Tuple<string, ResourceTypeGroup>("draw", ResourceTypeGroup.DRAW)
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
        /// Initializes a new instance of the <see cref="URN"/> class.
        /// </summary>
        /// <param name="prefix">The prefix of the URN</param>
        /// <param name="type">The type of the resource associated with the URN</param>
        /// <param name="id">The numerical identifier of the resource associated with the URN</param>
        public URN(string prefix, string type, long id)
        {
            Contract.Requires(!string.IsNullOrEmpty(prefix));
            Contract.Requires(!string.IsNullOrEmpty(type));
            Contract.Requires(id > 0);

            var tuple = Types.FirstOrDefault(t => t.Item1 == type);

            TypeGroup = tuple?.Item2 ?? ResourceTypeGroup.UNKNOWN;
            Prefix = prefix;
            Type = type;
            Id = id;
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="URN"/> class.
        /// </summary>
        /// <param name="urn">The urn to be used as a template</param>
        public URN(URN urn) : this(urn?.Prefix, urn?.Type, urn?.Id ?? 0)
        {
        }

        /// <summary>
        /// Constructs a <see cref="URN"/> instance by parsing the provided <see cref="string"/>
        /// </summary>
        /// <param name="urnString">The <see cref="string"/> representation of the URN</param>
        /// <returns>A <see cref="URN"/> constructed by parsing the provided string representation</returns>
        /// <exception cref="System.FormatException">The format of the provided representation is not correct</exception>
        public static URN Parse(string urnString)
        {
            Contract.Requires(!string.IsNullOrEmpty(urnString));
            Contract.Ensures(Contract.Result<URN>() != null);

            var match = Regex.Match(urnString, RegexPattern);
            if (!match.Success)
            {
                throw new FormatException($"Value '{urnString}' is not a valid string representation of the URN");
            }

            var type = match.Groups[TypeGroupName].Value;
            if (Types.All(t => t.Item1 != type))
            {
                //throw new FormatException($"Resource type name: '{type}' is not supported");
            }

            return new URN(
                match.Groups[PrefixGroupName].Value,
                match.Groups[TypeGroupName].Value,
                long.Parse(match.Groups[IdGroupName].Value));
        }

        /// <summary>
        /// Tries to construct a <see cref="URN"/> instance by parsing the provided <see cref="string"/>
        /// </summary>
        /// <param name="urnString">The <see cref="string"/> representation of the URN</param>
        /// <param name="urn">When the method returns it contains the <see cref="URN"/> constructed by parsing the provided string if the parsing was successful, otherwise null</param>
        /// <returns><c>true</c> if XXXX, <c>false</c> otherwise.</returns>
        public static bool TryParse(string urnString, out URN urn)
        {
            Contract.Requires(!string.IsNullOrEmpty(urnString));

            var success = false;

            try
            {
                urn = Parse(urnString);
                success = true;
            }
            catch (FormatException)
            {
                urn = null;
            }
            return success;
        }

        /// <summary>
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{Prefix}:{Type}:{Id}";
        }

        /// <summary>
        /// Determines whether the specified <see cref="System.Object" /> is equal to this instance.
        /// </summary>
        /// <param name="obj">The object to compare with the current object.</param>
        /// <returns><c>true</c> if the specified <see cref="System.Object" /> is equal to this instance; otherwise, <c>false</c>.</returns>
        public override bool Equals(object obj)
        {
            if (obj == null)
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            var other = obj as URN;
            if (other == null)
            {
                return false;
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
    }
}
