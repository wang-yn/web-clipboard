# 跨会话共享测试

## 测试步骤

### 1. 基本文本共享测试
1. **浏览器A** - 打开 http://localhost:5000/
2. 输入文本: "Hello from Browser A"
3. 点击 "Save Text"，记录ID (例如: `abc123`)
4. **浏览器B** - 打开 http://localhost:5000/
5. 在"Enter ID"框输入: `abc123`
6. 点击 "Load Text"
7. ✅ 应该显示 "Hello from Browser A"

### 2. 文件共享测试
1. **设备A** - 上传一个文件，获得ID
2. **设备B** - 使用相同ID下载文件
3. ✅ 应该获得完全相同的文件

### 3. 跨设备测试
1. **手机浏览器** - 保存一段文本
2. **电脑浏览器** - 使用ID加载文本
3. ✅ 内容应该完全同步

### 4. 匿名模式测试
1. **普通模式** - 保存内容
2. **匿名/隐私模式** - 加载内容
3. ✅ 匿名模式也能正常访问

## API直接测试

```bash
# 终端1 - 保存
curl -X POST http://localhost:5000/api/text \
  -H "Content-Type: application/json" \
  -d '{"content":"Cross-session test"}'
# 返回: {"id":"xxx","expiresAt":"..."}

# 终端2 - 获取 (使用上面的ID)
curl http://localhost:5000/api/text/xxx
# 返回: {"content":"Cross-session test","createdAt":"..."}
```

## 共享特性

### ✅ 支持的共享
- 不同浏览器实例
- 不同浏览器类型 
- 不同操作系统
- 不同网络环境
- 匿名/隐私模式
- 移动端和桌面端

### ⏰ 有效期限制
- 所有内容24小时后自动过期
- 过期后任何会话都无法访问
- 可通过 `/api/cleanup` 手动清理

### 🔒 安全考虑
- 无身份验证 - 任何人知道ID即可访问
- ID使用GUID - 难以猜测但不加密
- 内存存储 - 服务器重启后丢失
- 适合临时文件/文本分享