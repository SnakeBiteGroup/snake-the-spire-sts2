@echo off
setlocal

dotnet publish SBMod.sln -c Release %*
if errorlevel 1 (
    echo publish failed. >&2
    exit /b 1
)

endlocal
