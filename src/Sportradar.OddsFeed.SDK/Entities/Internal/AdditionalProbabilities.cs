/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
namespace Sportradar.OddsFeed.SDK.Entities.Internal
{
    internal class AdditionalProbabilities : IAdditionalProbabilities
    {
        /// <summary>
        /// The win probability
        /// </summary>
        public double? Win { get; }

        /// <summary>
        /// The lose probability
        /// </summary>
        public double? Lose { get; }

        /// <summary>
        /// The half_win probability
        /// </summary>
        public double? HalfWin { get; }

        /// <summary>
        /// The half_lose probability
        /// </summary>
        public double? HalfLose { get; }

        /// <summary>
        /// The refund probability
        /// </summary>
        public double? Refund { get; }

        /// <summary>
        /// Constructor for additional probabilities
        /// </summary>
        /// <param name="win">Win value</param>
        /// <param name="lose">Lose value</param>
        /// <param name="halfWin">HalfWin value</param>
        /// <param name="halfLose">HalfLose value</param>
        /// <param name="refund">Refund value</param>
        public AdditionalProbabilities(double? win, double? lose, double? halfWin, double? halfLose, double? refund)
        {
            Win = win;
            Lose = lose;
            HalfWin = halfWin;
            HalfLose = halfLose;
            Refund = refund;
        }
    }
}
