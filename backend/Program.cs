using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;

var builder = WebApplication.CreateBuilder(args);

// Configure JSON options for AOT compatibility
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        policy.AllowAnyOrigin()
              .AllowAnyMethod()
              .AllowAnyHeader();
    });
});

var app = builder.Build();

app.UseCors();
app.UseDefaultFiles(); // Must be before UseStaticFiles
app.UseStaticFiles();

var clipboardData = new ConcurrentDictionary<string, ClipboardItem>();

// Generate short alphanumeric ID
static string GenerateShortId(ConcurrentDictionary<string, ClipboardItem> existingData)
{
    const string chars = "ABCDEFGHIJKLMNOPQRSTUVWXYZ0123456789";
    var random = new Random();
    
    for (int attempt = 0; attempt < 100; attempt++)
    {
        var id = new string(Enumerable.Range(0, 4)
            .Select(_ => chars[random.Next(chars.Length)])
            .ToArray());
            
        if (!existingData.ContainsKey(id))
            return id;
    }
    
    // Fallback to longer ID if all 4-char combinations are taken
    return new string(Enumerable.Range(0, 6)
        .Select(_ => chars[random.Next(chars.Length)])
        .ToArray());
}

app.MapPost("/api/text", (TextRequest request) =>
{
    var id = GenerateShortId(clipboardData);
    var item = new ClipboardItem
    {
        Id = id,
        Type = "text",
        Content = request.Content,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddHours(24)
    };
    
    clipboardData[id] = item;
    return Results.Ok(new SaveTextResponse(id, item.ExpiresAt));
});

app.MapGet("/api/text/{id}", (string id) =>
{
    if (clipboardData.TryGetValue(id, out var item) && 
        item.Type == "text" && 
        item.ExpiresAt > DateTime.UtcNow)
    {
        return Results.Ok(new GetTextResponse(item.Content!, item.CreatedAt));
    }
    return Results.NotFound();
});

app.MapPost("/api/file", async (HttpRequest request) =>
{
    var form = await request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();
    
    if (file == null)
        return Results.BadRequest("No file uploaded");
    
    var id = GenerateShortId(clipboardData);
    using var memoryStream = new MemoryStream();
    await file.CopyToAsync(memoryStream);
    
    var item = new ClipboardItem
    {
        Id = id,
        Type = "file",
        FileName = file.FileName,
        FileData = memoryStream.ToArray(),
        ContentType = file.ContentType,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddHours(24)
    };
    
    clipboardData[id] = item;
    return Results.Ok(new SaveFileResponse(id, file.FileName, item.ExpiresAt));
});

app.MapGet("/api/file/{id}", (string id) =>
{
    if (clipboardData.TryGetValue(id, out var item) && 
        item.Type == "file" && 
        item.ExpiresAt > DateTime.UtcNow)
    {
        return Results.File(item.FileData!, item.ContentType, item.FileName);
    }
    return Results.NotFound();
});

app.MapDelete("/api/{id}", (string id) =>
{
    clipboardData.TryRemove(id, out _);
    return Results.Ok();
});

app.MapGet("/api/cleanup", () =>
{
    var expiredKeys = clipboardData
        .Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
        .Select(kvp => kvp.Key)
        .ToList();
    
    foreach (var key in expiredKeys)
    {
        clipboardData.TryRemove(key, out _);
    }
    
    return Results.Ok(new CleanupResponse(expiredKeys.Count));
});

app.MapFallbackToFile("index.html");

app.Run();

public record TextRequest(string Content);

public record SaveTextResponse(string Id, DateTime ExpiresAt);
public record GetTextResponse(string Content, DateTime CreatedAt);
public record SaveFileResponse(string Id, string FileName, DateTime ExpiresAt);
public record CleanupResponse(int RemovedCount);

public class ClipboardItem
{
    public string Id { get; set; } = string.Empty;
    public string Type { get; set; } = string.Empty;
    public string? Content { get; set; }
    public string? FileName { get; set; }
    public byte[]? FileData { get; set; }
    public string? ContentType { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime ExpiresAt { get; set; }
}

// JSON serialization context for AOT compatibility
[JsonSerializable(typeof(TextRequest))]
[JsonSerializable(typeof(SaveTextResponse))]
[JsonSerializable(typeof(GetTextResponse))]
[JsonSerializable(typeof(SaveFileResponse))]
[JsonSerializable(typeof(CleanupResponse))]
[JsonSerializable(typeof(ClipboardItem))]
internal partial class AppJsonSerializerContext : JsonSerializerContext
{
}