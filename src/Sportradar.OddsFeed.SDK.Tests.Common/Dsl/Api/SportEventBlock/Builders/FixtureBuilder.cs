// Copyright (C) Sportradar AG.See LICENSE for full license governing this code

using System;
using System.Linq;
using Sportradar.OddsFeed.SDK.Common;
using Sportradar.OddsFeed.SDK.Messages.Rest;

namespace Sportradar.OddsFeed.SDK.Tests.Common.Dsl.Api.SportEventBlock.Builders;

// <fixtures_fixture xmlns="http://schemas.sportradar.com/sportsapi/v1/unified" xmlns:xsi="http://www.w3.org/2001/XMLSchema-instance" generated_at="2025-04-08T10:57:54+00:00" xsi:schemaLocation="http://schemas.sportradar.com/sportsapi/v1/unified http://schemas.sportradar.com/ufsportsapi/v1/endpoints/unified/ufsportsapi.xsd">
//   <fixture start_time_confirmed="true" start_time="2025-04-08T17:30:00+00:00" liveodds="booked" status="not_started" next_live_time="2025-04-08T17:30:00+00:00" id="sr:match:58717423" scheduled="2025-04-08T17:30:00+00:00" start_time_tbd="false">
//     <tournament_round type="cup" name="Semifinal" cup_round_matches="7" cup_round_match_number="4" betradar_id="3048" betradar_name="DEL, Playoffs"/>
//     <season start_date="2024-09-19" end_date="2025-04-29" year="24/25" tournament_id="sr:tournament:225" id="sr:season:118991" name="DEL 24/25"/>
//     <tournament id="sr:tournament:225" name="DEL">
//       <sport id="sr:sport:4" name="Ice Hockey"/>
//       <category id="sr:category:41" name="Germany" country_code="DEU"/>
//     </tournament>
//     <competitors>
//       <competitor qualifier="home" id="sr:competitor:3853" name="Adler Mannheim" abbreviation="MAN" short_name="Mannheim" country="Germany" country_code="DEU" gender="male">
//         <reference_ids>
//           <reference_id name="betradar" value="8882"/>
//         </reference_ids>
//       </competitor>
//       <competitor qualifier="away" id="sr:competitor:3856" name="Eisbaren Berlin" abbreviation="BER" short_name="Berlin" country="Germany" country_code="DEU" gender="male">
//         <reference_ids>
//           <reference_id name="betradar" value="7362"/>
//         </reference_ids>
//       </competitor>
//     </competitors>
//     <venue id="sr:venue:2010" name="SAP Arena" capacity="13200" city_name="Mannheim" country_name="Germany" country_code="DEU" map_coordinates="49.46373,8.51757"/>
//     <tv_channels/>
//     <extra_info>
//       <info key="RTS" value="not_available"/>
//       <info key="coverage_source" value="venue"/>
//       <info key="extended_live_markets_offered" value="false"/>
//       <info key="streaming" value="false"/>
//       <info key="auto_traded" value="false"/>
//       <info key="neutral_ground" value="false"/>
//       <info key="period_length" value="20"/>
//       <info key="overtime_length" value="20"/>
//     </extra_info>
//     <coverage_info level="silver" live_coverage="true" covered_from="venue">
//       <coverage includes="basic_score"/>
//       <coverage includes="key_events"/>
//     </coverage_info>
//     <product_info>
//       <is_in_live_score/>
//       <is_in_hosted_statistics/>
//       <is_in_live_match_tracker/>
//       <links>
//         <link name="live_match_tracker" ref="https://widgets.sir.sportradar.com/sportradar/en/standalone/match.lmtPlus#matchId=58717423"/>
//       </links>
//     </product_info>
//     <reference_ids>
//       <reference_id name="BetradarCtrl" value="144965313"/>
//     </reference_ids>
//     <scheduled_start_time_changes>
//       <scheduled_start_time_change old_time="2025-04-01T13:00:00+00:00" new_time="2025-04-08T17:30:00+00:00" changed_at="2025-03-08T05:31:36+00:00"/>
//       <scheduled_start_time_change old_time="2025-03-16T13:00:00+00:00" new_time="2025-04-01T13:00:00+00:00" changed_at="2025-03-07T23:30:59+00:00"/>
//     </scheduled_start_time_changes>
//   </fixture>
// </fixtures_fixture>
public class FixtureBuilder
{
    private fixture _fixture = new fixture();

    public FixtureBuilder WithId(Urn id)
    {
        _fixture.id = id.ToString();
        return this;
    }

    public FixtureBuilder WithSportEvent(Func<SportEventBuilder, SportEventBuilder> builderFunc)
    {
        _fixture = builderFunc(new SportEventBuilder()).BuildFixture();
        return this;
    }

    public FixtureBuilder WithStartTime(DateTime startTime, bool? confirmed = true)
    {
        _fixture.start_time = startTime;
        _fixture.start_timeSpecified = true;
        _fixture.start_time_confirmed = confirmed ?? true;
        _fixture.start_time_confirmedSpecified = confirmed != null;
        _fixture.start_time_tbdSpecified = true;
        if (confirmed.HasValue && confirmed.Value)
        {
            _fixture.start_time_tbd = false;
        }
        else
        {
            _fixture.start_time_tbd = true;
        }
        return this;
    }

    public FixtureBuilder WithReferences(Func<ReferencesBuilder, ReferencesBuilder> builderFunc)
    {
        _fixture.reference_ids = builderFunc(new ReferencesBuilder()).BuildForSportEvent();
        return this;
    }

    public FixtureBuilder AddExtraInfo(string key, string value)
    {
        _fixture.extra_info ??= [];
        var list = _fixture.extra_info.ToList();
        list.Add(new info { key = key, value = value });
        _fixture.extra_info = list.ToArray();
        return this;
    }

    public FixtureBuilder AddTvChannel(string name, DateTime? startTime = null, string streamUrl = null)
    {
        _fixture.tv_channels ??= [];
        var list = _fixture.tv_channels.ToList();
        list.Add(new tvChannel
        {
            name = name,
            start_time = startTime ?? DateTime.Now,
            start_timeSpecified = startTime.HasValue,
            stream_url = streamUrl
        });
        _fixture.tv_channels = list.ToArray();
        return this;
    }

    public FixtureBuilder WithCoverageInfo(Func<CoverageInfoBuilder, CoverageInfoBuilder> builderFunc)
    {
        _fixture.coverage_info = builderFunc(new CoverageInfoBuilder()).Build();
        return this;
    }

    public FixtureBuilder WithProductInfo(Func<ProductInfoBuilder, ProductInfoBuilder> builderFunc)
    {
        _fixture.product_info = builderFunc(new ProductInfoBuilder()).Build();
        return this;
    }

    public FixtureBuilder WithDelayedInfo(int id, string description)
    {
        _fixture.delayed_info = new delayedInfo
        {
            id = id,
            description = description
        };
        return this;
    }

    public FixtureBuilder AddScheduledStartTimeChange(DateTime? oldTime, DateTime? newTime, DateTime? changedAt)
    {
        _fixture.scheduled_start_time_changes ??= [];
        var list = _fixture.scheduled_start_time_changes.ToList();
        list.Add(new scheduledStartTimeChange
        {
            old_time = oldTime ?? DateTime.MinValue,
            new_time = newTime ?? DateTime.MinValue,
            changed_at = changedAt ?? DateTime.MinValue
        });
        _fixture.scheduled_start_time_changes = list.ToArray();
        return this;
    }

    public fixturesEndpoint Build()
    {
        return new fixturesEndpoint
        {
            fixture = _fixture,
            generated_at = DateTime.UtcNow,
            generated_atSpecified = true
        };
    }
}
