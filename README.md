# Web Clipboard

A modern web-based clipboard application that allows you to store and share text content and files across devices and browsers.

## Features

- 📝 **Text Clipboard**: Store and share text content with 4-char IDs (e.g. A1B2)
- 📁 **File Upload/Download**: Upload files and download them using short IDs
- 📱 **Mobile-Friendly**: Optimized responsive design for mobile and desktop
- ⌨️ **Easy Input**: Short 4-character IDs easy to type and share
- ⚡ **High Performance**: Built with .NET 9 AOT for minimal resource usage
- 🐳 **Docker Ready**: Easy deployment with Docker and Docker Compose
- 🔒 **Quick Expiration**: Content expires after 10 minutes for memory efficiency
- 💾 **Smart Storage**: Files stored in temp directory, not memory
- 🛡️ **Security Protection**: Rate limiting, brute force protection, content validation
- 📊 **Access Monitoring**: Comprehensive logging and security event tracking
- 🌐 **Nginx Proxy**: Reverse proxy support with load balancing and SSL termination
- 🔒 **Production Ready**: HTTPS, security headers, and enterprise deployment options

## Quick Start

### Using Pre-built Docker Images (Recommended)

```bash
# Option 1: Standard image from GitHub Container Registry
docker run -p 8080:8080 ghcr.io/[username]/web-clipboard:latest

# Option 2: Minimal image (smaller size)
docker run -p 8080:8080 ghcr.io/[username]/web-clipboard-minimal:latest

# Option 3: From Docker Hub (if available)
docker run -p 8080:8080 [username]/web-clipboard:latest

# Access at http://localhost:8080
```

### Using Docker Compose (Development)

```bash
# Clone or download the project
git clone <repository-url>
cd web-clipboard

# Option 1: Simple deployment
docker-compose up -d
# Access at http://localhost:8080

# Option 2: With Nginx proxy (Production)
./start-with-nginx.bat  # Windows
./start-with-nginx.sh   # Linux/Mac
# Access at http://localhost
```

### Manual Setup

#### Prerequisites
- .NET 9 SDK
- Modern web browser

#### Running the Application

```bash
# Navigate to backend directory
cd backend

# Restore dependencies
dotnet restore

# Run the application
dotnet run

# Access the application
# Open http://localhost:5000 in your browser
```

## Usage

### Text Clipboard
1. Enter text in the text area
2. Click "Save Text" to store it and get a unique ID
3. Share the ID with others or use it on different devices
4. Enter an ID and click "Load Text" to retrieve stored text
5. Use "Copy Text" to copy content to your system clipboard

### File Clipboard
1. Click "Select File" or drag & drop a file
2. Click "Upload File" to store it and get a unique ID
3. Share the ID with others or use it on different devices
4. Enter an ID and click "Download File" to retrieve the file

### Recent Items
- View your recently created items with quick access buttons
- Copy IDs to clipboard or load items directly
- Items are stored locally and automatically cleaned up when expired

## API Endpoints

- `POST /api/text` - Store text content
- `GET /api/text/{id}` - Retrieve text content
- `POST /api/file` - Upload file
- `GET /api/file/{id}` - Download file
- `DELETE /api/{id}` - Delete item
- `GET /api/cleanup` - Clean expired items

## Technical Details

### Backend
- .NET 9 with AOT (Ahead of Time) compilation for optimal performance
- Minimal API design for reduced overhead
- In-memory storage with automatic expiration (24 hours)
- CORS enabled for cross-origin requests
- Static file serving for the frontend

### Frontend
- Vanilla JavaScript for minimal bundle size
- Tailwind CSS for responsive design
- Local storage for recent items tracking
- Drag & drop file upload support
- Mobile-optimized interface

### Docker Deployment
- Multi-stage Docker build for optimized image size
- Alpine Linux base for minimal runtime
- Docker Compose for easy orchestration
- Configurable port mapping (default: 8080)

## Configuration

### Environment Variables
- `ASPNETCORE_URLS`: Configure listening URLs (default: http://+:8080)
- `ASPNETCORE_ENVIRONMENT`: Set environment (Development/Production)

### Customization
- Modify expiration time in `Program.cs` (default: 24 hours)
- Adjust file size limits in the backend configuration
- Customize UI styling in `index.html` and Tailwind classes

## Security Testing

### Run Security Tests
```bash
# Test all security features
./test-security.bat

# Check security logs
tail -f backend/security.log
```

### Security Features
- **Rate Limiting**: 20 uploads, 100 downloads per minute per IP
- **File Type Blocking**: Dangerous extensions (.exe, .bat, .js, etc.) blocked
- **Content Validation**: XSS and injection pattern detection
- **Brute Force Protection**: Auto-block IPs after 20 failed attempts
- **Size Limits**: 1MB text, 50MB files maximum
- **Security Headers**: XSS protection, frame options, content sniffing prevention

See [SECURITY.md](SECURITY.md) for detailed security documentation.

## Nginx Proxy Deployment

### Quick Start with Nginx
```bash
# Start with Nginx proxy
./start-with-nginx.bat  # Windows
./start-with-nginx.sh   # Linux/Mac

# Test proxy functionality
./test-nginx-proxy.bat

# Access application
http://localhost        # Through Nginx proxy
http://localhost:5000   # Direct backend access
```

### Features
- **Reverse Proxy**: Load balancing and failover support
- **Rate Limiting**: 10 API requests, 5 uploads per second per IP
- **SSL Termination**: HTTPS support with modern TLS
- **Security Headers**: XSS protection, frame options, content sniffing prevention
- **Caching**: Static file caching for better performance
- **Health Checks**: Automatic backend health monitoring

### Configuration Files
- `nginx/nginx.conf` - Full production configuration
- `nginx/nginx-dev.conf` - Development environment
- `nginx/nginx-docker.conf` - Docker environment
- `nginx/ssl-example.conf` - HTTPS configuration template

See [NGINX-DEPLOYMENT.md](NGINX-DEPLOYMENT.md) for detailed deployment guide.

## Development

### Building for Production
```bash
cd backend
dotnet publish -c Release -o ../publish
```

### Windows Local Build Options (Recommended)

Multiple build options to meet different needs:

**1. Framework-dependent version (Smallest size):**
```bash
./build-simple.bat
# Or use PowerShell: ./build-simple.ps1
# Output: publish-simple/ (~2-5MB)
# Requires .NET 9 runtime on target machine
```

**2. Optimized self-contained version (Balanced):**
```bash  
./build-optimized.bat
# Output: publish-optimized/backend.exe (~40-60MB)
# Includes runtime, no need to install .NET
```

**3. AOT native version (Fastest startup):**
```bash
./build-win-minimal.bat  
# Output: publish-win/backend.exe (~15-25MB)
# Native code, fastest startup speed
```

**4. Linux version + Docker:**
```bash
./build-linux-minimal.bat
docker build -f Dockerfile.prebuilt-linux -t web-clipboard .
```

**5. One-click build + Docker:**
```bash
./build-and-docker.bat
```

### Linux/Mac Local Build
```bash
chmod +x build-minimal.sh
./build-minimal.sh
```

### Build Optimization Features:
- Assembly trimming
- Symbol stripping 
- Single file deployment
- Compression
- Size-optimized IL

### Expected Sizes:
- Windows AOT: ~15-20MB
- Linux (no AOT): ~25-35MB  
- Docker image: ~50-70MB

### Docker Builds

**Standard Docker:**
```bash
docker build -t web-clipboard .
docker run -p 8080:8080 web-clipboard
```

**Ultra-Minimal Docker (Smallest Image):**
```bash
docker build -f Dockerfile.minimal -t web-clipboard-minimal .
docker run -p 8080:8080 web-clipboard-minimal
```

The minimal Docker image uses:
- Distroless base image (no OS, just runtime)
- Single-file AOT executable
- Compressed frontend assets
- Full assembly trimming

Image sizes:
- Standard: ~150-200MB
- Minimal: ~50-80MB

## Security Considerations

- Content expires automatically after 10 minutes
- Text stored in memory, files in temp directory
- IDs are generated using GUIDs for uniqueness
- No authentication required - suitable for temporary sharing
- Consider adding authentication for production use with sensitive data

## Browser Compatibility

- Chrome/Chromium 80+
- Firefox 74+
- Safari 13.1+
- Edge 80+
- Mobile browsers with modern JavaScript support

## Automated Builds & Deployment

This project uses GitHub Actions for automated building and publishing:

### Available Images
- **GitHub Container Registry**:
  - `ghcr.io/[username]/web-clipboard:latest` - Standard image (~150MB)
  - `ghcr.io/[username]/web-clipboard-minimal:latest` - Minimal image (~50MB)

### Supported Architectures
- `linux/amd64` - x86_64 systems
- `linux/arm64` - ARM64 systems (Raspberry Pi 4+, Apple Silicon, etc.)

### Automated Workflows
- **CI/CD**: Automatic testing and security scanning on every commit
- **Docker Publishing**: Multi-architecture images published on push to main
- **Releases**: Binary builds and images published when creating GitHub releases

See [GITHUB-WORKFLOWS.md](GITHUB-WORKFLOWS.md) for detailed setup instructions.

## License

MIT License - feel free to use and modify as needed.

## Contributing

1. Fork the repository
2. Create a feature branch
3. Make your changes
4. Test thoroughly
5. Submit a pull request

## Support

For issues and questions:
1. Check the browser console for any errors
2. Ensure cookies/localStorage are enabled
3. Verify network connectivity to the backend
4. Check Docker logs if using containerized deployment