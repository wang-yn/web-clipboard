@echo off
echo Building web clipboard locally on Windows...

cd backend

echo Cleaning previous build...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
if exist ..\publish rmdir /s /q ..\publish

echo Restoring packages...
dotnet restore -r linux-x64

echo Publishing optimized Linux build...
dotnet publish -c Release -r linux-x64 ^
    --self-contained true ^
    --no-restore ^
    -p:PublishAot=false ^
    -p:PublishSingleFile=true ^
    -p:PublishTrimmed=true ^
    -p:TrimMode=partial ^
    -p:SuppressTrimAnalysisWarnings=true ^
    -p:EnableCompressionInSingleFile=true ^
    -o ..\publish

cd ..

echo Build complete! Files in publish folder:
dir publish

echo Building Docker image with pre-compiled binary...
docker build -f Dockerfile.prebuilt -t web-clipboard-prebuilt .

echo Done! Run with:
echo docker run -p 8080:8080 web-clipboard-prebuilt