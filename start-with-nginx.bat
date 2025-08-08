@echo off
echo Starting Web Clipboard with Nginx proxy...

echo Checking if Docker is available...
docker --version >nul 2>&1
if %errorlevel% neq 0 (
    echo Docker not found. Please install Docker first.
    pause
    exit /b 1
)

echo Creating nginx logs directory...
if not exist "nginx\logs" mkdir "nginx\logs"

echo Building and starting services...
docker-compose -f docker-compose.nginx.yml up --build -d

echo.
echo Services started successfully!
echo.
echo Access the application at:
echo   HTTP:  http://localhost
echo   HTTPS: https://localhost (if configured)
echo.
echo To view logs:
echo   docker-compose -f docker-compose.nginx.yml logs -f
echo.
echo To stop services:
echo   docker-compose -f docker-compose.nginx.yml down
echo.
pause