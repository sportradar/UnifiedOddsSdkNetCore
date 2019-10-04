/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Diagnostics.Contracts;
using System.Linq;
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
            Contract.Requires(lotteries != null);

            _data = lotteries;
        }

        /// <summary>
        /// Defines object invariants used by the code contracts
        /// </summary>
        [ContractInvariantMethod]
        private void ObjectInvariant()
        {
            Contract.Invariant(_data != null);
        }

        public EntityList<LotteryDTO> Map()
        {
            var items = _data.lottery.Select(s=> new LotteryDTO(s)).ToList();
            return new EntityList<LotteryDTO>(items);
        }
    }
}
