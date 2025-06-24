set LUBAN_DLL=Tools\Luban\Luban.dll
set CONF_ROOT=_Excels

dotnet %LUBAN_DLL% ^
    -t client ^
    -c cs-simple-json ^
    -d json  ^
    --conf luban.conf ^
    -x outputCodeDir=..\MiniGame_EarthDefender\Assets\Scripts\Luban\Output\CSharp ^
    -x outputDataDir=..\MiniGame_EarthDefender\Assets\StreamingAssets\Luban\Output\Json

pause