namespace Sportradar.OddsFeed.SDK.Entities
{
    /// <summary>
    /// Additional probability attributes for markets which potentially will be (partly) refunded
    /// </summary>
    /// <remarks>This is valid only for those markets which are sent with x.0, x.25 and x.75 lines and in addition the "no bet" markets (draw no bet, home no bet, ...)</remarks>
    public interface IAdditionalProbabilities
    {
        /// <summary>
        /// The win probability
        /// </summary>
        double? Win { get; }
        /// <summary>
        /// The lose probability
        /// </summary>
        double? Lose { get; }
        /// <summary>
        /// The half_win probability
        /// </summary>
        double? HalfWin { get; }
        /// <summary>
        /// The half_lose probability
        /// </summary>
        double? HalfLose { get; }
        /// <summary>
        /// The refund probability
        /// </summary>
        double? Refund { get; }
    }
}