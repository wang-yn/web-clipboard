@echo off
echo Building ultra-minimal Windows executable...

cd backend

echo Cleaning...
if exist bin rmdir /s /q bin
if exist obj rmdir /s /q obj
if exist ..\publish-win rmdir /s /q ..\publish-win

echo Restoring for Windows x64...
dotnet restore -r win-x64

echo Publishing Windows native with AOT optimization...
dotnet publish -c Release -r win-x64 ^
    --self-contained true ^
    --no-restore ^
    -p:PublishAot=true ^
    -p:PublishSingleFile=true ^
    -p:SuppressTrimAnalysisWarnings=true ^
    -p:EnableCompressionInSingleFile=true ^
    -p:IlcOptimizationPreference=Size ^
    -p:IlcFoldIdenticalMethodBodies=true ^
    -o ..\publish-win

cd ..

echo Windows build complete!
echo Binary size:
dir publish-win\backend.exe

echo Run with:
echo cd publish-win
echo backend.exe