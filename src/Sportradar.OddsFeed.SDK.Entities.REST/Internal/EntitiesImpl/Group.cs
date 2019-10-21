/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using Dawn;
using System.Globalization;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;
using Sportradar.OddsFeed.SDK.Messages;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.EntitiesImpl
{
    /// <summary>
    /// Represents a competition group
    /// </summary>
    /// <seealso cref="IGroup" />
    internal class Group : EntityPrinter, IGroupV1
    {
        /// <summary>
        /// The <see cref="Competitors"/> property backing field
        /// </summary>
        private readonly IReadOnlyCollection<ICompetitor> _competitors;

        /// <summary>
        /// Gets the id of the group represented by the current <see cref="IGroup"/> instance
        /// </summary>
        public string Id { get; }

        /// <summary>
        /// Gets the name of the group represented by the current <see cref="IGroup" /> instance
        /// </summary>
        public string Name { get; }

        /// <summary>
        /// Gets the <see cref="IEnumerable{ICompetitor}" /> representing group competitors
        /// </summary>
        public IEnumerable<ICompetitor> Competitors => _competitors;

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class
        /// </summary>
        /// <param name="name">the name of the group represented by the current <see cref="IGroup" /> instance</param>
        /// <param name="competitors">the <see cref="IEnumerable{ICompetitor}" /> representing group competitors</param>
        public Group(string name, IEnumerable<ICompetitor> competitors)
        {
            Id = string.Empty;
            Name = name;
            if (competitors != null)
            {
                _competitors = competitors as IReadOnlyCollection<ICompetitor> ??
                               new ReadOnlyCollection<ICompetitor>(competitors.ToList());
            }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="Group"/> class
        /// </summary>
        /// <param name="ci">A <see cref="GroupCI"/> used to create new instance</param>
        /// <param name="cultures">A culture of the current instance of <see cref="GroupDTO"/></param>
        /// <param name="sportEntityFactory">A <see cref="ISportEntityFactory"/> used to retrieve <see cref="IPlayerProfile"/></param>
        /// <param name="competitorsReferenceIds">A list of <see cref="ReferenceIdCI"/> for all competitors</param>
        public Group(GroupCI ci,
                     IEnumerable<CultureInfo> cultures,
                     ISportEntityFactory sportEntityFactory,
                     IDictionary<URN, ReferenceIdCI> competitorsReferenceIds)
        {
            Guard.Argument(ci).NotNull();

            Id = ci.Id;
            Name = ci.Name;
            if (ci.Competitors != null)
            {
                //var competitors = new List<ICompetitor>();
                //var cultureInfos = cultures.ToList();
                //foreach (CompetitorCI competitorCI in ci.Competitors)
                //{
                //    if (competitorCI == null)
                //    {
                //        var x = "2";
                //    }
                //    var comp = new Competitor(competitorCI, cultureInfos, sportEntityFactory, competitorsReferenceIds);
                //    competitors.Add(comp);
                //}
                //_competitors = competitors;
                _competitors = ci.Competitors.Select(t => sportEntityFactory.BuildCompetitor(t, cultures, competitorsReferenceIds)).ToList();
            }
        }

        /// <summary>
        /// Constructs and returns a <see cref="string"/> containing the id of the current instance
        /// </summary>
        /// <returns>A <see cref="string"/> containing the id of the current instance.</returns>
        protected override string PrintI()
        {
            return $"Group={Name}";
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing compacted representation of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing compacted representation of the current instance.</returns>
        protected override string PrintC()
        {
            var comps = Competitors == null ? string.Empty : string.Join(", ", Competitors.Select(c => c.Id));
            string result = $"{PrintI()}, Competitors=[{comps}]";
            return result;
        }

        /// <summary>
        /// Constructs and return a <see cref="string" /> containing details of the current instance
        /// </summary>
        /// <returns>A <see cref="string" /> containing details of the current instance.</returns>
        protected override string PrintF()
        {
            var comps = Competitors == null ? string.Empty : string.Join(", ", Competitors.Select(c => $"{Environment.NewLine}\t " + c.ToString("F")));
            string result = $"{PrintI()}, Competitors=[{comps}]";
            return result;
        }

        /// <summary>
        /// Constructs and returns a <see cref="string" /> containing a JSON representation of the current instance
        /// </summary>
        /// <returns>a <see cref="string" /> containing a JSON representation of the current instance.</returns>
        protected override string PrintJ()
        {
            return PrintJ(GetType(), this);
        }
    }
}
