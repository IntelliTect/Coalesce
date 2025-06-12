# Integration Patterns

This guide covers common patterns for integrating Coalesce applications with external systems and third-party services.

## External API Integration

### Service Pattern for External APIs

Create dedicated services for external API interactions:

```csharp
[Coalesce]
public class PaymentService
{
    private readonly HttpClient _httpClient;
    private readonly ILogger<PaymentService> _logger;
    private readonly IConfiguration _config;

    public PaymentService(HttpClient httpClient, ILogger<PaymentService> logger, IConfiguration config)
    {
        _httpClient = httpClient;
        _logger = logger;
        _config = config;
    }

    [Execute("User")]
    public async Task<ItemResult<PaymentResponse>> ProcessPaymentAsync(PaymentRequest request)
    {
        try
        {
            var apiKey = _config["PaymentProvider:ApiKey"];
            _httpClient.DefaultRequestHeaders.Authorization = 
                new System.Net.Http.Headers.AuthenticationHeaderValue("Bearer", apiKey);

            var json = JsonSerializer.Serialize(request);
            var content = new StringContent(json, Encoding.UTF8, "application/json");

            var response = await _httpClient.PostAsync("/api/payments", content);
            
            if (response.IsSuccessStatusCode)
            {
                var responseJson = await response.Content.ReadAsStringAsync();
                var result = JsonSerializer.Deserialize<PaymentResponse>(responseJson);
                return result;
            }
            else
            {
                var error = await response.Content.ReadAsStringAsync();
                _logger.LogError("Payment API error: {StatusCode} - {Error}", response.StatusCode, error);
                return "Payment processing failed";
            }
        }
        catch (HttpRequestException ex)
        {
            _logger.LogError(ex, "Network error processing payment");
            return "Network error occurred";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error processing payment");
            return "An unexpected error occurred";
        }
    }
}

// External type for API request/response
public class PaymentRequest
{
    public decimal Amount { get; set; }
    public string Currency { get; set; } = "USD";
    public string CardToken { get; set; } = "";
    public string Description { get; set; } = "";
}

public class PaymentResponse
{
    public bool Success { get; set; }
    public string TransactionId { get; set; } = "";
    public string ErrorMessage { get; set; } = "";
    public DateTime ProcessedAt { get; set; }
}
```

### Configuration Setup

Register HTTP clients in your `Program.cs`:

```csharp
// Configure HTTP clients for external APIs
builder.Services.AddHttpClient<PaymentService>(client =>
{
    client.BaseAddress = new Uri(builder.Configuration["PaymentProvider:BaseUrl"]!);
    client.Timeout = TimeSpan.FromSeconds(30);
});

// Add retry policies with Polly (optional)
builder.Services.AddHttpClient<PaymentService>()
    .AddPolicyHandler(GetRetryPolicy());

static IAsyncPolicy<HttpResponseMessage> GetRetryPolicy()
{
    return HttpPolicyExtensions
        .HandleTransientHttpError()
        .WaitAndRetryAsync(
            retryCount: 3,
            sleepDurationProvider: retryAttempt => TimeSpan.FromSeconds(Math.Pow(2, retryAttempt)));
}
```

## Background Jobs with Hangfire

### Job Processing Service

```csharp
[Coalesce]
public class BackgroundJobService
{
    private readonly IBackgroundJobClient _backgroundJobs;
    private readonly AppDbContext _db;

    public BackgroundJobService(IBackgroundJobClient backgroundJobs, AppDbContext db)
    {
        _backgroundJobs = backgroundJobs;
        _db = db;
    }

    [Execute("Admin")]
    public ItemResult ScheduleDataExport(int userId, ExportType exportType)
    {
        var jobId = _backgroundJobs.Enqueue<DataExportJob>(job => job.ExportUserDataAsync(userId, exportType));
        
        // Track the job in your database
        var jobRecord = new BackgroundJobRecord
        {
            JobId = jobId,
            UserId = userId,
            JobType = "DataExport",
            Status = JobStatus.Queued,
            CreatedAt = DateTime.UtcNow
        };
        
        _db.BackgroundJobs.Add(jobRecord);
        _db.SaveChanges();

        return $"Export job scheduled with ID: {jobId}";
    }

    [Execute("User")]
    public async Task<ListResult<BackgroundJobRecord>> GetMyJobsAsync(int userId)
    {
        var jobs = await _db.BackgroundJobs
            .Where(j => j.UserId == userId)
            .OrderByDescending(j => j.CreatedAt)
            .ToListAsync();

        return jobs;
    }
}

public class DataExportJob
{
    private readonly AppDbContext _db;
    private readonly IEmailService _emailService;

    public DataExportJob(AppDbContext db, IEmailService emailService)
    {
        _db = db;
        _emailService = emailService;
    }

    public async Task ExportUserDataAsync(int userId, ExportType exportType)
    {
        try
        {
            // Update job status
            var jobRecord = await _db.BackgroundJobs
                .FirstOrDefaultAsync(j => j.UserId == userId && j.JobType == "DataExport");
            
            if (jobRecord != null)
            {
                jobRecord.Status = JobStatus.Processing;
                await _db.SaveChangesAsync();
            }

            // Perform the export
            var exportData = await GenerateExportData(userId, exportType);
            var fileName = $"export_{userId}_{DateTime.UtcNow:yyyyMMdd}.xlsx";
            
            // Upload to cloud storage or save locally
            var fileUrl = await SaveExportFile(fileName, exportData);

            // Send notification email
            var user = await _db.Users.FindAsync(userId);
            if (user != null)
            {
                await _emailService.SendExportCompleteNotificationAsync(user.Email, fileUrl);
            }

            // Mark as completed
            if (jobRecord != null)
            {
                jobRecord.Status = JobStatus.Completed;
                jobRecord.CompletedAt = DateTime.UtcNow;
                jobRecord.ResultUrl = fileUrl;
                await _db.SaveChangesAsync();
            }
        }
        catch (Exception ex)
        {
            // Mark as failed and log error
            var jobRecord = await _db.BackgroundJobs
                .FirstOrDefaultAsync(j => j.UserId == userId && j.JobType == "DataExport");
            
            if (jobRecord != null)
            {
                jobRecord.Status = JobStatus.Failed;
                jobRecord.ErrorMessage = ex.Message;
                await _db.SaveChangesAsync();
            }

            throw; // Let Hangfire handle retry logic
        }
    }

    private async Task<byte[]> GenerateExportData(int userId, ExportType exportType)
    {
        // Implementation depends on your requirements
        // Could use EPPlus, ClosedXML, or other libraries
        return Array.Empty<byte>();
    }

    private async Task<string> SaveExportFile(string fileName, byte[] data)
    {
        // Save to Azure Blob Storage, AWS S3, or local file system
        // Return publicly accessible URL
        return "https://your-storage.com/exports/" + fileName;
    }
}
```

## Email Integration

### Email Service with Templates

```csharp
[Coalesce]
public class EmailService
{
    private readonly IEmailSender _emailSender;
    private readonly ITemplateEngine _templateEngine;

    public EmailService(IEmailSender emailSender, ITemplateEngine templateEngine)
    {
        _emailSender = emailSender;
        _templateEngine = templateEngine;
    }

    [Execute("Admin")]
    public async Task<ItemResult> SendBulkEmailAsync(
        List<int> userIds, 
        string templateName, 
        object templateData)
    {
        var users = await _db.Users
            .Where(u => userIds.Contains(u.Id))
            .ToListAsync();

        var tasks = users.Select(async user =>
        {
            var personalizedData = new { User = user, Data = templateData };
            var htmlContent = await _templateEngine.RenderAsync(templateName, personalizedData);
            
            await _emailSender.SendEmailAsync(
                user.Email, 
                GetSubjectForTemplate(templateName), 
                htmlContent);
        });

        await Task.WhenAll(tasks);
        return $"Sent emails to {users.Count} users";
    }

    [Execute("User")]
    public async Task<ItemResult> RequestPasswordResetAsync(string email)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
        {
            // Don't reveal whether email exists
            return "If an account with that email exists, a reset link has been sent.";
        }

        var resetToken = GenerateResetToken();
        user.PasswordResetToken = resetToken;
        user.PasswordResetExpiry = DateTime.UtcNow.AddHours(1);
        await _db.SaveChangesAsync();

        var resetUrl = $"https://yourdomain.com/reset-password?token={resetToken}";
        var emailContent = await _templateEngine.RenderAsync("PasswordReset", new { 
            User = user, 
            ResetUrl = resetUrl 
        });

        await _emailSender.SendEmailAsync(user.Email, "Password Reset Request", emailContent);
        return "If an account with that email exists, a reset link has been sent.";
    }

    private string GenerateResetToken()
    {
        return Convert.ToBase64String(RandomNumberGenerator.GetBytes(32));
    }

    private string GetSubjectForTemplate(string templateName)
    {
        return templateName switch
        {
            "Welcome" => "Welcome to Our Platform",
            "OrderConfirmation" => "Order Confirmation",
            "PasswordReset" => "Password Reset Request",
            _ => "Notification"
        };
    }
}
```

## File Storage Integration

### Cloud Storage Service

```csharp
[Coalesce]
public class FileStorageService
{
    private readonly BlobServiceClient _blobServiceClient;
    private readonly IConfiguration _config;

    public FileStorageService(BlobServiceClient blobServiceClient, IConfiguration config)
    {
        _blobServiceClient = blobServiceClient;
        _config = config;
    }

    [Execute("User")]
    public async Task<ItemResult<FileUploadResult>> UploadFileAsync(
        IFormFile file, 
        string category = "general")
    {
        if (file.Length == 0)
            return "File is empty";

        if (file.Length > 10 * 1024 * 1024) // 10MB limit
            return "File size exceeds 10MB limit";

        var allowedExtensions = new[] { ".jpg", ".jpeg", ".png", ".pdf", ".docx" };
        var extension = Path.GetExtension(file.FileName).ToLowerInvariant();
        
        if (!allowedExtensions.Contains(extension))
            return "File type not allowed";

        try
        {
            var containerName = _config["AzureStorage:ContainerName"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            
            var fileName = $"{category}/{Guid.NewGuid()}{extension}";
            var blobClient = containerClient.GetBlobClient(fileName);

            var metadata = new Dictionary<string, string>
            {
                ["OriginalFileName"] = file.FileName,
                ["ContentType"] = file.ContentType,
                ["UploadedBy"] = User.Identity?.Name ?? "Anonymous"
            };

            using var stream = file.OpenReadStream();
            await blobClient.UploadAsync(stream, new BlobHttpHeaders 
            { 
                ContentType = file.ContentType 
            }, metadata);

            var result = new FileUploadResult
            {
                FileName = fileName,
                OriginalFileName = file.FileName,
                Url = blobClient.Uri.ToString(),
                Size = file.Length
            };

            return result;
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error uploading file {FileName}", file.FileName);
            return "Upload failed";
        }
    }

    [Execute("User")]
    public async Task<ItemResult> DeleteFileAsync(string fileName)
    {
        try
        {
            var containerName = _config["AzureStorage:ContainerName"];
            var containerClient = _blobServiceClient.GetBlobContainerClient(containerName);
            var blobClient = containerClient.GetBlobClient(fileName);

            var response = await blobClient.DeleteIfExistsAsync();
            
            return response.Value 
                ? "File deleted successfully" 
                : "File not found";
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Error deleting file {FileName}", fileName);
            return "Delete failed";
        }
    }
}

public class FileUploadResult
{
    public string FileName { get; set; } = "";
    public string OriginalFileName { get; set; } = "";
    public string Url { get; set; } = "";
    public long Size { get; set; }
}
```

## Caching Integration

### Redis Caching Service

```csharp
[Coalesce]
public class CachingService
{
    private readonly IDistributedCache _cache;
    private readonly ILogger<CachingService> _logger;

    public CachingService(IDistributedCache cache, ILogger<CachingService> logger)
    {
        _cache = cache;
        _logger = logger;
    }

    [Execute("Admin")]
    public async Task<ItemResult> ClearCacheAsync(string pattern = "*")
    {
        // Note: This requires Redis-specific implementation
        // for pattern matching. For other cache providers,
        // you might need to track keys separately.
        
        if (_cache is IRedisCache redisCache)
        {
            await redisCache.RemoveByPatternAsync(pattern);
            return $"Cache cleared for pattern: {pattern}";
        }

        return "Cache clearing not supported for this provider";
    }

    public async Task<T?> GetOrSetAsync<T>(
        string key, 
        Func<Task<T>> getItem, 
        TimeSpan? expiry = null)
    {
        var cachedValue = await _cache.GetStringAsync(key);
        
        if (cachedValue != null)
        {
            return JsonSerializer.Deserialize<T>(cachedValue);
        }

        var item = await getItem();
        if (item != null)
        {
            var serialized = JsonSerializer.Serialize(item);
            var options = new DistributedCacheEntryOptions();
            
            if (expiry.HasValue)
                options.SetAbsoluteExpiration(expiry.Value);
            else
                options.SetSlidingExpiration(TimeSpan.FromMinutes(30));

            await _cache.SetStringAsync(key, serialized, options);
        }

        return item;
    }
}

// Usage in other services
public class ProductService
{
    private readonly CachingService _cache;
    private readonly AppDbContext _db;

    public ProductService(CachingService cache, AppDbContext db)
    {
        _cache = cache;
        _db = db;
    }

    [Execute]
    public async Task<ListResult<Product>> GetFeaturedProductsAsync()
    {
        var products = await _cache.GetOrSetAsync(
            "featured-products",
            async () => await _db.Products
                .Where(p => p.IsFeatured && !p.IsDeleted)
                .OrderBy(p => p.SortOrder)
                .ToListAsync(),
            TimeSpan.FromHours(1)
        );

        return products ?? new List<Product>();
    }
}
```

## Event-Driven Architecture

### Domain Events

```csharp
public interface IDomainEvent
{
    DateTime OccurredAt { get; }
}

public class OrderCreatedEvent : IDomainEvent
{
    public int OrderId { get; set; }
    public int CustomerId { get; set; }
    public decimal TotalAmount { get; set; }
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

public class OrderShippedEvent : IDomainEvent
{
    public int OrderId { get; set; }
    public string TrackingNumber { get; set; } = "";
    public DateTime OccurredAt { get; set; } = DateTime.UtcNow;
}

// Event handlers
public class OrderEventHandler
{
    private readonly IEmailService _emailService;
    private readonly AppDbContext _db;

    public OrderEventHandler(IEmailService emailService, AppDbContext db)
    {
        _emailService = emailService;
        _db = db;
    }

    public async Task HandleAsync(OrderCreatedEvent eventData)
    {
        // Send order confirmation email
        var order = await _db.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderId == eventData.OrderId);

        if (order?.Customer != null)
        {
            await _emailService.SendOrderConfirmationAsync(order.Customer.Email, order);
        }
    }

    public async Task HandleAsync(OrderShippedEvent eventData)
    {
        // Send shipping notification
        var order = await _db.Orders
            .Include(o => o.Customer)
            .FirstOrDefaultAsync(o => o.OrderId == eventData.OrderId);

        if (order?.Customer != null)
        {
            await _emailService.SendShippingNotificationAsync(
                order.Customer.Email, 
                order, 
                eventData.TrackingNumber);
        }
    }
}

// Integration in behaviors
public class OrderBehaviors : StandardBehaviors<Order, AppDbContext>
{
    private readonly IEventPublisher _eventPublisher;

    public OrderBehaviors(CrudContext<AppDbContext> context, IEventPublisher eventPublisher) 
        : base(context)
    {
        _eventPublisher = eventPublisher;
    }

    public override async Task<ItemResult> AfterSaveAsync(SaveKind kind, Order? oldItem, Order item)
    {
        if (kind == SaveKind.Create)
        {
            await _eventPublisher.PublishAsync(new OrderCreatedEvent
            {
                OrderId = item.OrderId,
                CustomerId = item.CustomerId,
                TotalAmount = item.TotalAmount
            });
        }
        else if (oldItem?.Status != OrderStatus.Shipped && item.Status == OrderStatus.Shipped)
        {
            await _eventPublisher.PublishAsync(new OrderShippedEvent
            {
                OrderId = item.OrderId,
                TrackingNumber = item.TrackingNumber ?? ""
            });
        }

        return await base.AfterSaveAsync(kind, oldItem, item);
    }
}
```

## Monitoring and Logging

### Application Insights Integration

```csharp
public class TelemetryService
{
    private readonly TelemetryClient _telemetryClient;

    public TelemetryService(TelemetryClient telemetryClient)
    {
        _telemetryClient = telemetryClient;
    }

    public void TrackCustomEvent(string eventName, Dictionary<string, string>? properties = null)
    {
        _telemetryClient.TrackEvent(eventName, properties);
    }

    public void TrackBusinessMetric(string metricName, double value, Dictionary<string, string>? properties = null)
    {
        _telemetryClient.TrackMetric(metricName, value, properties);
    }
}

// Usage in services
[Coalesce]
public class OrderService
{
    private readonly TelemetryService _telemetry;

    public OrderService(TelemetryService telemetry)
    {
        _telemetry = telemetry;
    }

    [Execute("User")]
    public async Task<ItemResult<Order>> PlaceOrderAsync(OrderRequest request)
    {
        var stopwatch = Stopwatch.StartNew();
        
        try
        {
            // Process order...
            var order = await ProcessOrderInternal(request);
            
            stopwatch.Stop();
            
            // Track success metrics
            _telemetry.TrackCustomEvent("OrderPlaced", new Dictionary<string, string>
            {
                ["CustomerId"] = request.CustomerId.ToString(),
                ["OrderValue"] = order.TotalAmount.ToString(),
                ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds.ToString()
            });

            _telemetry.TrackBusinessMetric("OrderValue", (double)order.TotalAmount);
            
            return order;
        }
        catch (Exception ex)
        {
            stopwatch.Stop();
            
            // Track failure metrics
            _telemetry.TrackCustomEvent("OrderFailed", new Dictionary<string, string>
            {
                ["CustomerId"] = request.CustomerId.ToString(),
                ["Error"] = ex.Message,
                ["ProcessingTimeMs"] = stopwatch.ElapsedMilliseconds.ToString()
            });
            
            throw;
        }
    }
}
```

## Configuration Management

### Feature Flags and Settings

```csharp
[Coalesce]
public class ConfigurationService
{
    private readonly IConfiguration _config;
    private readonly AppDbContext _db;

    public ConfigurationService(IConfiguration config, AppDbContext db)
    {
        _config = config;
        _db = db;
    }

    [Execute("Admin")]
    public async Task<ItemResult<AppSetting>> UpdateSettingAsync(string key, string value)
    {
        var setting = await _db.AppSettings.FirstOrDefaultAsync(s => s.Key == key);
        
        if (setting == null)
        {
            setting = new AppSetting { Key = key, Value = value };
            _db.AppSettings.Add(setting);
        }
        else
        {
            setting.Value = value;
            setting.LastModified = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();
        
        // Invalidate cache
        await InvalidateSettingCacheAsync(key);
        
        return setting;
    }

    [Execute]
    public async Task<string> GetSettingAsync(string key, string defaultValue = "")
    {
        // Check database first
        var setting = await _db.AppSettings
            .FirstOrDefaultAsync(s => s.Key == key && s.IsEnabled);
            
        if (setting != null)
            return setting.Value;

        // Fall back to appsettings.json
        return _config[key] ?? defaultValue;
    }

    [Execute]
    public async Task<bool> IsFeatureEnabledAsync(string featureName)
    {
        var setting = await GetSettingAsync($"Features:{featureName}", "false");
        return bool.TryParse(setting, out var isEnabled) && isEnabled;
    }
}

public class AppSetting
{
    public int Id { get; set; }
    public string Key { get; set; } = "";
    public string Value { get; set; } = "";
    public bool IsEnabled { get; set; } = true;
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? LastModified { get; set; }
}
```

These integration patterns provide a solid foundation for building robust, scalable Coalesce applications that can interact with various external systems and services.