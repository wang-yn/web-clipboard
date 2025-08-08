// 示例：如何添加会话隔离（可选功能）

// 在 Program.cs 中添加会话支持
builder.Services.AddSession();

// 修改存储结构
var clipboardData = new ConcurrentDictionary<string, Dictionary<string, ClipboardItem>>();

// 修改保存文本的端点
app.MapPost("/api/text", (TextRequest request, HttpContext context) =>
{
    var sessionId = context.Session.Id;
    
    if (!clipboardData.ContainsKey(sessionId))
        clipboardData[sessionId] = new Dictionary<string, ClipboardItem>();
    
    var id = Guid.NewGuid().ToString();
    var item = new ClipboardItem
    {
        Id = id,
        Type = "text", 
        Content = request.Content,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddHours(24)
    };
    
    clipboardData[sessionId][id] = item;
    return Results.Ok(new SaveTextResponse(id, item.ExpiresAt));
});

// 修改获取文本的端点
app.MapGet("/api/text/{id}", (string id, HttpContext context) =>
{
    var sessionId = context.Session.Id;
    
    if (clipboardData.TryGetValue(sessionId, out var userClipboard) &&
        userClipboard.TryGetValue(id, out var item) && 
        item.Type == "text" && 
        item.ExpiresAt > DateTime.UtcNow)
    {
        return Results.Ok(new GetTextResponse(item.Content!, item.CreatedAt));
    }
    return Results.NotFound();
});

/* 
这样修改后：
- 每个浏览器会话有独立的剪贴板空间
- 不同浏览器无法访问彼此的内容
- 同一浏览器的不同标签页共享内容
*/