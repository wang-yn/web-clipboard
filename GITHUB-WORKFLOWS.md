# GitHub Workflows 使用指南

本项目包含多个 GitHub Actions 工作流，用于自动构建、测试和发布 Docker 镜像。

## 工作流概述

### 1. CI/CD Pipeline (`ci.yml`)
- **触发条件**: 推送到 `main`/`develop` 分支或创建 Pull Request
- **功能**:
  - .NET 项目构建和测试
  - Docker 镜像构建测试
  - 安全漏洞扫描（Trivy）
  - 基本功能测试

### 2. Docker 镜像发布 (`docker-publish.yml`)
- **触发条件**: 
  - 推送到 `main`/`develop` 分支
  - 创建标签 `v*.*.*`
  - Pull Request
  - 手动触发
- **功能**:
  - 构建标准镜像和最小化镜像
  - 多架构支持 (linux/amd64, linux/arm64)
  - 发布到 GitHub Container Registry (ghcr.io)
  - 生成构建证明（attestation）

### 3. 发布工作流 (`release.yml`)
- **触发条件**:
  - GitHub Release 发布时
  - 手动触发（可指定标签）
- **功能**:
  - 构建多平台二进制文件
  - 构建多架构 Docker 镜像
  - 上传发布资产到 GitHub Releases
  - 同时发布到 Docker Hub（可选）

## 快速开始

### 1. 设置仓库

确保你的 GitHub 仓库已启用以下权限：
- Actions: 读写权限
- Packages: 写权限
- Contents: 读权限

### 2. Docker Hub 发布（可选）

如需发布到 Docker Hub，需设置以下 Secrets：
- `DOCKERHUB_USERNAME`: Docker Hub 用户名
- `DOCKERHUB_TOKEN`: Docker Hub 访问令牌

### 3. 自动触发发布

#### 方法一：创建 Git 标签
```bash
# 创建并推送标签
git tag v1.0.0
git push origin v1.0.0
```

#### 方法二：创建 GitHub Release
1. 在 GitHub 仓库页面点击 "Releases"
2. 点击 "Create a new release"
3. 输入标签名称（如 `v1.0.0`）
4. 填写发布说明
5. 点击 "Publish release"

#### 方法三：手动触发
1. 进入 Actions 页面
2. 选择对应的工作流
3. 点击 "Run workflow"
4. 输入参数并运行

## 发布的镜像

### GitHub Container Registry
- **标准镜像**: `ghcr.io/[username]/web-clipboard:latest`
- **最小化镜像**: `ghcr.io/[username]/web-clipboard-minimal:latest`

### Docker Hub (可选)
- **标准镜像**: `[username]/web-clipboard:latest`
- **最小化镜像**: `[username]/web-clipboard-minimal:latest`

## 使用发布的镜像

### 从 GitHub Container Registry 拉取
```bash
# 标准镜像
docker pull ghcr.io/[username]/web-clipboard:latest

# 最小化镜像
docker pull ghcr.io/[username]/web-clipboard-minimal:latest
```

### 运行容器
```bash
# 使用标准镜像
docker run -p 8080:8080 ghcr.io/[username]/web-clipboard:latest

# 使用最小化镜像
docker run -p 8080:8080 ghcr.io/[username]/web-clipboard-minimal:latest
```

## 支持的平台

### Docker 镜像
- `linux/amd64` - x86_64 架构
- `linux/arm64` - ARM64 架构

### 二进制文件 (Release 时)
- **Linux**: x64, ARM64
- **Windows**: x64
- **macOS**: x64, ARM64 (Apple Silicon)

## 镜像变体

### 标准镜像 (`Dockerfile`)
- 基于 Microsoft .NET Runtime 镜像
- 包含完整运行时
- 体积约 150-200MB
- 适用于生产环境

### 最小化镜像 (`Dockerfile.minimal`)
- 基于 Distroless 镜像
- 使用 AOT 编译
- 体积约 50-80MB
- 更高的安全性

## 高级配置

### 自定义构建参数

编辑 `.github/workflows/docker-publish.yml` 文件：

```yaml
- name: Build and push Docker image
  uses: docker/build-push-action@v5
  with:
    build-args: |
      CUSTOM_ARG=value
```

### 添加新的镜像变体

在 `strategy.matrix` 中添加新的配置：

```yaml
strategy:
  matrix:
    include:
      - dockerfile: Dockerfile.custom
        image-suffix: "-custom"
        platforms: linux/amd64
```

### 启用 Docker Buildx 缓存

工作流已配置 GitHub Actions 缓存：
- `cache-from: type=gha` - 从缓存读取
- `cache-to: type=gha,mode=max` - 写入缓存

## 故障排除

### 常见问题

1. **权限错误**
   - 检查仓库的 Actions 和 Packages 权限
   - 确保 GITHUB_TOKEN 有足够权限

2. **Docker Hub 推送失败**
   - 验证 DOCKERHUB_USERNAME 和 DOCKERHUB_TOKEN
   - 确保 Docker Hub 仓库存在

3. **多架构构建失败**
   - 检查 QEMU 和 Buildx 设置
   - 确保 Dockerfile 支持目标架构

### 查看构建日志

1. 进入 GitHub 仓库的 Actions 页面
2. 选择失败的工作流运行
3. 点击具体的作业查看详细日志

### 测试本地构建

```bash
# 测试标准镜像构建
docker build -t web-clipboard:local .

# 测试最小化镜像构建
docker build -t web-clipboard:minimal-local -f Dockerfile.minimal .

# 测试多架构构建
docker buildx build --platform linux/amd64,linux/arm64 -t web-clipboard:multiarch .
```

## 安全最佳实践

1. **使用最新的 Action 版本**
   - 定期更新 GitHub Actions
   - 使用特定版本而非 `@main`

2. **镜像扫描**
   - CI 工作流包含 Trivy 安全扫描
   - 定期检查安全报告

3. **最小权限原则**
   - 只授予必要的权限
   - 定期审查 Secret 和权限

4. **签名验证**
   - 工作流生成构建证明
   - 支持供应链安全验证

## 相关文档

- [Docker 部署指南](README.md#docker-builds)
- [Nginx 代理部署](NGINX-DEPLOYMENT.md)
- [安全配置](SECURITY.md)
- [内存优化](MEMORY-OPTIMIZATION.md)