@echo off
echo Building minimal Linux executable on Windows...

cd backend

echo Cleaning...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
if exist ..\publish-linux rmdir /s /q ..\publish-linux

echo Restoring for Linux x64...
dotnet restore -r linux-x64

echo Publishing with optimization (no AOT for cross-platform compatibility)...
dotnet publish -c Release -r linux-x64 ^
    --self-contained true ^
    --no-restore ^
    -p:PublishAot=false ^
    -p:PublishSingleFile=true ^
    -p:PublishTrimmed=true ^
    -p:TrimMode=partial ^
    -p:SuppressTrimAnalysisWarnings=true ^
    -p:EnableCompressionInSingleFile=true ^
    -o ..\publish-linux

cd ..

echo Linux build complete!
echo Binary size:
dir publish-linux\backend

echo To create Docker image:
echo docker build -f Dockerfile.prebuilt-linux -t web-clipboard-linux .