// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto.Lottery;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping.Lottery
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> used to map <see cref="lotteries" /> instances to <see cref="EntityList{T}" /> instances
    /// </summary>
    /// <seealso cref="ISingleTypeMapper{LotteryDto}" />
    internal class LotteriesMapper : ISingleTypeMapper<EntityList<LotteryDto>>
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

        public EntityList<LotteryDto> Map()
        {
            var items = _data.lottery.Select(s => new LotteryDto(s)).ToList();
            return new EntityList<LotteryDto>(items);
        }
    }
}
