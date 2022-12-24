@echo off

echo.

set platform=%1

set found=0

if "%platform%" == "Linux-x64" set found=1
if "%platform%" == "Win-x64" set found=1
if "%platform%" == "Mac-x64" set found=1

if %found% == 0 (
	echo [91mInvalid platform specified![0m Valid options are: [94mLinux-64[0m, [94mWin-x64[0m or [94mMac-x64[0m
	exit /B 64
)

echo Building with target [94m%1[0m


echo.
echo [104;97mDeleting previous build...[0m

for /f %%i in ('dir /a:d /b Release\%1\*') do rd /s /q Release\%1\%%i
del Release\%1\* /s /f /q 1>nul

echo.
echo [104;97mBuilding VectSharp.Demo...[0m

cd VectSharp.Demo
dotnet publish -c Release -f net7.0 /p:PublishProfile=Properties\PublishProfiles\%1.pubxml
cd ..

echo.
echo [104;97mBuilding VectSharp.Demo3D...[0m

cd VectSharp.Demo3D
dotnet publish -c Release -f net7.0 /p:PublishProfile=Properties\PublishProfiles\%1.pubxml
cd ..

echo.
echo [94mAll done![0m
