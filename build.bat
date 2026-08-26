@echo off
echo Building Panopticon Audit History Search...
echo.
echo Restoring NuGet packages...
dotnet restore
echo.
echo Building project...
dotnet build --configuration Release
echo.
echo Build complete!
echo.
echo To install the plugin:
echo 1. Copy the DLL from bin\Release\net48\ to your XRM ToolBox plugins folder
echo 2. Restart XRM ToolBox
echo 3. Look for "Panopticon Audit History Search" in the plugins list
echo.
pause
