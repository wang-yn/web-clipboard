using System.Collections.Concurrent;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Security.Cryptography;
using System.Text;
using System.Net;
using Microsoft.AspNetCore.HttpOverrides;

var builder = WebApplication.CreateBuilder(args);

// Configure for proxy environment
builder.Services.Configure<ForwardedHeadersOptions>(options =>
{
    options.ForwardedHeaders = ForwardedHeaders.XForwardedFor | ForwardedHeaders.XForwardedProto | ForwardedHeaders.XForwardedHost;
    // Accept forwarded headers from any source (configure more restrictively in production)
    options.KnownNetworks.Clear();
    options.KnownProxies.Clear();
});

// Configure JSON options for AOT compatibility
builder.Services.ConfigureHttpJsonOptions(options =>
{
    options.SerializerOptions.TypeInfoResolverChain.Insert(0, AppJsonSerializerContext.Default);
});

// Security services
builder.Services.AddSingleton<SecurityService>();
builder.Services.AddSingleton<RateLimitService>();

builder.Services.AddCors(options =>
{
    options.AddDefaultPolicy(policy =>
    {
        // Allow common development and proxy origins
        var allowedOrigins = new[]
        {
            "http://localhost:5000",
            "https://localhost:5001",
            "http://localhost:8080",
            "https://localhost:8443",
            "http://127.0.0.1:5000",
            "http://127.0.0.1:8080"
        };
        
        // In production, you should configure this more restrictively
        policy.WithOrigins(allowedOrigins)
              .AllowAnyMethod()
              .AllowAnyHeader()
              .WithExposedHeaders("Content-Disposition");
    });
});

var app = builder.Build();

// Use forwarded headers (must be before other middleware)
app.UseForwardedHeaders();

app.UseCors();

// Security middleware
app.Use(async (context, next) =>
{
    // Security headers
    context.Response.Headers.Append("X-Content-Type-Options", "nosniff");
    context.Response.Headers.Append("X-Frame-Options", "DENY");
    context.Response.Headers.Append("X-XSS-Protection", "1; mode=block");
    context.Response.Headers.Append("Referrer-Policy", "strict-origin-when-cross-origin");
    context.Response.Headers.Append("Content-Security-Policy", "default-src 'self' 'unsafe-inline' cdn.tailwindcss.com");
    
    await next();
});

// Rate limiting middleware
app.UseMiddleware<RateLimitMiddleware>();

app.UseDefaultFiles(); // Must be before UseStaticFiles
app.UseStaticFiles();

var clipboardData = new ConcurrentDictionary<string, ClipboardItem>();

// Create temp directory for files
var tempDir = Path.Combine(Path.GetTempPath(), "web-clipboard");
Directory.CreateDirectory(tempDir);

// Cleanup temp directory on startup
foreach (var oldFile in Directory.GetFiles(tempDir))
{
    try { File.Delete(oldFile); } catch { }
}

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

app.MapPost("/api/text", async (TextRequest request, HttpContext context, SecurityService security) =>
{
    // Security validation
    if (!await security.ValidateContentRequest(context, request.Content))
    {
        return Results.BadRequest("Request rejected for security reasons");
    }
    
    var id = GenerateShortId(clipboardData);
    var item = new ClipboardItem
    {
        Id = id,
        Type = "text",
        Content = request.Content,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddMinutes(10)
    };
    
    clipboardData[id] = item;
    return Results.Ok(new SaveTextResponse(id, item.ExpiresAt));
});

app.MapGet("/api/text/{id}", async (string id, HttpContext context, SecurityService security) =>
{
    // Security validation
    if (!await security.ValidateAccessRequest(context, id))
    {
        return Results.BadRequest("Access denied");
    }
    
    if (clipboardData.TryGetValue(id, out var item) && 
        item.Type == "text" && 
        item.ExpiresAt > DateTime.UtcNow)
    {
        security.LogAccess(context, id, "text", true);
        return Results.Ok(new GetTextResponse(item.Content!, item.CreatedAt));
    }
    
    security.LogAccess(context, id, "text", false);
    return Results.NotFound();
});

app.MapPost("/api/file", async (HttpRequest request, SecurityService security) =>
{
    // Security validation
    if (!await security.ValidateFileRequest(request))
    {
        return Results.BadRequest("Request rejected for security reasons");
    }
    
    var form = await request.ReadFormAsync();
    var file = form.Files.FirstOrDefault();
    
    if (file == null)
        return Results.BadRequest("No file uploaded");
        
    if (!security.ValidateFileType(file.FileName, file.ContentType))
    {
        return Results.BadRequest("File type not allowed");
    }
    
    if (file.Length > 50 * 1024 * 1024) // 50MB limit
    {
        return Results.BadRequest("File too large (max 50MB)");
    }
    
    var id = GenerateShortId(clipboardData);
    var filePath = Path.Combine(tempDir, $"{id}_{file.FileName}");
    
    // Save file to temp directory
    using var fileStream = new FileStream(filePath, FileMode.Create);
    await file.CopyToAsync(fileStream);
    
    var item = new ClipboardItem
    {
        Id = id,
        Type = "file",
        FileName = file.FileName,
        FilePath = filePath, // Store file path instead of data
        ContentType = file.ContentType,
        CreatedAt = DateTime.UtcNow,
        ExpiresAt = DateTime.UtcNow.AddMinutes(10)
    };
    
    clipboardData[id] = item;
    return Results.Ok(new SaveFileResponse(id, file.FileName, item.ExpiresAt));
});

app.MapGet("/api/file/{id}", async (string id, HttpContext context, SecurityService security) =>
{
    // Security validation
    if (!await security.ValidateAccessRequest(context, id))
    {
        return Results.BadRequest("Access denied");
    }
    
    if (clipboardData.TryGetValue(id, out var item) && 
        item.Type == "file" && 
        item.ExpiresAt > DateTime.UtcNow &&
        File.Exists(item.FilePath))
    {
        security.LogAccess(context, id, "file", true);
        return Results.File(item.FilePath!, item.ContentType, item.FileName);
    }
    
    security.LogAccess(context, id, "file", false);
    return Results.NotFound();
});

app.MapDelete("/api/{id}", (string id) =>
{
    if (clipboardData.TryRemove(id, out var item) && item.Type == "file" && !string.IsNullOrEmpty(item.FilePath))
    {
        try { File.Delete(item.FilePath); } catch { }
    }
    return Results.Ok();
});

app.MapGet("/api/cleanup", () =>
{
    var expiredItems = clipboardData
        .Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
        .ToList();
    
    foreach (var (key, item) in expiredItems)
    {
        // Delete file from disk if it exists
        if (item.Type == "file" && !string.IsNullOrEmpty(item.FilePath))
        {
            try { File.Delete(item.FilePath); } catch { }
        }
        
        clipboardData.TryRemove(key, out _);
    }
    
    return Results.Ok(new CleanupResponse(expiredItems.Count));
});

app.MapFallbackToFile("index.html");

// Background cleanup task
var cleanupTimer = new Timer(_ =>
{
    try
    {
        var expiredItems = clipboardData
            .Where(kvp => kvp.Value.ExpiresAt <= DateTime.UtcNow)
            .ToList();
        
        foreach (var (key, item) in expiredItems)
        {
            if (item.Type == "file" && !string.IsNullOrEmpty(item.FilePath))
            {
                try { File.Delete(item.FilePath); } catch { }
            }
            clipboardData.TryRemove(key, out var _);
        }
        
        if (expiredItems.Count > 0)
        {
            Console.WriteLine($"Cleaned up {expiredItems.Count} expired items");
        }
    }
    catch (Exception ex)
    {
        Console.WriteLine($"Cleanup error: {ex.Message}");
    }
}, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1)); // Check every minute

app.Run();

// Cleanup timer when app stops
cleanupTimer.Dispose();

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
    public string? FilePath { get; set; } // Path to file in temp directory
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

// Security services
public class RateLimitService
{
    private readonly ConcurrentDictionary<string, RateLimitInfo> _ipLimits = new();
    private readonly Timer _cleanupTimer;

    public RateLimitService()
    {
        _cleanupTimer = new Timer(CleanupExpired, null, TimeSpan.FromMinutes(1), TimeSpan.FromMinutes(1));
    }

    public bool IsAllowed(string ipAddress, string endpoint)
    {
        var key = $"{ipAddress}:{endpoint}";
        var now = DateTime.UtcNow;
        
        var info = _ipLimits.AddOrUpdate(key, 
            new RateLimitInfo { Count = 1, WindowStart = now },
            (k, existing) =>
            {
                if (now - existing.WindowStart > TimeSpan.FromMinutes(1))
                {
                    return new RateLimitInfo { Count = 1, WindowStart = now };
                }
                existing.Count++;
                return existing;
            });

        // Different limits for different endpoints
        return endpoint switch
        {
            "POST" => info.Count <= 20, // 20 uploads per minute
            "GET" => info.Count <= 100, // 100 downloads per minute
            _ => info.Count <= 50
        };
    }

    private void CleanupExpired(object? state)
    {
        var cutoff = DateTime.UtcNow - TimeSpan.FromMinutes(2);
        var expiredKeys = _ipLimits
            .Where(kvp => kvp.Value.WindowStart < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _ipLimits.TryRemove(key, out _);
        }
    }
}

public class RateLimitInfo
{
    public int Count { get; set; }
    public DateTime WindowStart { get; set; }
}

public class RateLimitMiddleware
{
    private readonly RequestDelegate _next;
    private readonly RateLimitService _rateLimitService;

    public RateLimitMiddleware(RequestDelegate next, RateLimitService rateLimitService)
    {
        _next = next;
        _rateLimitService = rateLimitService;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        var ipAddress = context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
        var endpoint = context.Request.Method;

        if (!_rateLimitService.IsAllowed(ipAddress, endpoint))
        {
            context.Response.StatusCode = 429; // Too Many Requests
            await context.Response.WriteAsync("Rate limit exceeded. Please slow down.");
            return;
        }

        await _next(context);
    }
}

public class SecurityService
{
    private readonly ConcurrentDictionary<string, FailedAttemptInfo> _failedAttempts = new();
    private readonly HashSet<string> _blockedIps = new();
    private readonly Timer _cleanupTimer;

    // Dangerous file extensions
    private readonly HashSet<string> _blockedExtensions = new()
    {
        ".exe", ".bat", ".cmd", ".com", ".pif", ".scr", ".vbs", ".js", ".jar", 
        ".ps1", ".sh", ".msi", ".dll", ".sys", ".php", ".asp", ".aspx", ".jsp"
    };

    // Suspicious content patterns
    private readonly string[] _suspiciousPatterns = new[]
    {
        "<script", "javascript:", "data:text/html", "eval(", "document.write",
        "base64,", "php://", "file://", "ftp://", "../../"
    };

    public SecurityService()
    {
        _cleanupTimer = new Timer(CleanupExpired, null, TimeSpan.FromMinutes(5), TimeSpan.FromMinutes(5));
    }

    public Task<bool> ValidateContentRequest(HttpContext context, string content)
    {
        var ip = GetClientIP(context);
        
        // Check if IP is blocked
        if (_blockedIps.Contains(ip))
        {
            return Task.FromResult(false);
        }

        // Content size check
        if (content.Length > 1024 * 1024) // 1MB limit for text
        {
            RecordFailedAttempt(ip, "Large content");
            return Task.FromResult(false);
        }

        // Check for suspicious content
        var lowerContent = content.ToLower();
        foreach (var pattern in _suspiciousPatterns)
        {
            if (lowerContent.Contains(pattern.ToLower()))
            {
                RecordFailedAttempt(ip, $"Suspicious pattern: {pattern}");
                return Task.FromResult(false);
            }
        }

        return Task.FromResult(true);
    }

    public Task<bool> ValidateFileRequest(HttpRequest request)
    {
        var ip = GetClientIP(request.HttpContext);
        
        if (_blockedIps.Contains(ip))
        {
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public bool ValidateFileType(string fileName, string? contentType)
    {
        if (string.IsNullOrEmpty(fileName))
            return false;

        var extension = Path.GetExtension(fileName).ToLower();
        return !_blockedExtensions.Contains(extension);
    }

    public Task<bool> ValidateAccessRequest(HttpContext context, string id)
    {
        var ip = GetClientIP(context);
        
        // Check if IP is blocked
        if (_blockedIps.Contains(ip))
        {
            return Task.FromResult(false);
        }

        // Check for brute force attempts (too many failed requests for non-existent IDs)
        if (_failedAttempts.TryGetValue(ip, out var info) && info.Count > 50)
        {
            _blockedIps.Add(ip);
            Console.WriteLine($"Blocked IP {ip} for excessive failed attempts");
            return Task.FromResult(false);
        }

        return Task.FromResult(true);
    }

    public void LogAccess(HttpContext context, string id, string type, bool success)
    {
        var ip = GetClientIP(context);
        var timestamp = DateTime.UtcNow;
        
        Console.WriteLine($"[{timestamp}] {ip} accessed {type} {id}: {(success ? "SUCCESS" : "FAILED")}");
        
        if (!success)
        {
            RecordFailedAttempt(ip, $"Failed access to {id}");
        }
    }

    private string GetClientIP(HttpContext context)
    {
        // Try to get real IP from headers (behind proxy/load balancer)
        var xForwardedFor = context.Request.Headers["X-Forwarded-For"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xForwardedFor))
        {
            return xForwardedFor.Split(',')[0].Trim();
        }

        var xRealIp = context.Request.Headers["X-Real-IP"].FirstOrDefault();
        if (!string.IsNullOrEmpty(xRealIp))
        {
            return xRealIp;
        }

        return context.Connection.RemoteIpAddress?.ToString() ?? "unknown";
    }

    private void RecordFailedAttempt(string ip, string reason)
    {
        var info = _failedAttempts.AddOrUpdate(ip,
            new FailedAttemptInfo { Count = 1, LastAttempt = DateTime.UtcNow, Reason = reason },
            (k, existing) =>
            {
                existing.Count++;
                existing.LastAttempt = DateTime.UtcNow;
                existing.Reason = reason;
                return existing;
            });

        if (info.Count > 20) // Block after 20 failed attempts
        {
            _blockedIps.Add(ip);
            Console.WriteLine($"Blocked IP {ip} after {info.Count} failed attempts. Latest: {reason}");
        }
    }

    private void CleanupExpired(object? state)
    {
        var cutoff = DateTime.UtcNow - TimeSpan.FromHours(1);
        var expiredKeys = _failedAttempts
            .Where(kvp => kvp.Value.LastAttempt < cutoff)
            .Select(kvp => kvp.Key)
            .ToList();

        foreach (var key in expiredKeys)
        {
            _failedAttempts.TryRemove(key, out _);
        }
    }
}

public class FailedAttemptInfo
{
    public int Count { get; set; }
    public DateTime LastAttempt { get; set; }
    public string Reason { get; set; } = string.Empty;
}