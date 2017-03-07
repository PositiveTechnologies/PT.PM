set NUNIT="Sources\packages\NUnit.ConsoleRunner.3.6.0\tools\nunit3-console.exe"
if exist "%NUNIT%" (
	echo.
	echo Using NUnit 3.6.0 from solution packages
	echo.
) else goto nonu

"%NUNIT%" "Tests\PT.PM.UnitTests.nunit" --config=Release --result="Tests\Unit\Bin\Release\TestResult.xml"
goto :end

:nonu
echo. 
echo Nunit not found 
echo.
GOTO :end

:end