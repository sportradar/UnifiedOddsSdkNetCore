// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Caching.CI;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents player's profile information
    /// </summary>
    /// <seealso cref="IPlayer" />
    /// <seealso cref="IPlayerProfile" />
    internal class PlayerProfile : Player, IPlayerProfile
    {
        private readonly PlayerProfileCacheItem _playerProfileCacheItem;
        private readonly IReadOnlyCollection<CultureInfo> _cultures;

        /// <summary>
        /// Gets a value describing the type(e.g. forward, defense, ...) of the player represented by current instance
        /// </summary>
        public string Type => _playerProfileCacheItem.Type;

        /// <summary>
        /// Gets a <see cref="DateTime" /> specifying the date of birth of the player associated with the current instance
        /// </summary>
        public DateTime? DateOfBirth => _playerProfileCacheItem.DateOfBirth;

        /// <summary>
        /// Gets the height in centimeters of the player represented by the current instance or a null reference if height is not known
        /// </summary>
        public int? Height => _playerProfileCacheItem.Height;

        /// <summary>
        /// Gets the weight in kilograms of the player represented by the current instance or a null reference if weight is not known
        /// </summary>
        public int? Weight => _playerProfileCacheItem.Weight;

        /// <summary>
        /// Gets a <see cref="IReadOnlyDictionary{CultureInfo, String}" /> containing player nationality in different languages
        /// </summary>
        public IReadOnlyDictionary<CultureInfo, string> Nationalities => new ReadOnlyDictionary<CultureInfo, string>(_cultures.Where(c => _playerProfileCacheItem.GetNationality(c) != null).ToDictionary(c => c, _playerProfileCacheItem.GetNationality));

        /// <summary>
        /// Gets the gender
        /// </summary>
        public string Gender => _playerProfileCacheItem.Gender;

        /// <summary>
        /// Gets the country code
        /// </summary>
        public string CountryCode => _playerProfileCacheItem.CountryCode;

        /// <summary>
        /// Gets the full name of the player
        /// </summary>
        public string FullName => _playerProfileCacheItem.FullName;

        /// <summary>
        /// Gets the nickname of the player
        /// </summary>
        public string Nickname => _playerProfileCacheItem.Nickname;

        /// <summary>
        /// Initializes a new instance of the <see cref="PlayerProfile"/> class
        /// </summary>
        /// <param name="ci">A <see cref="PlayerProfileCacheItem"/> representing cached player profile info</param>
        /// <param name="cultures">A <see cref="ICollection{CultureInfo}"/> specifying supported languages of the constructed instance</param>
        public PlayerProfile(PlayerProfileCacheItem ci, IReadOnlyCollection<CultureInfo> cultures)
            : base(ci.Id, cultures.Where(c => ci.GetName(c) != null).ToDictionary(c => c, ci.GetName))
        {
            Guard.Argument(ci, nameof(ci)).NotNull();
            Guard.Argument(cultures, nameof(cultures)).NotNull().NotEmpty();

            _playerProfileCacheItem = ci;
            _cultures = cultures;
        }

        /// <summary>
        /// Gets the nationality of the player represented by the current instance in the specified language or a null reference
        /// </summary>
        /// <param name="culture">The culture</param>
        /// <returns>The nationality of the player represented by the current instance in  the language specified by <c>culture</c></returns>
        public string GetNationality(CultureInfo culture)
        {
            return Nationalities.ContainsKey(culture)
                ? Nationalities[culture]
                : null;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing the id of the current instance</returns>
        protected override string PrintI()
        {
            return $"Id={Id}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance</returns>
        protected override string PrintC()
        {
            var defaultCulture = new CultureInfo("en");
            var nationality = Nationalities.ContainsKey(defaultCulture)
                ? Nationalities[defaultCulture]
                : Nationalities.Values.FirstOrDefault();
            return $"{base.PrintC()}, Nationality={nationality}";
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance</returns>
        protected override string PrintF()
        {
            var nationalities = string.Join(" ", Nationalities.Select(x => x.Key.TwoLetterISOLanguageName + ":" + x.Value));
            return $"{base.PrintF()}, Type={Type}, DateOfBirth={DateOfBirth}, FullName={FullName}, Nickname={Nickname}, Weight={Weight}, Height={Height}, Nationality=[{nationalities}], CountryCode={CountryCode}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
