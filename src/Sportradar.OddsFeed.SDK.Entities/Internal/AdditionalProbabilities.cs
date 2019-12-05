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

        /// <inheritdoc />
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
