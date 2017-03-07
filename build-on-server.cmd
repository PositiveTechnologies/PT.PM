set msbuild=c:\program files (x86)\msbuild\14.0\bin\msbuild.exe
if exist "%msbuild%" (
	echo.
	echo using msbuild from visual studio 2015
	echo.
) else goto nomsb

call nuget_restore.bat

"%msbuild%" "Sources\PT.PM.sln" /t:Rebuild /p:Configuration=Release

goto :end

:nomsb
echo. 
echo MSBuild not found 
echo.
goto :end

:end