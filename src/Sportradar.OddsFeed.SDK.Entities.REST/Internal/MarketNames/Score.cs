/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.MarketNames
{
    /// <summary>
    /// Represents a match score
    /// </summary>
    public class Score
    {
        /// <summary>
        /// A score of the home team
        /// </summary>
        public readonly decimal HomeScore;

        /// <summary>
        /// A score of the away team
        /// </summary>
        public readonly decimal AwayScore;

        /// <summary>
        /// Initializes a new instance of the <see cref="Score"/> class
        /// </summary>
        /// <param name="homeScore">A score of the home team</param>
        /// <param name="awayScore">A score of the away team</param>
        public Score(decimal homeScore, decimal awayScore)
        {
            HomeScore = homeScore;
            AwayScore = awayScore;
        }

        /// <summary>
        /// Overrides the + operator
        /// </summary>
        /// <param name="score1">A <see cref="Score"/> representing the first operand of the operation</param>
        /// <param name="score2">A <see cref="Score"/> representing the second operand of the operation.</param>
        /// <returns>A <see cref="Score"/> instance representing the result of the addition.</returns>
        public static Score operator +(Score score1, Score score2)
        {
            Contract.Requires(score1 != null);
            Contract.Requires(score2 != null);
            Contract.Ensures(Contract.Result<Score>() != null);

            return new Score(score1.HomeScore + score2.HomeScore, score1.AwayScore + score2.AwayScore);
        }

        /// <summary>
        /// Constructs and returns a <see cref="Score"/> instance constructed from it's string representation
        /// </summary>
        /// <param name="value">A string representation of a <see cref="Score"/></param>
        /// <returns>a <see cref="Score"/> instance constructed from it's string representation</returns>
        /// <exception cref="FormatException">The format of <code>value</code> is not correct</exception>
        public static Score Parse(string value)
        {
            Contract.Requires(!string.IsNullOrEmpty(value));
            Contract.Ensures(Contract.Result<Score>() != null);

            var parts = value.Split(new[] {":"}, StringSplitOptions.RemoveEmptyEntries);
            if (parts.Length != 2)
            {
                throw new FormatException($"The format of value={value} is not correct. It must contain exactly one : sign");
            }

            decimal home;
            try
            {
                home = decimal.Parse(parts[0]);
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is OverflowException)
                {
                    throw new FormatException($"The representation of home score={parts[0]} is not correct");
                }
                throw;
            }

            decimal away;
            try
            {
                away = decimal.Parse(parts[1]);
            }
            catch (Exception ex)
            {
                if (ex is FormatException || ex is OverflowException)
                {
                    throw new FormatException($"The representation of away score={parts[1]} is not correct");
                }
                throw;
            }
            return new Score(home, away);
        }

        /// <summary>
        /// Attempts to construct a <see cref="Score"/> instance from it's string representation
        /// </summary>
        /// <param name="value">A <see cref="string"/> representation of a <see cref="Score"/>.</param>
        /// <param name="score">A <see cref="Score"/> instance if method returned true, otherwise null.</param>
        /// <returns>True if the provided value could be parsed, otherwise false.</returns>
        public static bool TryParse(string value, out Score score)
        {
            Contract.Requires(!string.IsNullOrEmpty(value));
            try
            {
                score = Parse(value);
                return true;
            }
            catch (FormatException)
            {
                score = null;
                return false;
            }
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

            var other = obj as Score;
            if (other == null)
            {
                return false;
            }

            return other.HomeScore == HomeScore && other.AwayScore == AwayScore;
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
        /// Returns a <see cref="System.String" /> that represents this instance.
        /// </summary>
        /// <returns>A <see cref="System.String" /> that represents this instance.</returns>
        public override string ToString()
        {
            return $"{HomeScore}:{AwayScore}";
        }
    }
}
