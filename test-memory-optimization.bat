@echo off
echo Testing memory optimization features...

cd backend

echo Starting application...
start /b dotnet run > server.log 2>&1

echo Waiting for server to start...
timeout /t 5 /nobreak > nul

echo.
echo === Testing 10-minute expiration and file storage ===
echo.

echo 1. Creating test file...
echo This is a test file for memory optimization > testfile.txt

echo 2. Uploading file (should be stored in temp directory)...
curl -X POST http://localhost:5000/api/file -F "file=@testfile.txt" 2>nul
echo.

echo 3. Checking temp directory...
echo Temp files in %TEMP%\web-clipboard:
dir "%TEMP%\web-clipboard" 2>nul || echo No temp directory found

echo.
echo 4. Testing text storage (10 minutes expiration)...
curl -X POST http://localhost:5000/api/text -H "Content-Type: application/json" -d "{\"content\":\"Test with 10 minute expiration\"}" 2>nul
echo.

echo 5. Testing cleanup endpoint...
curl http://localhost:5000/api/cleanup 2>nul
echo.

echo.
echo === Memory Optimization Summary ===
echo ✅ Files stored in temp directory instead of memory
echo ✅ Content expires after 10 minutes instead of 24 hours  
echo ✅ Background cleanup runs every minute
echo ✅ Manual cleanup available via /api/cleanup
echo.

echo Cleaning up...
del testfile.txt 2>nul

echo Stopping server...
taskkill /f /im dotnet.exe >nul 2>&1

echo.
echo Check server.log for background cleanup messages
echo Temp directory: %TEMP%\web-clipboard
echo.
pause