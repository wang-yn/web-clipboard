#!/bin/bash

echo "Building ultra-minimal web clipboard..."

cd backend

echo "Restoring packages..."
dotnet restore -r linux-x64

echo "Publishing with maximum size optimization..."
dotnet publish -c Release -r linux-x64 \
    --self-contained true \
    --no-restore \
    -p:PublishReadyToRun=false \
    -p:PublishSingleFile=true \
    -p:PublishTrimmed=true \
    -p:TrimMode=full \
    -p:EnableCompressionInSingleFile=true \
    -p:DebuggerSupport=false \
    -p:EnableUnsafeUTF7Encoding=false \
    -p:HttpActivityPropagationSupport=false \
    -p:MetadataUpdaterSupport=false \
    -p:UseNativeHttpHandler=true \
    -p:EventSourceSupport=false \
    -p:UseSystemResourceKeys=true \
    -p:IlcDisableReflection=true \
    -p:IlcOptimizationPreference=Size \
    -p:IlcFoldIdenticalMethodBodies=true \
    -p:StripSymbols=true \
    -o ../publish

echo "Build complete!"
echo "Binary size:"
ls -lh ../publish/backend
echo "Total publish folder size:"
du -sh ../publish/