A UnifiedOdds Feed SDK library

For more information please contact support@sportradar.com or visit https://iodocs.betradar.com/unifiedsdk/index.html

Important: Version 1.1.0.0 includes breaking changes, below are the steps needed to update 3rd party code.
1. replace/rename ISportEvent to ICompetition (make sure to search whole words with 'Match case' enabled)
2. replace/rename ISportEntity to ISportEvent (make sure to search whole words with 'Match case' enabled)
3. resolve any remaining issues


CHANGE LOG:
2019-09-05  0.9.0.0
Exposed option to delete old matches from cache - introduced ISportDataProviderV4
Loading home and away penalty score from penalty PeriodScore if present


2016-06-17 Initial Version (alpha)