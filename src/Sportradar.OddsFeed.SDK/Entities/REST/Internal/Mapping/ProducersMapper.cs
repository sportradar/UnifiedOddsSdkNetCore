/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using Dawn;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages.REST;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Mapping
{
    /// <summary>
    /// A <see cref="ISingleTypeMapper{T}" /> implementation used to construct <see cref="EntityList{ProducerDTO}" /> instances
    /// from <see cref="producers"/> instances
    /// </summary>
    public class ProducersMapper : ISingleTypeMapper<EntityList<ProducerDTO>>
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
        /// Maps it's data to instance of <see cref="EntityList{ProducerDTO}"/>
        /// </summary>
        /// <returns>The created <see cref="EntityList{ProducerDTO}"/> instance</returns>
        public EntityList<ProducerDTO> Map()
        {
            if (_data == null || !_data.producer.Any())
            {
                throw new InvalidOperationException("The provided producers instance contains no sports");
            }

            var producers = _data.producer.Select(x => new ProducerDTO(x));
            return new EntityList<ProducerDTO>(producers);
        }

        /// <summary>
        /// Constructs and returns a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="producers"/> instances
        /// to <see cref="EntityList{ProducerDTO}"/> instances
        /// </summary>
        /// <param name="data">A <see cref="producers"/> instance containing tournaments data</param>
        /// <returns>a new instance of the <see cref="ISingleTypeMapper{T}"/> instance used to map <see cref="producers"/> instances
        /// to <see cref="EntityList{ProducerDTO}"/> instances</returns>
        internal static ISingleTypeMapper<EntityList<ProducerDTO>> Create(producers data)
        {
            return new ProducersMapper(data);
        }
    }
}