# Web Clipboard - 网页剪贴板

现代化的基于网页的剪贴板应用程序，允许您跨设备和浏览器存储和共享文本内容和文件。

## 功能特点

- 📝 **文本剪贴板**：使用4字符ID（如A1B2）存储和共享文本内容
- 📁 **文件上传/下载**：上传文件并使用短ID下载
- 📱 **移动端友好**：针对移动设备和桌面设备优化的响应式设计
- ⌨️ **输入便捷**：4字符短ID易于输入和分享
- ⚡ **高性能**：基于.NET 9 AOT构建，资源占用极少
- 🐳 **Docker就绪**：支持Docker和Docker Compose轻松部署
- 🔒 **快速过期**：内容10分钟后过期，提高内存效率
- 💾 **智能存储**：文件存储在临时目录中，而不是内存
- 🛡️ **安全保护**：限速、暴力破解保护、内容验证
- 📊 **访问监控**：全面的日志记录和安全事件跟踪
- 🌐 **Nginx代理**：反向代理支持，负载均衡和SSL终止
- 🔒 **生产就绪**：HTTPS、安全头和企业级部署选项

## 快速开始

### 使用Docker Compose（推荐）

```bash
# 克隆或下载项目
git clone <repository-url>
cd web-clipboard

# 选项1：简单部署
docker-compose up -d
# 访问 http://localhost:8080

# 选项2：使用Nginx代理（生产环境）
./start-with-nginx.bat  # Windows
./start-with-nginx.sh   # Linux/Mac
# 访问 http://localhost
```

### 手动设置

#### 前置要求
- .NET 9 SDK
- 现代网页浏览器

#### 运行应用程序

```bash
# 进入backend目录
cd backend

# 恢复依赖
dotnet restore

# 运行应用程序
dotnet run

# 访问应用程序
# 在浏览器中打开 http://localhost:5000
```

## 使用方法

### 文本剪贴板
1. 在文本区域输入文本
2. 点击"保存文本"存储并获取唯一ID
3. 与他人分享ID或在不同设备上使用
4. 输入ID并点击"加载文本"检索存储的文本
5. 使用"复制文本"将内容复制到系统剪贴板

### 文件剪贴板
1. 点击"选择文件"或拖放文件
2. 点击"上传文件"存储并获取唯一ID
3. 与他人分享ID或在不同设备上使用
4. 输入ID并点击"下载文件"检索文件

### 最近项目
- 查看最近创建的项目，提供快速访问按钮
- 将ID复制到剪贴板或直接加载项目
- 项目在本地存储，过期时自动清理

## API端点

- `POST /api/text` - 存储文本内容
- `GET /api/text/{id}` - 检索文本内容
- `POST /api/file` - 上传文件
- `GET /api/file/{id}` - 下载文件
- `DELETE /api/{id}` - 删除项目
- `GET /api/cleanup` - 清理过期项目

## 技术详情

### 后端
- .NET 9，采用AOT（提前编译）实现最佳性能
- 最小API设计，减少开销
- 内存存储，自动过期（24小时）
- 启用CORS支持跨域请求
- 静态文件服务用于前端

### 前端
- 原生JavaScript，最小化包大小
- Tailwind CSS响应式设计
- 本地存储用于最近项目跟踪
- 拖放文件上传支持
- 移动端优化界面

### Docker部署
- 多阶段Docker构建，优化镜像大小
- Alpine Linux基础镜像，最小运行时
- Docker Compose易于编排
- 可配置端口映射（默认：8080）

## 配置

### 环境变量
- `ASPNETCORE_URLS`：配置监听URL（默认：http://+:8080）
- `ASPNETCORE_ENVIRONMENT`：设置环境（Development/Production）

### 自定义
- 在`Program.cs`中修改过期时间（默认：24小时）
- 在后端配置中调整文件大小限制
- 在`index.html`和Tailwind类中自定义UI样式

## 安全测试

### 运行安全测试
```bash
# 测试所有安全功能
./test-security.bat

# 查看安全日志
tail -f backend/security.log
```

### 安全功能
- **限速**：每个IP每分钟20次上传，100次下载
- **文件类型阻止**：危险扩展名（.exe、.bat、.js等）被阻止
- **内容验证**：XSS和注入模式检测
- **暴力破解保护**：20次失败尝试后自动阻止IP
- **大小限制**：文本最大1MB，文件最大50MB
- **安全头**：XSS保护、框架选项、内容嗅探防护

详细安全文档请参见[SECURITY.md](SECURITY.md)。

## Nginx代理部署

### 使用Nginx快速开始
```bash
# 使用Nginx代理启动
./start-with-nginx.bat  # Windows
./start-with-nginx.sh   # Linux/Mac

# 测试代理功能
./test-nginx-proxy.bat

# 访问应用程序
http://localhost        # 通过Nginx代理
http://localhost:5000   # 直接后端访问
```

### 功能
- **反向代理**：负载均衡和故障转移支持
- **限速**：每个IP每秒10个API请求，5次上传
- **SSL终止**：支持现代TLS的HTTPS
- **安全头**：XSS保护、框架选项、内容嗅探防护
- **缓存**：静态文件缓存，提高性能
- **健康检查**：自动后端健康监控

### 配置文件
- `nginx/nginx.conf` - 完整生产配置
- `nginx/nginx-dev.conf` - 开发环境
- `nginx/nginx-docker.conf` - Docker环境
- `nginx/ssl-example.conf` - HTTPS配置模板

详细部署指南请参见[NGINX-DEPLOYMENT.md](NGINX-DEPLOYMENT.md)。

## 开发

### Windows本地编译方案（推荐）

多种构建选项，满足不同需求：

**1. 框架依赖版本（最小体积）:**
```bash
./build-simple.bat
# 或使用 PowerShell: ./build-simple.ps1
# 输出: publish-simple/ (~2-5MB)
# 需要目标机器安装 .NET 9 运行时
```

**2. 优化自包含版本（平衡）:**
```bash  
./build-optimized.bat
# 输出: publish-optimized/backend.exe (~40-60MB)
# 包含运行时，无需安装 .NET
```

**3. AOT原生版本（最快启动）:**
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
- Assembly trimming（程序集裁剪）
- Symbol stripping（符号剥离） 
- Single file deployment（单文件部署）
- Compression（压缩）
- Size-optimized IL（尺寸优化）

### 预期尺寸:
- Windows AOT: ~15-20MB
- Linux (no AOT): ~25-35MB  
- Docker镜像: ~50-70MB

### 生产构建
```bash
cd backend
dotnet publish -c Release -o ../publish
```

### Docker构建

**标准Docker:**
```bash
docker build -t web-clipboard .
docker run -p 8080:8080 web-clipboard
```

**超小Docker（最小镜像）:**
```bash
docker build -f Dockerfile.minimal -t web-clipboard-minimal .
docker run -p 8080:8080 web-clipboard-minimal
```

最小Docker镜像使用：
- Distroless基础镜像（无操作系统，仅运行时）
- 单文件AOT可执行文件
- 压缩前端资源
- 完整程序集裁剪

镜像大小：
- 标准：~150-200MB
- 最小：~50-80MB

## 安全考虑

- 内容10分钟后自动过期
- 文本存储在内存中，文件存储在临时目录
- 使用GUID生成ID以确保唯一性
- 无需身份验证 - 适用于临时分享
- 对于包含敏感数据的生产使用，建议添加身份验证

## 浏览器兼容性

- Chrome/Chromium 80+
- Firefox 74+
- Safari 13.1+
- Edge 80+
- 支持现代JavaScript的移动浏览器

## 许可证

MIT许可证 - 可自由使用和修改。

## 贡献

1. Fork仓库
2. 创建功能分支
3. 进行更改
4. 全面测试
5. 提交pull request

## 支持

如有问题和疑问：
1. 检查浏览器控制台是否有错误
2. 确保启用cookies/localStorage
3. 验证与后端的网络连接
4. 如使用容器化部署，检查Docker日志