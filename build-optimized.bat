@echo off
echo Building optimized self-contained version (medium size, no runtime required)...

cd backend

echo Cleaning...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
if exist ..\publish-optimized rmdir /s /q ..\publish-optimized

echo Publishing optimized self-contained build...
dotnet publish -c Release -r win-x64 ^
    --self-contained true ^
    -p:PublishAot=false ^
    -p:PublishSingleFile=true ^
    -p:PublishTrimmed=true ^
    -p:TrimMode=partial ^
    -p:SuppressTrimAnalysisWarnings=true ^
    -p:EnableCompressionInSingleFile=true ^
    -o ..\publish-optimized

cd ..

echo Optimized build complete!
echo Binary size:
dir publish-optimized\backend.exe

echo.
echo This is a self-contained executable that includes the .NET runtime
echo File size: ~40-60MB
echo No .NET runtime installation required on target machine