/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO.Lottery;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping.Lottery
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="lotteries" /> instances to <see cref="EntityList{LotteryDTO}" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{LotteryDTO}" />
    internal class LotteriesMapper : ISingleTypeMapper<EntityList<LotteryDTO>>
    {
        /// <summary>
        /// A <see cref="lotteries"/> containing rest data
        /// </summary>
        private readonly lotteries _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="LotteriesMapper"/> class
        /// </summary>
        /// <param name="lotteries">A <see cref="lottery"/> containing list of lotteries</param>
        internal LotteriesMapper(lotteries lotteries)
        {
            Guard.Argument(lotteries, nameof(lotteries)).NotNull();

            _data = lotteries;
        }

        public EntityList<LotteryDTO> Map()
        {
            var items = _data.lottery.Select(s => new LotteryDTO(s)).ToList();
            return new EntityList<LotteryDTO>(items);
        }
    }
}
