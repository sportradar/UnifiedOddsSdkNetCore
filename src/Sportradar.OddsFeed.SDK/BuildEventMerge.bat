cd
set bat_dir=%~dp0
pushd "%bat_dir%"
cd
mkdir "lib\netcoreapp3.0"

echo Deleting old dlls
del "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.dll"
del "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.dll"
del "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.API.dll"
del "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.API.dll"
del "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Entities.dll"
del "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.Entities.dll"
del "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Entities.REST.dll"
del "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.Entities.REST.dll"
del "..\..\lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Common.dll"
del "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.Common.dll"
del "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Messages.dll"
del "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.Messages.dll"

pushd "%bat_dir%"
cd
echo Copying new dlls to lib folder

copy "bin\Debug\netcoreapp3.0\Sportradar.OddsFeed.SDK.dll" "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.dll"
copy "bin\Debug\netcoreapp3.0\Sportradar.OddsFeed.SDK.API.dll" "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.API.dll"
copy "bin\Debug\netcoreapp3.0\Sportradar.OddsFeed.SDK.Entities.dll" "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Entities.dll"
copy "bin\Debug\netcoreapp3.0\Sportradar.OddsFeed.SDK.Entities.REST.dll" "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Entities.REST.dll"
copy "bin\Debug\netcoreapp3.0\Sportradar.OddsFeed.SDK.Common.dll" "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Common.dll"
copy "bin\Debug\netcoreapp3.0\Sportradar.OddsFeed.SDK.Messages.dll" "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Messages.dll"

echo Copying new dlls from lib to DemoProject resources folder

copy "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.dll" "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.dll"
copy "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.API.dll" "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.API.dll"
copy "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Entities.dll" "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.Entities.dll"
copy "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Entities.REST.dll" "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.Entities.REST.dll"
copy "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Common.dll" "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.Common.dll"
copy "lib\netcoreapp3.0\Sportradar.OddsFeed.SDK.Messages.dll" "..\Sportradar.OddsFeed.SDK.DemoProject\resources\Sportradar.OddsFeed.SDK.Messages.dll"

echo Creating nuget package ...

..\..\tools\nuget.exe pack bin\Debug\netcoreapp3.0\Sportradar.OddsFeed.SDKCore.nuspec

echo.
echo Creating finished.
echo.
popd