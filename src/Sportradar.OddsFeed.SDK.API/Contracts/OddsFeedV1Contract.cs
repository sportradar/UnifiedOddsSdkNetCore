/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Diagnostics.Contracts;
using Sportradar.OddsFeed.SDK.API.EventArguments;
using Sportradar.OddsFeed.SDK.Entities.REST;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.API.Contracts
{
    [ContractClassFor(typeof(IOddsFeedV1))]
    abstract class OddsFeedV1Contract : IOddsFeedV1
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

        /// <summary>
        /// Gets a <see cref="IBookmakerDetails"/> instance used to get info about bookmaker and token used
        /// </summary>
        public IBookmakerDetails BookmakerDetails
        {
            get
            {
                Contract.Ensures(Contract.Result<IBookmakerDetails>() != null);
                return Contract.Result<IBookmakerDetails>();
            }
        }
    }
}
