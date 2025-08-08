Write-Host "Building framework-dependent version (smallest, requires .NET runtime)..." -ForegroundColor Green

Set-Location backend

Write-Host "Cleaning..." -ForegroundColor Yellow
if (Test-Path "bin") { Remove-Item -Recurse -Force "bin" }
if (Test-Path "obj") { Remove-Item -Recurse -Force "obj" }
if (Test-Path "..\publish-simple") { Remove-Item -Recurse -Force "..\publish-simple" }

Write-Host "Publishing framework-dependent build..." -ForegroundColor Yellow
dotnet publish -c Release --no-self-contained -p:PublishAot=false -p:PublishSingleFile=false -p:PublishTrimmed=false -o ..\publish-simple

Set-Location ..

Write-Host "Framework-dependent build complete!" -ForegroundColor Green
Write-Host "Files in publish-simple:" -ForegroundColor Cyan
Get-ChildItem publish-simple

Write-Host ""
Write-Host "This version requires .NET 9 runtime on target machine" -ForegroundColor Yellow
Write-Host "Run with: dotnet backend.dll" -ForegroundColor Cyan
Write-Host ""
Write-Host "To install .NET 9 runtime:" -ForegroundColor Yellow
Write-Host "https://dotnet.microsoft.com/download/dotnet/9.0" -ForegroundColor Cyan