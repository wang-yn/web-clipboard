# Web Clipboard

A modern web-based clipboard application that allows you to store and share text content and files across devices and browsers.

## Features

- 📝 **Text Clipboard**: Store and share text content with 4-char IDs (e.g. A1B2)
- 📁 **File Upload/Download**: Upload files and download them using short IDs
- 📱 **Mobile-Friendly**: Optimized responsive design for mobile and desktop
- ⌨️ **Easy Input**: Short 4-character IDs easy to type and share
- ⚡ **High Performance**: Built with .NET 9 AOT for minimal resource usage
- 🐳 **Docker Ready**: Easy deployment with Docker and Docker Compose
- 🔒 **Automatic Cleanup**: Content expires after 24 hours

## Quick Start

### Using Docker Compose (Recommended)

```bash
# Clone or download the project
git clone <repository-url>
cd web-clipboard

# Start the application
docker-compose up -d

# Access the application
# Open http://localhost:8080 in your browser
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

## Development

### Building for Production
```bash
cd backend
dotnet publish -c Release -o ../publish
```

### Windows本地编译方案 (推荐)

多种构建选项，满足不同需求：

**1. 框架依赖版本 (最小体积):**
```bash
./build-simple.bat
# 或使用 PowerShell: ./build-simple.ps1
# 输出: publish-simple/ (~2-5MB)
# 需要目标机器安装 .NET 9 运行时
```

**2. 优化自包含版本 (平衡):**
```bash  
./build-optimized.bat
# 输出: publish-optimized/backend.exe (~40-60MB)
# 包含运行时，无需安装 .NET
```

**3. AOT原生版本 (最快启动):**
```bash
./build-win-minimal.bat  
# 输出: publish-win/backend.exe (~15-25MB)
# 原生代码，最快启动速度
```

**4. Linux版本 + Docker:**
```bash
./build-linux-minimal.bat
docker build -f Dockerfile.prebuilt-linux -t web-clipboard .
```

**5. 一键编译+Docker:**
```bash
./build-and-docker.bat
```

### Linux/Mac本地编译
```bash
chmod +x build-minimal.sh
./build-minimal.sh
```

### 编译优化特性:
- Assembly trimming (程序集裁剪)
- Symbol stripping (符号剥离) 
- Single file deployment (单文件部署)
- Compression (压缩)
- Size-optimized IL (尺寸优化)

### 预期尺寸:
- Windows AOT: ~15-20MB
- Linux (no AOT): ~25-35MB  
- Docker镜像: ~50-70MB

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

- Content expires automatically after 24 hours
- No persistent storage - all data is in memory
- IDs are generated using GUIDs for uniqueness
- No authentication required - suitable for temporary sharing
- Consider adding authentication for production use with sensitive data

## Browser Compatibility

- Chrome/Chromium 80+
- Firefox 74+
- Safari 13.1+
- Edge 80+
- Mobile browsers with modern JavaScript support

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