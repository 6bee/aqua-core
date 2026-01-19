@echo off
set configuration=Release

call "%~dp0clean.bat"
dotnet test "%~dp0Aqua.slnx" -c %configuration%

set exitcode=%errorlevel%
if %exitcode% neq 0 goto :failure

goto :end

:failure
echo build failed!
pause

:end
exit /b %exitcode%