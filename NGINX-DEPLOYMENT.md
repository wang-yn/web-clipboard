# Nginx 代理部署指南

## 🌐 概述

本文档详细说明如何使用Nginx作为Web剪贴板应用的反向代理，提供更好的性能、安全性和可扩展性。

## 📋 目录结构

```
web-clipboard/
├── nginx/
│   ├── nginx.conf              # 完整生产配置
│   ├── nginx-dev.conf          # 开发环境简化配置
│   ├── nginx-docker.conf       # Docker环境配置
│   ├── ssl-example.conf        # HTTPS配置示例
│   └── logs/                   # 日志目录
├── docker-compose.nginx.yml    # 包含Nginx的Docker Compose
├── start-with-nginx.bat/.sh    # 一键启动脚本
└── test-nginx-proxy.bat        # 代理功能测试
```

## 🚀 快速开始

### 方法1: Docker Compose (推荐)

```bash
# Windows
./start-with-nginx.bat

# Linux/Mac
chmod +x start-with-nginx.sh
./start-with-nginx.sh

# 手动启动
docker-compose -f docker-compose.nginx.yml up -d
```

### 方法2: 本地Nginx

```bash
# 1. 启动应用
cd backend
dotnet run

# 2. 安装并配置Nginx
# Ubuntu/Debian
sudo apt update
sudo apt install nginx

# CentOS/RHEL
sudo yum install nginx

# 3. 使用提供的配置文件
sudo cp nginx/nginx-dev.conf /etc/nginx/nginx.conf
sudo nginx -t
sudo systemctl restart nginx
```

## ⚙️ 配置详解

### 🔧 基础配置 (nginx-dev.conf)

适用于开发和测试环境的简化配置：

```nginx
upstream backend {
    server 127.0.0.1:5000;  # .NET应用地址
}

server {
    listen 8080;
    server_name localhost;
    
    location / {
        proxy_pass http://backend;
        proxy_set_header Host $host;
        proxy_set_header X-Real-IP $remote_addr;
        proxy_set_header X-Forwarded-For $proxy_add_x_forwarded_for;
        proxy_set_header X-Forwarded-Proto $scheme;
    }
}
```

### 🛡️ 生产配置 (nginx.conf)

包含完整的安全和性能优化：

```nginx
# 频率限制
limit_req_zone $binary_remote_addr zone=api_limit:10m rate=10r/s;
limit_req_zone $binary_remote_addr zone=upload_limit:10m rate=5r/s;

# 安全标头
add_header X-Frame-Options DENY always;
add_header X-Content-Type-Options nosniff always;
add_header X-XSS-Protection "1; mode=block" always;

# 负载均衡
upstream web_clipboard_backend {
    server 127.0.0.1:5000 max_fails=3 fail_timeout=30s;
    keepalive 32;
}
```

### 🔒 HTTPS配置

```nginx
server {
    listen 443 ssl http2;
    server_name yourdomain.com;
    
    ssl_certificate /path/to/cert.pem;
    ssl_certificate_key /path/to/key.pem;
    ssl_protocols TLSv1.2 TLSv1.3;
    
    # HSTS
    add_header Strict-Transport-Security "max-age=63072000" always;
    
    location / {
        proxy_pass http://web_clipboard_backend;
        proxy_set_header X-Forwarded-Proto https;
        # ... 其他代理配置
    }
}
```

## 🎯 功能特性

### 🚦 速率限制

| 端点类型 | 限制 | 突发 |
|---------|------|------|
| 普通API | 10请求/秒 | 20 |
| 文件上传 | 5请求/秒 | 10 |
| 文件下载 | 20请求/秒 | 50 |

### 🛡️ 安全防护

- **DDoS防护**: 连接和请求频率限制
- **安全标头**: 防XSS、点击劫持等
- **访问控制**: 阻止敏感文件访问
- **SSL/TLS**: 现代加密协议支持

### ⚡ 性能优化

- **负载均衡**: 多后端实例支持
- **连接复用**: Keepalive连接池
- **缓存优化**: 静态资源缓存
- **压缩传输**: Gzip压缩

## 🐳 Docker部署

### 服务架构

```
┌─────────────┐    ┌─────────────┐    ┌─────────────┐
│   客户端     │───▶│    Nginx    │───▶│  .NET App   │
│  浏览器      │    │   (Port 80) │    │ (Port 5000) │
└─────────────┘    └─────────────┘    └─────────────┘
```

### Docker Compose 配置

```yaml
services:
  web-clipboard:
    build: .
    ports:
      - "5000:5000"  # 内部端口
    networks:
      - web-clipboard-network

  nginx:
    image: nginx:alpine
    ports:
      - "80:80"      # HTTP
      - "443:443"    # HTTPS
    volumes:
      - ./nginx/nginx-docker.conf:/etc/nginx/nginx.conf:ro
    depends_on:
      - web-clipboard
```

## 🧪 测试验证

### 自动化测试

```bash
# 运行完整代理测试
./test-nginx-proxy.bat

# 测试项目包括:
✅ HTTP访问测试
✅ API端点代理
✅ 文件上传代理
✅ 代理标头转发
✅ 速率限制测试
✅ 健康检查
✅ 配置验证
```

### 手动测试

```bash
# 1. 基础连通性测试
curl -I http://localhost/

# 2. API功能测试
curl -X POST http://localhost/api/text \
  -H "Content-Type: application/json" \
  -d '{"content":"proxy test"}'

# 3. 文件上传测试
curl -X POST http://localhost/api/file \
  -F "file=@test.txt"

# 4. 速率限制测试
for i in {1..15}; do
  curl -s -o /dev/null -w "Status: %{http_code}\n" \
    http://localhost/api/text/test$i
done
```

## 🔧 故障排除

### 常见问题

**1. 502 Bad Gateway**
```bash
# 检查后端服务状态
docker-compose logs web-clipboard

# 检查网络连接
docker-compose exec nginx ping web-clipboard
```

**2. 413 Request Entity Too Large**
```nginx
# 增加文件大小限制
client_max_body_size 100M;
```

**3. 504 Gateway Timeout**
```nginx
# 增加超时设置
proxy_connect_timeout 300s;
proxy_send_timeout 300s;
proxy_read_timeout 300s;
```

### 日志分析

```bash
# Nginx访问日志
docker-compose logs nginx

# 应用程序日志
docker-compose logs web-clipboard

# 实时监控
docker-compose logs -f nginx
```

## 📊 监控指标

### 关键指标

- **响应时间**: 平均 < 100ms
- **错误率**: < 1%
- **吞吐量**: 支持数百并发用户
- **可用性**: > 99.9%

### 监控命令

```bash
# Nginx状态
curl http://localhost/health

# 服务状态
docker-compose ps

# 资源使用
docker stats
```

## 🌟 生产部署建议

### SSL证书

```bash
# 使用Let's Encrypt免费证书
sudo apt install certbot python3-certbot-nginx
sudo certbot --nginx -d yourdomain.com

# 自动续期
sudo crontab -e
# 添加: 0 12 * * * /usr/bin/certbot renew --quiet
```

### 安全加固

```nginx
# 1. 限制代理来源
set_real_ip_from 10.0.0.0/8;
set_real_ip_from 172.16.0.0/12;
set_real_ip_from 192.168.0.0/16;

# 2. 隐藏服务器信息
server_tokens off;

# 3. 限制请求方法
if ($request_method !~ ^(GET|HEAD|POST|DELETE)$ ) {
    return 405;
}
```

### 性能调优

```nginx
# 1. 工作进程优化
worker_processes auto;
worker_connections 2048;

# 2. 缓冲区优化
proxy_buffering on;
proxy_buffer_size 128k;
proxy_buffers 4 256k;

# 3. 压缩配置
gzip on;
gzip_vary on;
gzip_min_length 1024;
gzip_types text/plain application/json;
```

## 📋 检查清单

### 部署前检查

- [ ] 域名DNS配置正确
- [ ] SSL证书有效
- [ ] 防火墙端口开放 (80, 443)
- [ ] 后端服务健康
- [ ] Nginx配置语法正确

### 部署后验证

- [ ] HTTP重定向到HTTPS工作
- [ ] 所有API端点可访问
- [ ] 文件上传下载正常
- [ ] 安全标头设置正确
- [ ] 速率限制生效
- [ ] 日志记录正常

通过Nginx代理，Web剪贴板获得了企业级的可靠性和性能！