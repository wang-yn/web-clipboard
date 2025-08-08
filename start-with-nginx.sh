#!/bin/bash

echo "Starting Web Clipboard with Nginx proxy..."

# Check if Docker is available
if ! command -v docker &> /dev/null; then
    echo "Docker not found. Please install Docker first."
    exit 1
fi

if ! command -v docker-compose &> /dev/null; then
    echo "Docker Compose not found. Please install Docker Compose first."
    exit 1
fi

# Create nginx logs directory
mkdir -p nginx/logs

echo "Building and starting services..."
docker-compose -f docker-compose.nginx.yml up --build -d

echo ""
echo "Services started successfully!"
echo ""
echo "Access the application at:"
echo "  HTTP:  http://localhost"
echo "  HTTPS: https://localhost (if configured)"
echo ""
echo "To view logs:"
echo "  docker-compose -f docker-compose.nginx.yml logs -f"
echo ""
echo "To stop services:"
echo "  docker-compose -f docker-compose.nginx.yml down"
echo ""