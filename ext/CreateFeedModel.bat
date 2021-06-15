@echo off

REM set DevEnvDir="C:\Program Files\Microsoft Visual Studio 10.0\Common7\Tools"
set DevEnvDir="C:\Program Files (x86)\Microsoft Visual Studio 11.0\Common7\Tools"
set DevEnvDir="C:\Program Files (x86)\Microsoft Visual Studio 12.0\Common7\Tools"
set DevEnvDir="C:\Program Files (x86)\Microsoft Visual Studio 14.0\Common7\Tools"
call %DevEnvDir%\..\Tools\vsvars32.bat


@xsd.exe /c /l:C# unifiedsdk/xsd/UnifiedFeed.xsd /n:Sportradar.OddsFeed.SDK.Messages.Feed
echo.
echo Renaming result file to FeedMessages.cs
del FeedMessages.cs
rename UnifiedFeed.cs FeedMessages.cs