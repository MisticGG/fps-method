@echo off
setlocal

rem PublishReadyToRun removed: pre-compiled native stubs add 25-35 MB and conflict with trimming
set FLAGS=-c Release --self-contained true -p:PublishSingleFile=true

echo.
echo == win-x64 ============================================================
dotnet publish -r win-x64   %FLAGS% -o dist\win-x64
if errorlevel 1 ( echo FAILED: win-x64   & set ERRORS=1 ) else ( move /Y dist\win-x64\FpsMethod.exe dist\win-x64\FpsMethod-win-x64.exe >nul )

echo.
echo == linux-x64 ==========================================================
dotnet publish -r linux-x64 %FLAGS% -o dist\linux-x64
if errorlevel 1 ( echo FAILED: linux-x64 & set ERRORS=1 ) else ( move /Y dist\linux-x64\FpsMethod dist\linux-x64\FpsMethod-linux-x64 >nul )

echo.
echo == osx-x64 ============================================================
dotnet publish -r osx-x64   %FLAGS% -o dist\osx-x64
if errorlevel 1 ( echo FAILED: osx-x64   & set ERRORS=1 ) else ( move /Y dist\osx-x64\FpsMethod dist\osx-x64\FpsMethod-osx-x64 >nul )

echo.
echo == osx-arm64 ==========================================================
dotnet publish -r osx-arm64 %FLAGS% -o dist\osx-arm64
if errorlevel 1 ( echo FAILED: osx-arm64 & set ERRORS=1 ) else ( move /Y dist\osx-arm64\FpsMethod dist\osx-arm64\FpsMethod-osx-arm64 >nul )

echo.
if defined ERRORS (
    echo One or more builds failed.
) else (
    echo All builds succeeded.
    echo.
    echo dist\win-x64\FpsMethod-win-x64.exe
    echo dist\linux-x64\FpsMethod-linux-x64
    echo dist\osx-x64\FpsMethod-osx-x64
    echo dist\osx-arm64\FpsMethod-osx-arm64
)

endlocal
