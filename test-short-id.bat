@echo off
echo Testing short ID functionality...

cd backend

echo Starting application in background...
start /b dotnet run > nul 2>&1

echo Waiting for application to start...
timeout /t 3 /nobreak > nul

echo.
echo Testing API with short IDs...
echo.

echo 1. Saving text...
curl -X POST http://localhost:5000/api/text -H "Content-Type: application/json" -d "{\"content\":\"Hello Short ID Test\"}" 2>nul
echo.
echo.

echo 2. Testing cleanup endpoint...
curl http://localhost:5000/api/cleanup 2>nul
echo.
echo.

echo 3. Uploading a test file...
echo Test file content > test.txt
curl -X POST http://localhost:5000/api/file -F "file=@test.txt" 2>nul
del test.txt
echo.
echo.

echo Short ID test completed!
echo.
echo Check the responses above - IDs should now be 4 characters like: A1B2, X9Y8, etc.
echo.
echo Press any key to stop the application...
pause > nul

taskkill /f /im dotnet.exe > nul 2>&1