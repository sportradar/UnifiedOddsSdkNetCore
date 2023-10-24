/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Linq;
using Dawn;
using Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Dto;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Entities.Rest.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> implementation used to construct <see cref="EntityList{T}" /> instances
    /// from <see cref="producers"/> instances
    /// </summary>
    internal class ProducersMapper : ISingleTypeMapper<EntityList<ProducerDto>>
    {
        /// <summary>
        /// A <see cref="producers"/> instance containing data about available producers
        /// </summary>
        private readonly producers _data;

        /// <summary>
        /// Initializes a new instance of the <see cref="ProducersMapper"/> class
        /// </summary>
        /// <param name="data">The <see cref="producers"/> instance containing data about available producers.</param>
        protected ProducersMapper(producers data)
        {
            Guard.Argument(data, nameof(data)).NotNull();

            _data = data;
        }

        /// <summary>
        /// Maps it's data to instance of <see cref="EntityList{ProducerDto}"/>
        /// </summary>
        /// <returns>The created <see cref="EntityList{ProducerDto}"/> instance</returns>
        public EntityList<ProducerDto> Map()
        {
            if (_data == null || !_data.producer.Any())
            {
                throw new InvalidOperationException("The provided producers instance contains no sports");
            }

            var producers = _data.producer.Select(x => new ProducerDto(x));
            return new EntityList<ProducerDto>(producers);
        }

        /// <summary>
        /// Constructs and returns a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="producers"/> instances
        /// to <see cref="EntityList{ProducerDto}"/> instances
        /// </summary>
        /// <param name="data">A <see cref="producers"/> instance containing tournaments data</param>
        /// <returns>a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="producers"/> instances
        /// to <see cref="EntityList{ProducerDto}"/> instances</returns>
        internal static ISingleTypeMapper<EntityList<ProducerDto>> Create(producers data)
        {
            return new ProducersMapper(data);
        }
    }
}
