@echo off
echo Building framework-dependent version (smallest, requires .NET runtime)...

cd backend

echo Cleaning...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
if exist ..\publish-simple rmdir /s /q ..\publish-simple

echo Publishing framework-dependent build...
dotnet publish -c Release ^
    --no-self-contained ^
    -p:PublishAot=false ^
    -p:PublishSingleFile=false ^
    -p:PublishTrimmed=false ^
    -o ..\publish-simple

cd ..

echo Framework-dependent build complete!
echo Files in publish-simple:
dir publish-simple

echo.
echo This version requires .NET 9 runtime on target machine
echo Run with: dotnet backend.dll
echo.
echo To install .NET 9 runtime:
echo https://dotnet.microsoft.com/download/dotnet/9.0