# Web Clipboard 测试指南

## 快速测试

1. **启动应用**
   ```bash
   cd backend
   dotnet run
   ```
   
2. **访问地址**
   - 主页: http://localhost:5000/
   - 直接文件: http://localhost:5000/index.html
   - API测试: http://localhost:5000/api/cleanup

## 验证步骤

### 1. 静态文件服务
- ✅ 访问 http://localhost:5000/ 应显示剪贴板界面
- ✅ 访问 http://localhost:5000/index.html 应显示相同界面
- ✅ 访问 http://localhost:5000/app.js 应返回JavaScript文件

### 2. 文本功能测试
1. 在文本框输入内容
2. 点击"Save Text"
3. 应显示成功消息和ID
4. 复制ID到"Enter ID"框
5. 点击"Load Text"
6. 应显示原始文本

### 3. 文件功能测试
1. 点击"Select File"选择文件
2. 点击"Upload File"
3. 应显示成功消息和ID
4. 复制ID到文件ID框
5. 点击"Download File"
6. 应下载原始文件

### 4. API直接测试

**保存文本:**
```bash
curl -X POST http://localhost:5000/api/text \
  -H "Content-Type: application/json" \
  -d '{"content":"Hello World"}'
```

**获取文本:**
```bash
curl http://localhost:5000/api/text/[ID]
```

**清理过期项:**
```bash
curl http://localhost:5000/api/cleanup
```

## 常见问题

### 404错误
- 确保在`backend`文件夹运行`dotnet run`
- 检查`wwwroot`文件夹是否存在`index.html`
- 访问 http://localhost:5000/ (不是localhost:5000/index.html)

### CORS错误
- 应用已配置允许所有来源的请求
- 如果仍有问题，检查浏览器控制台

### JSON序列化错误
- AOT版本已修复JSON序列化问题
- 使用源代码生成器支持所有类型

## 性能测试

```bash
# 并发测试
for i in {1..10}; do
  curl -X POST http://localhost:5000/api/text \
    -H "Content-Type: application/json" \
    -d "{\"content\":\"Test $i\"}" &
done
wait
```

## 构建测试

```bash
# 测试所有构建版本
./test-build.bat

# 测试特定版本
./build-simple.bat
./build-optimized.bat  
./build-win-minimal.bat
```