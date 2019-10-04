/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IOddsFeed))]
    abstract class OddsFeedContract : IOddsFeed
    {
        public void Dispose()
        {
        }

        [Pure]
        public event EventHandler<EventArgs> Disconnected;

        [Pure]
        public event EventHandler<FeedCloseEventArgs> Closed;

        [Pure]
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerDown;

        [Pure]
        public event EventHandler<ProducerStatusChangeEventArgs> ProducerUp;

        [Pure]
        public IEventRecoveryRequestIssuer EventRecoveryRequestIssuer
        {
            get
            {
                Contract.Ensures(Contract.Result<IEventRecoveryRequestIssuer>() != null);
                return Contract.Result<IEventRecoveryRequestIssuer>();
            }
        }

        [Pure]
        public ISportDataProvider SportDataProvider
        {
            get
            {
                Contract.Ensures(Contract.Result<ISportDataProvider>() != null);
                return Contract.Result<ISportDataProvider>();
            }
        }

        public IProducerManager ProducerManager {
            get
            {
                Contract.Ensures(Contract.Result<IProducerManager>() != null);
                return Contract.Result<IProducerManager>();
            }
        }

        public IBookingManager BookingManager
        {
            get
            {
                Contract.Ensures(Contract.Result<IBookingManager>() != null);
                return Contract.Result<IBookingManager>();
            }
        }

        [Pure]
        public ICashOutProbabilitiesProvider CashOutProbabilitiesProvider
        {
            get
            {
                Contract.Ensures(Contract.Result<ICashOutProbabilitiesProvider>() != null);
                return Contract.Result<ICashOutProbabilitiesProvider>();
            }
        }

        public IOddsFeedSessionBuilder CreateBuilder()
        {
            Contract.Ensures(Contract.Result<IOddsFeedSessionBuilder>() != null);
            return Contract.Result<IOddsFeedSessionBuilder>();
        }

        public void Open()
        {

        }

        public void Close()
        {

        }
    }
}
