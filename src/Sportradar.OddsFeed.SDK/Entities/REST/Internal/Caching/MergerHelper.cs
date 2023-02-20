/*
* Copyright (C) Sportradar AG. See LICENSE for full license governing this code
*/
using System.Collections.Generic;
using System.Linq;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching.CI;
using Sportradar.OddsFeed.SDK.Entities.REST.Internal.DTO;

namespace Sportradar.OddsFeed.SDK.Entities.REST.Internal.Caching
{
    internal static class MergerHelper
    {
        public static GroupCI FindExistingGroup(List<GroupCI> ciGroups, GroupDTO dtoGroup)
        {
            // find by id
            var resultGroup = !string.IsNullOrEmpty(dtoGroup.Id) ? ciGroups.FirstOrDefault(c => c.Id.Equals(dtoGroup.Id)) : null;

            // find by name
            if (resultGroup == null)
            {
                resultGroup = !string.IsNullOrEmpty(dtoGroup.Name) ? ciGroups.FirstOrDefault(c => c.Name.Equals(dtoGroup.Name)) : null;
            }

            // find by competitors, only for when group id and name not defined
            if (resultGroup == null && string.IsNullOrEmpty(dtoGroup.Id) && string.IsNullOrEmpty(dtoGroup.Name))
            {
                foreach (var existingGroup in ciGroups)
                {
                    if (string.IsNullOrEmpty(existingGroup.Id) && string.IsNullOrEmpty(existingGroup.Name))
                    {
                        if (existingGroup.CompetitorsIds?.Count() != dtoGroup.Competitors.Count())
                        {
                            continue;
                        }

                        // if all competitors match in the group
                        if (dtoGroup.Competitors.All(groupCompetitor => existingGroup.CompetitorsIds.Count(cId => groupCompetitor.Id.Equals(cId)) == 1))
                        {
                            return existingGroup;
                        }
                    }
                }
            }

            return resultGroup;
        }

        public static GroupDTO FindExistingGroup(List<GroupDTO> dtoGroups, GroupCI ciGroup)
        {
            // find by id
            var resultGroup = !string.IsNullOrEmpty(ciGroup.Id) ? dtoGroups.FirstOrDefault(c => c.Id.Equals(ciGroup.Id)) : null;

            // find by name
            if (resultGroup == null)
            {
                resultGroup = !string.IsNullOrEmpty(ciGroup.Name) ? dtoGroups.FirstOrDefault(c => c.Name.Equals(ciGroup.Name)) : null;
            }

            // find by competitors, only for when group id and name not defined
            if (resultGroup == null && string.IsNullOrEmpty(ciGroup.Id) && string.IsNullOrEmpty(ciGroup.Name))
            {
                foreach (var existingGroup in dtoGroups)
                {
                    if (string.IsNullOrEmpty(existingGroup.Id) && string.IsNullOrEmpty(existingGroup.Name))
                    {
                        if (existingGroup.Competitors?.Count() != ciGroup.CompetitorsIds.Count())
                        {
                            continue;
                        }

                        // if all competitors match in the group
                        if (ciGroup.CompetitorsIds.All(cId => existingGroup.Competitors.Count(c => cId.Equals(c.Id)) == 1))
                        {
                            return existingGroup;
                        }
                    }
                }
            }

            return resultGroup;
        }
    }
}
