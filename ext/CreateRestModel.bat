@echo off
echo.
REM set DevEnvDir="C:\Program Files\Microsoft Visual Studio 10.0\Common7\Tools"
set DevEnvDir="C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools"
set DevEnvDir="C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools"
set DevEnvDir="C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools"
call %DevEnvDir%\..\Tools\vsvars32.bat
REM REM REM KILL KILL KILL

echo Deleting old classes:
del RestMessages.cs

echo.
echo Replace xs:integer to xs:int:
"../tools/fart.exe" -r -c *.xsd "xs:integer" "xs:int"

echo.
echo Correct schema namespace (from v0 to v1)
"../tools/fart.exe" -r -c *.xsd "v0/" "v1/"

echo.
echo Generating new classes:
@xsd.exe /c /l:C# /n:Sportradar.OddsFeed.SDK.Messages.REST bsa/v1/endpoints/unified/competitor_profile.xsd bsa/v1/endpoints/unified/fixture_changes.xsd bsa/v1/endpoints/unified/fixtures_fixture.xsd bsa/v1/endpoints/unified/match_summary.xsd bsa/v1/endpoints/unified/match_timeline.xsd bsa/v1/endpoints/unified/player_profile.xsd bsa/v1/endpoints/unified/result.xsd bsa/v1/endpoints/unified/results.xsd bsa/v1/endpoints/unified/schedule.xsd bsa/v1/endpoints/unified/sport_categories.xsd bsa/v1/endpoints/unified/sport_tournaments.xsd bsa/v1/endpoints/unified/sports.xsd bsa/v1/endpoints/unified/tournament_info.xsd bsa/v1/endpoints/unified/tournament_schedule.xsd bsa/v1/endpoints/unified/tournaments.xsd bsa/v1/endpoints/unified/venue_summary.xsd bsa/v1/endpoints/unified/tournament_seasons.xsd bsa/v1/endpoints/unified/race_summary.xsd bsa/v1/endpoints/unified/lottery_schedule.xsd bsa/v1/endpoints/unified/lotteries.xsd bsa/v1/endpoints/unified/draw_fixtures.xsd bsa/v1/endpoints/unified/simpleteam_profile.xsd bsa/v1/endpoints/unified/draw_summary.xsd custombet/v1/endpoints/AvailableSelections.xsd custombet/v1/endpoints/Calculation.xsd custombet/v1/endpoints/ErrorResponse.xsd bsa/v1/endpoints/unified/result_changes.xsd bsa/v1/endpoints/unified/period_summary.xsd bsa/v1/endpoints/unified/race_schedule.xsd custombet/v1/endpoints/Selections.xsd

echo.
echo Renaming result file to RestMessages.cs
rename Selections.cs  RestMessages.cs

REM echo.
REM echo Replace [][] to []:
REM "../tools/fart.exe" -r -c RestMessages.cs "[][]" "[]"

echo.
echo Fixing "Compiling JScript/CSharp scripts is not supported" error
echo Fixing matchPeriod - replace "teamStatistics[][] teams" to "teamStatistics[] teams":
"../tools/fart.exe" -r -c RestMessages.cs "teamStatistics[][] teams" "teamStatistics[] teams"
echo Fixing tournamentSchedule - replace sportEvent[][] sport_events to sportEvent[] sport_events:
"../tools/fart.exe" -r -c RestMessages.cs "sportEvent[][] sport_events" "sportEvent[] sport_events"

echo.
echo Replace sportEventStatus to restSportEventStatus:
"../tools/fart.exe" -r -c RestMessages.cs "sportEventStatus" "restSportEventStatus"

echo.
echo Done.