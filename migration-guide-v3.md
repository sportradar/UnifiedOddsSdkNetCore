# UOF .NET SDK - Migration Guide

The UOF .NET SDK 2.x.x is upgraded to 3.0.0, and this is your roadmap to a smooth transition from your current SDK version to the latest version. The upgrade is designed to elevate your experience and
align the SDK more closely with your business needs.

This guide is intended to offer practical advice to ensure your transition is not only efficient but also enhances the performance and capabilities of your software.

## 1. Upgraded dependencies

- RabbitMQ.Client 6.8.1
- Dawn.Guard 1.12.0
- Humanizer 2.14.1
- Microsoft.Bcl.AsyncInterfaces 9.0.1
- Microsoft.Extensions.Caching.Memory 9.0.1
- Microsoft.Extensions.Configuration 9.0.1
- Microsoft.Extensions.DependencyInjection 9.0.1
- Microsoft.Extensions.Diagnostics.HealthChecks 9.0.1
- Microsoft.Extensions.Http 9.0.1
- System.Configuration.ConfigurationManager 9.0.1
- OpenTelemetry 1.11.1
- OpenTelemetry.Extensions.Hosting 1.11.1
- OpenTelemetry.Instrumentation.Runtime 1.10.0 (added)
- OpenTelemetry.Exporter.OpenTelemetryProtocol 1.11.1 (added)
- Removed: OpenTelemetry.Api

## 2. Update the Methods and Classes in Your Code

Review your codebase to identify any parts that might be affected by the upgrade. Look for deprecated methods or classes that have been removed in the new version. Update your code to use the new APIs
provided by the SDK. This may involve making changes to method calls, imports, and references. Handle any breaking changes or deprecations by updating your code accordingly. You can
contact support if you encounter specific issues.

The following classes and methods are changed. Hence, you will need to update your code to use the new names:

#### Removed Methods, Classes and Interfaces

| Removed Methods / Classes / Interfaces | Alternative Methods / Classes / Interfaces |
|----------------------------------------|--------------------------------------------|
| ISoccerEvent                           | IMatch                                     |
| ISoccerStatistics                      | IMatchStatistics                           |
| ISoccerStatus                          | IMatchStatus                               
| ICalculationFilterV1                   | ICalculationFilter                         |
| ICalculationV1                         | ICalculation                               |
| ICustomBetSelectionBuilderV1           | ICustomBetSelectionBuilder                 |
| IMatchStatusV1                         | IMatchStatus                               |
| ISelectionV1                           | ISelection                                 |

#### Removed Methods / Properties

| Method / Property name                                                                           | Recommendation                                                                                                            |
|--------------------------------------------------------------------------------------------------|---------------------------------------------------------------------------------------------------------------------------|
| ExportableCompetition.CompetitorsVirtual                                                         | Use Competitor.IsVirtual property to identify virtual competitors                                                         |
| ICustomBetSelectionBuilder.Build(Urn eventId, int marketId, string specifiers, string outcomeId) | Use ICustomBetSelectionBuilder.Build(Urn eventId, int marketId, string specifiers, string outcomeId, double? odds = null) |
| IRecoveryConfigurationBuilder.SetAdjustAfterAge(bool adjustAfterAge)                             | internally always true                                                                                                    |
| IUofProducerConfiguration.AdjustAfterAge                                                         | internally always true                                                                                                    |

#### Updated Methods / Properties

| Method / Property name | Recommendation                                                                                                                             |
|------------------------|--------------------------------------------------------------------------------------------------------------------------------------------|
| ICompetitor.IsVirtual  | Check it to be true to confirm that the competitor is virual. Consider all other values as an indicator that the competitor is not virtual |

#### Renamed Methods / Properties

- IAvailableSelections.Event to EventId
- IAvailableSelectionsFilter.Event to EventId

#### New Features

SDK Usage Service: This service will allow us to anonymously track producer downtrends, helping us proactively identify potential bottlenecks and broader implementation trends before they lead to
future issues.

New configuration properties:

- IUofUsageConfiguration exposed as IUofConfiguration.Usage for properties configuring usage export (metrics)
- IUofConfiguration.Usage.IsExportEnabled (default: enabled)
- Added IConfigurationBuilder.EnableUsageExport(bool enable) method

## 5. Test your project

Thoroughly test your project after making the changes. Test all critical functionality to ensure that everything still works as expected. Pay special attention to any areas of your setup that interact
with the SDK, as these are likely to be the most affected by the upgrade.

## 6. Update the Documentation

Update your project's documentation and any training materials to reflect the changes introduced by the upgrade. This will help your team members understand and work with the new version.

## 7. Deploy to Production

Once you are confident that your project works correctly with the upgraded SDK, you can deploy the updated version to your production environment.

## 8. Monitoring and Maintenance

After deployment, monitor your project closely for any unexpected issues or performance problems. Be prepared to address any post-upgrade issues promptly.

## 9. Feedback and Reporting

If you encounter any bugs or issues in the SDK, consider reporting them to support@sportradar.com. Providing feedback can help improve the SDK for future releases.