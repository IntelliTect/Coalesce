# Troubleshooting

This guide covers common issues you might encounter when developing with Coalesce and their solutions.

## Code Generation Issues

### Generated Files Not Updating

**Problem**: Changes to your C# models aren't reflected in the generated TypeScript files.

**Solutions**:
1. **Run code generation manually**: Execute `dotnet coalesce` in your solution directory
2. **Check coalesce.json configuration**: Ensure paths to your projects are correct
3. **Clean and rebuild**: Sometimes a clean build is needed to refresh dependencies
4. **Check attribute usage**: Ensure your models are decorated with `[Coalesce]` or exposed via `DbSet<>` properties

### Build Errors After Model Changes

**Problem**: TypeScript compilation errors after changing C# model structure.

**Solutions**:
1. **Regenerate code**: Run `dotnet coalesce` to update generated files
2. **Update client references**: Check if property names or types changed in your Vue components
3. **Clear node_modules**: Delete `node_modules` and run `npm install` if dependencies seem corrupted

### Missing API Endpoints

**Problem**: Expected API endpoints are not being generated.

**Checklist**:
- Is the class decorated with `[Coalesce]`?
- For entities: Is it exposed via a `DbSet<>` property on your `DbContext`?
- For services: Is the class or interface decorated with `[Coalesce]`?
- Check security attributes - are operations restricted by `[Read]`, `[Edit]`, etc.?

## Database Issues

### Navigation Properties Not Loading

**Problem**: Related entities are null or empty when they should contain data.

**Solutions**:
1. **Check Include configuration**: Use `.Include()` in your custom data sources
2. **Verify foreign keys**: Ensure EF Core relationships are properly configured
3. **Check security**: Navigation properties might be restricted by `[Read]` attributes
4. **Use IncludeTree**: Verify the [Include Tree](/concepts/include-tree.md) includes the desired properties

### Slow Query Performance

**Problem**: API endpoints are slow or timing out.

**Solutions**:
1. **Add database indexes**: Index foreign keys and frequently filtered properties
2. **Optimize data sources**: Use selective `.Include()` instead of loading all related data
3. **Implement paging**: Use `list.$params.pageSize` to limit result sets
4. **Use projection**: Consider custom DTOs to limit data transfer
5. **Check SQL queries**: Enable EF Core logging to examine generated SQL

## Security Issues

### Unauthorized Access Errors

**Problem**: Getting 401/403 errors when accessing APIs.

**Solutions**:
1. **Check authentication**: Ensure user is properly authenticated
2. **Verify roles**: Check if required roles are assigned to the user
3. **Review security attributes**: Confirm `[Execute]`, `[Read]`, etc. have correct role names
4. **Test with admin role**: Temporarily test with unrestricted access to isolate the issue

### Data Not Showing for Some Users

**Problem**: Some users can't see data that should be accessible.

**Solutions**:
1. **Check data sources**: Custom data sources might have user-specific filtering
2. **Review row-level security**: Look for user-based `.Where()` clauses in data sources
3. **Verify role assignments**: Ensure users have the necessary roles
4. **Check [Restrict] attributes**: These can hide data based on complex conditions

## Frontend Issues

### Vue Components Not Rendering

**Problem**: Coalesce Vue components appear blank or throw errors.

**Solutions**:
1. **Import components**: Ensure components are properly imported from `coalesce-vue-vuetify3`
2. **Check Vuetify setup**: Verify Vuetify is properly initialized in your app
3. **Verify props**: Check that required props are being passed to components
4. **Check console errors**: Browser developer tools often show helpful error messages

### Data Not Loading in ViewModels

**Problem**: `ViewModel.$load()` or `ListViewModel.$load()` not working.

**Solutions**:
1. **Check API endpoints**: Verify the corresponding API endpoints are accessible
2. **Examine network requests**: Use browser dev tools to see if requests are being made
3. **Verify parameters**: Check if required parameters are being passed correctly
4. **Check for async issues**: Ensure you're handling promises correctly

### Search Not Working

**Problem**: Search functionality returns no results or doesn't filter correctly.

**Solutions**:
1. **Add [Search] attributes**: Mark searchable properties with `[Search]`
2. **Check data types**: Ensure searchable properties are strings or have appropriate conversion
3. **Verify search terms**: Check if search input is being passed to `list.$params.search`
4. **Database collation**: Ensure database supports case-insensitive string comparison

## Performance Issues

### Slow Page Load Times

**Problem**: Pages take a long time to load, especially admin pages.

**Solutions**:
1. **Enable response caching**: Use `list.$useResponseCaching()` for frequently accessed data
2. **Implement pagination**: Set appropriate page sizes to limit data transfer
3. **Optimize includes**: Use selective data loading instead of eagerly loading all relationships
4. **Use ref responses**: Enable `list.$params.refResponse = true` to deduplicate repeated objects

### High Memory Usage

**Problem**: Application consumes excessive memory.

**Solutions**:
1. **Dispose ViewModels**: Call `$dispose()` on ViewModels when no longer needed
2. **Limit data loading**: Avoid loading large datasets without pagination
3. **Check for memory leaks**: Use browser dev tools to profile memory usage
4. **Optimize data sources**: Ensure queries are efficient and selective

## Common Error Messages

### "Object reference not set to an instance of an object"

**Cause**: Usually indicates a null reference in server-side code.

**Solutions**:
1. Check for null values before accessing properties
2. Ensure required services are properly registered in DI
3. Verify database relationships are properly configured

### "Cannot resolve service for type X"

**Cause**: Dependency injection cannot find a required service.

**Solutions**:
1. Register the service in your `Program.cs` or `Startup.cs`
2. Check constructor parameters in your data sources or services
3. Ensure the service implements the expected interface

### "No coalesce.json found"

**Cause**: Code generation cannot find configuration file.

**Solutions**:
1. Ensure `coalesce.json` exists in your solution root
2. Run `dotnet coalesce` from the correct directory
3. Check file permissions and path structure

## Getting Help

If you continue to experience issues:

1. **Check the logs**: Enable detailed logging to see what's happening
2. **Search GitHub issues**: Look for similar problems in the [Coalesce repository](https://github.com/IntelliTect/Coalesce/issues)
3. **Create a minimal reproduction**: Isolate the problem in a simple test case
4. **Contact support**: For commercial support, reach out to info@intellitect.com

## Debug Configuration

To enable detailed logging for troubleshooting:

```csharp
// In Program.cs or Startup.cs
builder.Logging.AddConsole();
builder.Logging.SetMinimumLevel(LogLevel.Debug);

// For EF Core query logging
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(connectionString)
           .EnableSensitiveDataLogging() // Only in development!
           .LogTo(Console.WriteLine, LogLevel.Information)
);
```

```typescript
// For client-side debugging in main.ts
import { AxiosClient } from 'coalesce-vue'

// Enable request/response logging
AxiosClient.defaults.validateStatus = () => true;
AxiosClient.interceptors.response.use(
    response => {
        console.log('API Response:', response);
        return response;
    },
    error => {
        console.error('API Error:', error);
        return Promise.reject(error);
    }
);
```