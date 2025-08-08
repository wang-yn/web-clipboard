@echo off

echo Building ultra-minimal web clipboard...

cd backend

echo Restoring packages...
dotnet restore -r win-x64

echo Publishing with maximum size optimization...
dotnet publish -c Release -r win-x64 ^
    --self-contained true ^
    --no-restore ^
    -p:PublishReadyToRun=false ^
    -p:PublishSingleFile=true ^
    -p:PublishTrimmed=true ^
    -p:TrimMode=full ^
    -p:EnableCompressionInSingleFile=true ^
    -p:DebuggerSupport=false ^
    -p:EnableUnsafeUTF7Encoding=false ^
    -p:HttpActivityPropagationSupport=false ^
    -p:MetadataUpdaterSupport=false ^
    -p:UseNativeHttpHandler=true ^
    -p:EventSourceSupport=false ^
    -p:UseSystemResourceKeys=true ^
    -p:IlcDisableReflection=true ^
    -p:IlcOptimizationPreference=Size ^
    -p:IlcFoldIdenticalMethodBodies=true ^
    -p:StripSymbols=true ^
    -o ..\publish

echo Build complete!
echo Binary location: ..\publish\backend.exe
dir ..\publish\backend.exe