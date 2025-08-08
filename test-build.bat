@echo off
echo Testing AOT compatibility...

cd backend

echo Cleaning...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj

echo Testing framework-dependent build first...
dotnet build -c Release

if %errorlevel% neq 0 (
    echo Build failed!
    exit /b 1
)

echo Framework build successful!

echo Testing AOT build...
dotnet publish -c Release -r win-x64 ^
    --self-contained true ^
    -p:PublishAot=true ^
    -p:PublishSingleFile=true ^
    -p:SuppressTrimAnalysisWarnings=true

if %errorlevel% neq 0 (
    echo AOT build failed!
    exit /b 1
)

echo AOT build successful!
cd ..
echo All builds passed!