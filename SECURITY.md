# Web Clipboard 安全性说明

## 🛡️ 安全特性概览

### 🚦 **访问控制**
- **频率限制**: 每IP每分钟限制请求数量
- **暴力破解防护**: 失败尝试过多自动封IP
- **实时监控**: 所有访问尝试都被记录

### 🔒 **内容验证**
- **文件类型检查**: 阻止危险文件扩展名
- **内容模式检测**: 识别并阻止恶意内容
- **大小限制**: 防止资源耗尽攻击

### 🌐 **网络安全**
- **安全标头**: 防止XSS、点击劫持等攻击
- **CORS限制**: 限制跨域访问源
- **IP追踪**: 支持代理环境的真实IP获取

## 📊 **安全限制详情**

### ⚡ **频率限制**
| 操作类型 | 限制 | 时间窗口 |
|---------|------|----------|
| 文件上传 (POST) | 20次 | 每分钟 |
| 内容下载 (GET) | 100次 | 每分钟 |
| 其他操作 | 50次 | 每分钟 |

### 📁 **文件类型限制**
**被阻止的扩展名**:
```
.exe, .bat, .cmd, .com, .pif, .scr, .vbs, .js, .jar, 
.ps1, .sh, .msi, .dll, .sys, .php, .asp, .aspx, .jsp
```

### 📏 **内容大小限制**
- **文本内容**: 最大 1MB
- **文件上传**: 最大 50MB
- **超出限制**: 自动拒绝并记录

### 🕵️ **恶意内容检测**
**检测模式**:
```
<script, javascript:, data:text/html, eval(, document.write,
base64,, php://, file://, ftp://, ../../
```

## 🔧 **安全机制实现**

### 🛑 **IP封锁策略**
```csharp
// 封锁触发条件
- 20次失败的内容访问尝试  
- 50次暴力破解尝试
- 大内容或恶意模式攻击

// 封锁时长
- 失败尝试记录保留1小时
- IP封锁立即生效
- 服务重启后清除封锁
```

### 📋 **访问日志格式**
```
[2025-08-08T16:30:00Z] 192.168.1.100 accessed text A1B2: SUCCESS
[2025-08-08T16:30:05Z] 192.168.1.101 accessed file X9Y8: FAILED
[2025-08-08T16:30:10Z] 192.168.1.102 blocked after 21 failed attempts
```

### 🔐 **安全标头**
```http
X-Content-Type-Options: nosniff
X-Frame-Options: DENY  
X-XSS-Protection: 1; mode=block
Referrer-Policy: strict-origin-when-cross-origin
Content-Security-Policy: default-src 'self' 'unsafe-inline' cdn.tailwindcss.com
```

## 🎯 **攻击防护矩阵**

| 攻击类型 | 防护措施 | 检测方式 | 响应动作 |
|---------|---------|---------|---------|
| **暴力破解** | 失败计数 | 连续失败访问 | IP封锁 |
| **DDoS** | 频率限制 | 请求速率监控 | 429错误 |
| **文件上传攻击** | 扩展名过滤 | 文件类型检查 | 400错误 |
| **XSS注入** | 内容模式检测 | 特征字符串匹配 | 400错误 |
| **点击劫持** | Frame阻止 | HTTP标头 | 页面保护 |
| **内容嗅探** | MIME保护 | HTTP标头 | 类型强制 |

## 🧪 **安全测试**

### 📝 **手动测试**
```bash
# 测试频率限制
for i in {1..25}; do curl http://localhost:5000/api/text/test$i; done

# 测试文件类型限制  
curl -X POST http://localhost:5000/api/file -F "file=@malware.exe"

# 测试内容大小限制
curl -X POST http://localhost:5000/api/text -d '{"content":"'$(head -c 2000000 /dev/zero)'"}'

# 测试XSS检测
curl -X POST http://localhost:5000/api/text -d '{"content":"<script>alert()</script>"}'
```

### 🤖 **自动化测试**
```bash
# 运行完整安全测试套件
./test-security.bat

# 检查安全日志
tail -f security.log
```

## ⚠️ **安全建议**

### 🔧 **生产环境配置**
1. **HTTPS强制**: 在生产环境启用HTTPS
2. **防火墙配置**: 限制不必要的端口访问
3. **日志监控**: 设置日志收集和告警
4. **定期更新**: 保持系统和依赖项最新

### 📊 **监控指标**
- **失败请求率**: 监控异常访问模式
- **IP封锁频率**: 识别攻击趋势  
- **大文件上传**: 监控资源使用
- **响应时间**: 检测性能异常

### 🛡️ **额外安全层**
```bash
# 建议在前端添加
- WAF (Web Application Firewall)
- CDN保护 (如 Cloudflare)
- 负载均衡器安全规则
- 网络层DDoS防护
```

## 🚨 **事件响应**

### 📋 **安全事件处理**
1. **检测**: 自动检测和记录异常行为
2. **响应**: 立即封锁恶意IP地址
3. **记录**: 详细日志记录所有安全事件
4. **清理**: 定期清理过期的安全记录

### 🔍 **日志分析**
```bash
# 查找暴力破解尝试
grep "excessive failed attempts" security.log

# 统计被阻止的文件类型
grep "File type not allowed" security.log | wc -l

# 分析访问模式
grep "accessed" security.log | cut -d' ' -f2 | sort | uniq -c
```

## 🎖️ **安全等级评估**

| 安全级别 | 当前状态 | 说明 |
|---------|---------|------|
| **基础防护** | ✅ 已实现 | 频率限制、文件过滤 |
| **内容安全** | ✅ 已实现 | XSS防护、大小限制 |
| **访问控制** | ✅ 已实现 | IP封锁、暴力破解防护 |
| **监控日志** | ✅ 已实现 | 访问记录、安全事件 |
| **传输安全** | ⚠️ 建议 | HTTPS配置 |
| **身份认证** | ⚠️ 可选 | 用户账户系统 |

Web剪贴板现已具备生产级安全防护能力！