// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api;

public class ProducersEndpoint
{
    private readonly producers _producers = new producers { producer = Array.Empty<producer>(), response_code = response_code.OK, response_codeSpecified = true };
    private readonly List<producer> _producerList = new List<producer>();

    public static producers BuildAll()
    {
        var builder = new ProducersEndpoint();
        builder.WithProducer(producer => producer.WithId(1).WithName("LO").WithDescription("Live Odds").WithUrl("https://api.betradar.com/v1/liveodds/").IsActive(true).WithScope("live").WithRecoveryWindow(600));
        builder.WithProducer(producer => producer.WithId(3).WithName("Ctrl").WithDescription("Betradar Ctrl").WithUrl("https://api.betradar.com/v1/pre/").IsActive(true).WithScope("prematch").WithRecoveryWindow(4320));
        builder.WithProducer(producer => producer.WithId(4).WithName("BetPal").WithDescription("BetPal").WithUrl("https://api.betradar.com/v1/betpal/").IsActive(false).WithScope("live").WithRecoveryWindow(4320));
        builder.WithProducer(producer => producer.WithId(5).WithName("PremiumCricket").WithDescription("Premium Cricket").WithUrl("https://api.betradar.com/v1/premium_cricket/").IsActive(true).WithScope("live|prematch").WithRecoveryWindow(4320));
        builder.WithProducer(producer => producer.WithId(6).WithName("VF").WithDescription("Virtual football").WithUrl("https://api.betradar.com/v1/vf/").IsActive(true).WithScope("virtual").WithRecoveryWindow(180));
        builder.WithProducer(producer => producer.WithId(7).WithName("WNS").WithDescription("Numbers Betting").WithUrl("https://api.betradar.com/v1/wns/").IsActive(true).WithScope("prematch").WithRecoveryWindow(4320));
        builder.WithProducer(producer => producer.WithId(8).WithName("VBL").WithDescription("Virtual Basketball League").WithUrl("https://api.betradar.com/v1/vbl/").IsActive(true).WithScope("virtual").WithRecoveryWindow(180));
        builder.WithProducer(producer => producer.WithId(9).WithName("VTO").WithDescription("Virtual Tennis Open").WithUrl("https://api.betradar.com/v1/vto/").IsActive(true).WithScope("virtual").WithRecoveryWindow(180));
        builder.WithProducer(producer => producer.WithId(10).WithName("VDR").WithDescription("Virtual Dog Racing").WithUrl("https://api.betradar.com/v1/vdr/").IsActive(true).WithScope("virtual").WithRecoveryWindow(180));
        builder.WithProducer(producer => producer.WithId(11).WithName("VHC").WithDescription("Virtual Horse Classics").WithUrl("https://api.betradar.com/v1/vhc/").IsActive(true).WithScope("virtual").WithRecoveryWindow(180));
        builder.WithProducer(producer => producer.WithId(12).WithName("VTI").WithDescription("Virtual Tennis In-Play").WithUrl("https://api.betradar.com/v1/vti/").IsActive(true).WithScope("virtual").WithRecoveryWindow(180));
        builder.WithProducer(producer => producer.WithId(14).WithName("C-Odds").WithDescription("Competition Odds").WithUrl("https://api.betradar.com/v1/codds/").IsActive(true).WithScope("live").WithRecoveryWindow(4320));
        builder.WithProducer(producer => producer.WithId(15).WithName("VBI").WithDescription("Virtual Baseball In-Play").WithUrl("https://api.betradar.com/v1/vbi/").IsActive(true).WithScope("virtual").WithRecoveryWindow(180));
        builder.WithProducer(producer => producer.WithId(17).WithName("VCI").WithDescription("Virtual Cricket In-Play").WithUrl("https://api.betradar.com/v1/vci/").IsActive(true).WithScope("virtual").WithRecoveryWindow(180));
        return builder.Build();
    }

    public static ProducersEndpoint Create()
    {
        return new ProducersEndpoint();
    }

    public ProducersEndpoint WithProducer(Action<ProducerBuilder> options)
    {
        var producerBuilder = new ProducerBuilder();
        options(producerBuilder);
        _producerList.Add(producerBuilder.Build());
        return this;
    }

    public ProducersEndpoint WithProducer(int producerId)
    {
        var producers = BuildAll();
        var producer = producers.producer.FirstOrDefault(p => p.id == producerId);
        _producerList.Add(producer);
        return this;
    }

    public ProducersEndpoint WithResponseCode(response_code responseCode)
    {
        _producers.response_code = responseCode;
        _producers.response_codeSpecified = true;
        return this;
    }

    public producers Build()
    {
        _producers.producer = _producerList.ToArray();
        return _producers;
    }

    public class ProducerBuilder
    {
        private readonly producer _producer = new producer();

        public ProducerBuilder WithId(int id)
        {
            _producer.id = id;
            return this;
        }

        public ProducerBuilder WithName(string name)
        {
            _producer.name = name;
            return this;
        }

        public ProducerBuilder WithDescription(string description)
        {
            _producer.description = description;
            return this;
        }

        public ProducerBuilder IsActive(bool active)
        {
            _producer.active = active;
            return this;
        }

        public ProducerBuilder WithScope(string scope)
        {
            _producer.scope = scope;
            return this;
        }

        public ProducerBuilder WithUrl(string url)
        {
            _producer.api_url = url;
            return this;
        }

        public ProducerBuilder WithRecoveryWindow(int state)
        {
            _producer.stateful_recovery_window_in_minutes = state;
            return this;
        }

        public producer Build()
        {
            return _producer;
        }
    }
}
