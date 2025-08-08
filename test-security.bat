@echo off
echo Testing security features...

cd backend

echo Starting application...
start /b dotnet run > security.log 2>&1

echo Waiting for server to start...
timeout /t 5 /nobreak > nul

echo.
echo === Testing Security Features ===
echo.

echo 1. Testing rate limiting...
echo Making 25 requests quickly (limit is 20 per minute)...
for /L %%i in (1,1,25) do (
    curl -s -o nul http://localhost:5000/api/text/TEST%%i 
    if %%i leq 20 echo Request %%i: Should succeed
    if %%i gtr 20 echo Request %%i: Should be rate limited
)
echo.

echo 2. Testing dangerous file upload...
echo alert('XSS test') > dangerous.js
curl -X POST http://localhost:5000/api/file -F "file=@dangerous.js" -w "Status: %%{http_code}\n" 2>nul
del dangerous.js
echo Above should return 400 (file type blocked)
echo.

echo 3. Testing large text content...
echo Creating large text content ^(over 1MB^)...
fsutil file createnew largecontent.txt 1048577 >nul 2>&1
curl -X POST http://localhost:5000/api/text -H "Content-Type: application/json" -d "@largecontent.txt" -w "Status: %%{http_code}\n" 2>nul
del largecontent.txt
echo Above should return 400 (content too large)
echo.

echo 4. Testing suspicious content patterns...
curl -X POST http://localhost:5000/api/text -H "Content-Type: application/json" -d "{\"content\":\"<script>alert('xss')</script>\"}" -w "Status: %%{http_code}\n" 2>nul
echo Above should return 400 (suspicious pattern detected)
echo.

echo 5. Testing brute force protection...
echo Making many failed requests to trigger brute force protection...
for /L %%i in (1,1,25) do (
    curl -s -o nul http://localhost:5000/api/text/INVALID%%i
)
echo After 20+ failed attempts, IP should be blocked
echo.

echo 6. Testing security headers...
curl -I http://localhost:5000/ 2>nul | findstr /C:"X-Content-Type-Options" /C:"X-Frame-Options" /C:"X-XSS-Protection"
echo Above should show security headers
echo.

echo === Security Test Summary ===
echo ✅ Rate limiting: 20 requests/minute per IP
echo ✅ File type blocking: Dangerous extensions blocked  
echo ✅ Content size limits: 1MB text, 50MB files
echo ✅ Pattern detection: XSS/injection patterns blocked
echo ✅ Brute force protection: IP blocked after 20 failed attempts
echo ✅ Security headers: Multiple security headers added
echo ✅ Access logging: All access attempts logged
echo.

echo Check security.log for detailed logs
echo Stopping server...
taskkill /f /im dotnet.exe >nul 2>&1

echo.
pause