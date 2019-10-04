/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Diagnostics.Contracts;
using System.Linq;

namespace Sportradar.OddsFeed.SDK.Messages.Contracts
{
    [ContractClassFor(typeof(IProducerManager))]
    internal abstract class ProducerManagerContract : IProducerManager
    {
        public IReadOnlyCollection<IProducer> Producers
        {
            get
            {
                Contract.Ensures(Contract.Result<IReadOnlyCollection<IProducer>>() != null);
                // ReSharper disable once AssignNullToNotNullAttribute
                Contract.Ensures(Contract.Result<IReadOnlyCollection<IProducer>>().Any());

                return Contract.Result<IReadOnlyCollection<IProducer>>();
            }
        }

        public void DisableProducer(int id)
        {
            Contract.Requires(id > 0);
        }

        public IProducer Get(int id)
        {
            Contract.Requires(id > 0);
            Contract.Ensures(Contract.Result<IProducer>() != null);

            return Contract.Result<IProducer>();
        }

        public IProducer Get(string name)
        {
            Contract.Requires(!string.IsNullOrEmpty(name));
            Contract.Ensures(Contract.Result<IProducer>() != null);

            return Contract.Result<IProducer>();
        }

        public bool Exists(int id)
        {
            return Contract.Result<bool>();
        }

        public bool Exists(string name)
        {
            return Contract.Result<bool>();
        }

        public void AddTimestampBeforeDisconnect(int id, DateTime timestamp)
        {
            Contract.Requires(id > 0);
            Contract.Requires(timestamp != null && timestamp > DateTime.MinValue);
        }

        public void RemoveTimestampBeforeDisconnect(int id)
        {
            Contract.Requires(id > 0);
        }
    }
}
