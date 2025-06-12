# Best Practices & Patterns

This guide covers recommended patterns and best practices for building robust Coalesce applications.

## Architecture Patterns

### Domain-Driven Design

Structure your models to reflect your business domain:

```csharp
// Good: Domain-focused entity
public class Order
{
    public int OrderId { get; set; }
    public DateTime OrderDate { get; set; }
    public OrderStatus Status { get; set; }
    public Customer Customer { get; set; }
    public ICollection<OrderItem> Items { get; set; } = [];

    // Business logic methods
    [Coalesce]
    public ItemResult CalculateTotal()
    {
        var total = Items.Sum(i => i.Quantity * i.UnitPrice);
        return new ItemResult<decimal>(total);
    }

    [Coalesce]
    public ItemResult Ship()
    {
        if (Status != OrderStatus.Pending)
            return "Order cannot be shipped from current status";
            
        Status = OrderStatus.Shipped;
        return true;
    }
}
```

### Service Layer Pattern

Use services for complex business operations that span multiple entities:

```csharp
[Coalesce]
public class OrderService
{
    public OrderService(AppDbContext db, IEmailService emailService)
    {
        Db = db;
        EmailService = emailService;
    }

    public AppDbContext Db { get; }
    public IEmailService EmailService { get; }

    [Execute]
    public async Task<ItemResult<Order>> CreateOrderAsync(
        int customerId, 
        List<OrderItemRequest> items)
    {
        var customer = await Db.Customers.FindAsync(customerId);
        if (customer == null)
            return "Customer not found";

        var order = new Order
        {
            CustomerId = customerId,
            OrderDate = DateTime.UtcNow,
            Status = OrderStatus.Pending
        };

        foreach (var item in items)
        {
            order.Items.Add(new OrderItem
            {
                ProductId = item.ProductId,
                Quantity = item.Quantity,
                UnitPrice = item.UnitPrice
            });
        }

        Db.Orders.Add(order);
        await Db.SaveChangesAsync();

        await EmailService.SendOrderConfirmationAsync(customer.Email, order);

        return order;
    }
}
```

## Security Best Practices

### Layered Security

Implement security at multiple layers:

```csharp
// 1. Entity-level security
[Read("User"), Edit("Admin"), Delete("Admin")]
public class SensitiveDocument
{
    public int Id { get; set; }
    public string Title { get; set; }
    public string UserId { get; set; }
    public User User { get; set; }

    // 2. Row-level security via data source
    [DefaultDataSource]
    public class MyDataSource : StandardDataSource<SensitiveDocument, AppDbContext>
    {
        public MyDataSource(CrudContext<AppDbContext> context) : base(context) { }

        public override IQueryable<SensitiveDocument> GetQuery(IDataSourceParameters parameters)
        {
            var query = base.GetQuery(parameters);
            
            if (!User.IsInRole("Admin"))
            {
                var userId = User.GetUserId();
                query = query.Where(d => d.UserId == userId);
            }
            
            return query;
        }
    }

    // 3. Method-level security
    [Coalesce, Execute("Manager")]
    public ItemResult Archive()
    {
        // Only managers can archive documents
        this.IsArchived = true;
        return true;
    }
}
```

### Input Validation

Always validate input data:

```csharp
public class ProductBehaviors : StandardBehaviors<Product, AppDbContext>
{
    public ProductBehaviors(CrudContext<AppDbContext> context) : base(context) { }

    public override ItemResult BeforeSave(SaveKind kind, Product? oldItem, Product item)
    {
        // Business rule validation
        if (item.Price <= 0)
            return "Price must be greater than zero";

        if (string.IsNullOrWhiteSpace(item.Name))
            return "Product name is required";

        if (kind == SaveKind.Update && oldItem != null)
        {
            // Prevent modification of critical fields
            if (oldItem.ProductCode != item.ProductCode)
                return "Product code cannot be changed";
        }

        return true;
    }
}
```

## Performance Optimization

### Efficient Data Loading

Use selective includes and projection:

```csharp
public class OptimizedProductDataSource : StandardDataSource<Product, AppDbContext>
{
    public OptimizedProductDataSource(CrudContext<AppDbContext> context) : base(context) { }

    public override IQueryable<Product> GetQuery(IDataSourceParameters parameters)
    {
        // Only include what's needed
        return Db.Products
            .Include(p => p.Category) // Only include direct relationships
            .Where(p => !p.IsDeleted); // Filter early
    }
}

// For list views, consider using custom DTOs for better performance
public class ProductSummaryDto : IClassDto<Product>
{
    public int ProductId { get; set; }
    public string Name { get; set; }
    public decimal Price { get; set; }
    public string CategoryName { get; set; }

    public void MapFrom(Product obj, IMappingContext context, IncludeTree? tree = null)
    {
        ProductId = obj.ProductId;
        Name = obj.Name;
        Price = obj.Price;
        CategoryName = obj.Category?.Name ?? "";
    }

    public void MapTo(Product obj, IMappingContext context) 
    {
        // Read-only DTO - no mapping back
        throw new NotSupportedException();
    }
}
```

### Response Caching

Enable caching for frequently accessed, relatively static data:

```typescript
// Use response caching for lookup data
const categoryList = new CategoryListViewModel();
categoryList.$useResponseCaching({ duration: 300 }); // 5 minutes
await categoryList.$load();

// Use ref responses for complex object graphs
const orderList = new OrderListViewModel();
orderList.$params.refResponse = true; // Deduplicate repeated objects
await orderList.$load();
```

## Error Handling

### Graceful Error Handling

Implement comprehensive error handling:

```csharp
[Coalesce]
public class PaymentService
{
    public PaymentService(
        AppDbContext db, 
        IPaymentProcessor processor,
        ILogger<PaymentService> logger)
    {
        Db = db;
        Processor = processor;
        Logger = logger;
    }

    public AppDbContext Db { get; }
    public IPaymentProcessor Processor { get; }
    public ILogger<PaymentService> Logger { get; }

    [Execute]
    public async Task<ItemResult<PaymentResult>> ProcessPaymentAsync(int orderId, decimal amount)
    {
        try
        {
            var order = await Db.Orders.FindAsync(orderId);
            if (order == null)
                return "Order not found";

            if (order.Status != OrderStatus.Pending)
                return "Order is not in a payable status";

            var result = await Processor.ProcessAsync(amount);
            if (!result.Success)
            {
                Logger.LogWarning("Payment failed for order {OrderId}: {Error}", 
                    orderId, result.ErrorMessage);
                return result.ErrorMessage ?? "Payment processing failed";
            }

            order.Status = OrderStatus.Paid;
            order.PaymentReference = result.TransactionId;
            await Db.SaveChangesAsync();

            return new PaymentResult { Success = true, TransactionId = result.TransactionId };
        }
        catch (Exception ex)
        {
            Logger.LogError(ex, "Unexpected error processing payment for order {OrderId}", orderId);
            return "An unexpected error occurred while processing payment";
        }
    }
}
```

### Client-Side Error Handling

Handle errors gracefully on the client:

```typescript
// Good error handling in Vue components
export default defineComponent({
  setup() {
    const orderViewModel = new OrderViewModel();
    const loading = ref(false);
    const error = ref<string | null>(null);

    const saveOrder = async () => {
      loading.value = true;
      error.value = null;
      
      try {
        const result = await orderViewModel.$save();
        if (result.wasSuccessful) {
          // Handle success
          router.push(`/orders/${result.object?.orderId}`);
        } else {
          error.value = result.message ?? 'Save failed';
        }
      } catch (err) {
        error.value = 'An unexpected error occurred';
        console.error('Save error:', err);
      } finally {
        loading.value = false;
      }
    };

    return { orderViewModel, loading, error, saveOrder };
  }
});
```

## Testing Strategies

### Unit Testing Models and Services

```csharp
[TestClass]
public class OrderServiceTests
{
    private AppDbContext CreateInMemoryContext()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }

    [TestMethod]
    public async Task CreateOrder_WithValidData_CreatesOrder()
    {
        // Arrange
        using var context = CreateInMemoryContext();
        var customer = new Customer { Name = "Test Customer", Email = "test@example.com" };
        context.Customers.Add(customer);
        await context.SaveChangesAsync();

        var emailService = new Mock<IEmailService>();
        var service = new OrderService(context, emailService.Object);

        // Act
        var result = await service.CreateOrderAsync(customer.CustomerId, new List<OrderItemRequest>
        {
            new() { ProductId = 1, Quantity = 2, UnitPrice = 10.00m }
        });

        // Assert
        Assert.IsTrue(result.WasSuccessful);
        Assert.IsNotNull(result.Object);
        Assert.AreEqual(OrderStatus.Pending, result.Object.Status);
        emailService.Verify(e => e.SendOrderConfirmationAsync(customer.Email, It.IsAny<Order>()), Times.Once);
    }
}
```

### Integration Testing

```csharp
[TestClass]
public class OrderIntegrationTests
{
    private WebApplicationFactory<Program> _factory;
    private HttpClient _client;

    [TestInitialize]
    public void Setup()
    {
        _factory = new WebApplicationFactory<Program>()
            .WithWebHostBuilder(builder =>
            {
                builder.ConfigureServices(services =>
                {
                    // Replace database with test database
                    services.AddDbContext<AppDbContext>(options =>
                        options.UseInMemoryDatabase("TestDb"));
                });
            });
        _client = _factory.CreateClient();
    }

    [TestMethod]
    public async Task GetOrders_ReturnsOrderList()
    {
        // Act
        var response = await _client.GetAsync("/api/Order/list");

        // Assert
        response.EnsureSuccessStatusCode();
        var content = await response.Content.ReadAsStringAsync();
        var result = JsonSerializer.Deserialize<ListResult<OrderDto>>(content);
        Assert.IsNotNull(result);
    }
}
```

## Development Workflow

### Code Generation Workflow

1. **Model First**: Design your domain models first
2. **Generate**: Run `dotnet coalesce` after model changes
3. **Customize**: Override generated behavior where needed
4. **Test**: Verify both API and frontend functionality

### Version Control Best Practices

```gitignore
# Include in .gitignore
/MyProject.Web/wwwroot/dist/
/MyProject.Web/Models/Generated/
/MyProject.Web/Api/Generated/
/MyProject.Web/src/models.g.ts
/MyProject.Web/src/viewmodels.g.ts
/MyProject.Web/src/api-clients.g.ts
/MyProject.Web/src/metadata.g.ts

# But DO commit
coalesce.json
/MyProject.Web/src/
/MyProject.Data/
```

### CI/CD Pipeline

```yaml
# Example GitHub Actions workflow
name: Build and Test
on: [push, pull_request]

jobs:
  build:
    runs-on: ubuntu-latest
    steps:
    - uses: actions/checkout@v3
    
    - name: Setup .NET
      uses: actions/setup-dotnet@v3
      with:
        dotnet-version: '8.x'
        
    - name: Setup Node.js
      uses: actions/setup-node@v3
      with:
        node-version: '18'
        
    - name: Restore dependencies
      run: dotnet restore
      
    - name: Install npm packages
      run: npm ci
      working-directory: ./MyProject.Web
      
    - name: Generate Coalesce code
      run: dotnet coalesce
      
    - name: Build
      run: dotnet build --no-restore
      
    - name: Test
      run: dotnet test --no-build --verbosity normal
      
    - name: Build frontend
      run: npm run build
      working-directory: ./MyProject.Web
```

## Common Anti-Patterns to Avoid

### ❌ Over-Eager Loading

```csharp
// Don't do this - loads too much data
public override IQueryable<Product> GetQuery(IDataSourceParameters parameters)
{
    return Db.Products
        .Include(p => p.Category)
            .ThenInclude(c => c.Products) // Circular reference!
        .Include(p => p.OrderItems)
            .ThenInclude(oi => oi.Order)
                .ThenInclude(o => o.Customer); // Too deep!
}
```

### ❌ Business Logic in Controllers

```csharp
// Don't put business logic in controllers
[HttpPost]
public async Task<IActionResult> ProcessOrder(int orderId)
{
    // This should be in a service or behavior
    var order = await Db.Orders.FindAsync(orderId);
    // ... complex business logic here
}
```

### ❌ Ignoring Security

```csharp
// Don't forget security attributes
public class AdminOnlyData // Missing [Read("Admin")]
{
    public string SensitiveInformation { get; set; }
}
```

### ❌ Not Using TypeScript

```javascript
// Don't use plain JavaScript - use TypeScript for better development experience
const viewModel = new PersonViewModel(); // No intellisense, no type safety
```

## Conclusion

Following these patterns and practices will help you build maintainable, secure, and performant Coalesce applications. Remember to:

- **Start simple** and evolve your architecture as needed
- **Test early and often** to catch issues before production
- **Secure by default** - always consider security implications
- **Monitor performance** and optimize based on real usage patterns
- **Document your customizations** for future developers