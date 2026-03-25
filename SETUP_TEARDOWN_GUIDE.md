# Unit Test Setup and Teardown Phases Guide

## Overview
Setup and teardown phases are crucial for:
- **Test Isolation**: Each test starts with a clean state
- **Resource Management**: Proper allocation and cleanup of resources
- **Reliability**: Preventing test interdependencies and flaky tests

## xUnit Patterns for Setup and Teardown

### 1. **Constructor and IDisposable Pattern** (Synchronous)
**Best for**: Simple cleanup, non-async resources

```csharp
public class MyTestClass : IDisposable
{
    private readonly MyDependency _dependency;

    // SETUP: Constructor runs before each test
    public MyTestClass()
    {
        _dependency = new MyDependency();
    }

    // TEARDOWN: Dispose runs after each test
    public void Dispose()
    {
        _dependency?.Dispose();
    }

    [Fact]
    public void MyTest()
    {
        // Use _dependency
    }
}
```

### 2. **IAsyncLifetime Pattern** (Asynchronous)
**Best for**: Async setup/teardown (database connections, async operations)

```csharp
public class MyAsyncTestClass : IAsyncLifetime
{
    private readonly IAsyncRepository _repository;

    public MyAsyncTestClass()
    {
        _repository = new AsyncRepository();
    }

    // SETUP: Called before each test
    public async Task InitializeAsync()
    {
        await _repository.InitializeAsync();
        await _repository.SeedDataAsync(testData);
    }

    // TEARDOWN: Called after each test
    public async Task DisposeAsync()
    {
        await _repository.CleanupAsync();
    }

    [Fact]
    public async Task MyAsyncTest()
    {
        // Use _repository
    }
}
```

### 3. **Fixture Pattern** (Reusable Setup)
**Best for**: Sharing complex setup across multiple test classes

```csharp
public class SharedFixture : IAsyncLifetime
{
    public Database Db { get; private set; }

    public async Task InitializeAsync()
    {
        Db = new Database();
        await Db.InitializeAsync();
    }

    public async Task DisposeAsync()
    {
        await Db.CleanupAsync();
    }
}

[Collection("Shared Collection")]
public class TestsUsingFixture
{
    private readonly SharedFixture _fixture;

    public TestsUsingFixture(SharedFixture fixture)
    {
        _fixture = fixture;
    }

    [Fact]
    public async Task TestUsingSharedFixture()
    {
        await _fixture.Db.QueryAsync(...);
    }
}
```

### 4. **Multiple Fixtures and Collections**
**Best for**: Complex scenarios with multiple dependencies

```csharp
[CollectionDefinition("Database and Cache Collection")]
public class DatabaseAndCacheCollection : 
    ICollectionFixture<DatabaseFixture>,
    ICollectionFixture<CacheFixture>
{
    // No code, just defines the collection
}

[Collection("Database and Cache Collection")]
public class ComplexTests
{
    private readonly DatabaseFixture _dbFixture;
    private readonly CacheFixture _cacheFixture;

    public ComplexTests(DatabaseFixture dbFixture, CacheFixture cacheFixture)
    {
        _dbFixture = dbFixture;
        _cacheFixture = cacheFixture;
    }
}
```

## Setup/Teardown Execution Order

### For Test Class with IAsyncLifetime:
1. Fixture's `InitializeAsync()` is called
2. Test class constructor runs
3. Test class's `InitializeAsync()` runs
4. **Test method executes**
5. Test class's `DisposeAsync()` runs
6. Fixture's `DisposeAsync()` is called

## Best Practices

### ✅ DO:
- **Clean up after each test**: Remove test data, close connections
- **Use fixtures for shared setup**: Avoid code duplication
- **Keep setup/teardown fast**: Don't do unnecessary work
- **Make tests independent**: No test should depend on another
- **Use IAsyncLifetime for async operations**: Better than Task.Run workarounds

### ❌ DON'T:
- **Share mutable state**: Each test needs its own state
- **Use test interdependencies**: Test A shouldn't depend on Test B running first
- **Ignore cleanup**: Resource leaks can cause test failures
- **Put test logic in setup**: Setup should only initialize, not test

## Execution Phases (AAA Pattern with Setup/Teardown)

```csharp
public class TestPhases : IAsyncLifetime
{
    // SETUP PHASE
    public async Task InitializeAsync()
    {
        // Initialize resources needed for all tests
        await InitializeDatabaseAsync();
        await SeedTestDataAsync();
    }

    [Fact]
    public async Task MyTest()
    {
        // ARRANGE PHASE (test-specific setup)
        var testInput = CreateTestData();

        // ACT PHASE (execute test)
        var result = await _service.ProcessAsync(testInput);

        // ASSERT PHASE (verify result)
        Assert.NotNull(result);
    }

    // TEARDOWN PHASE
    public async Task DisposeAsync()
    {
        // Clean up resources
        await CleanupDatabaseAsync();
        await DisconnectAsync();
    }
}
```

## Common Scenarios

### Database Testing
```csharp
public class DatabaseFixture : IAsyncLifetime
{
    private IDbConnection _connection;

    public async Task InitializeAsync()
    {
        _connection = new SqlConnection(connectionString);
        await _connection.OpenAsync();
        await CreateSchemaAsync();
    }

    public async Task DisposeAsync()
    {
        await DropSchemaAsync();
        _connection?.Dispose();
    }
}
```

### Mock Cleanup
```csharp
public class MockTestFixture : IDisposable
{
    public Mock<IService> ServiceMock { get; }

    public MockTestFixture()
    {
        ServiceMock = new Mock<IService>();
    }

    public void Dispose()
    {
        ServiceMock?.Dispose();
    }
}
```

### Temporary Files/Directories
```csharp
public class FileSystemFixture : IAsyncLifetime
{
    public string TempDirectory { get; private set; }

    public async Task InitializeAsync()
    {
        TempDirectory = Path.Combine(Path.GetTempPath(), Guid.NewGuid().ToString());
        Directory.CreateDirectory(TempDirectory);
        await Task.CompletedTask;
    }

    public async Task DisposeAsync()
    {
        if (Directory.Exists(TempDirectory))
        {
            Directory.Delete(TempDirectory, recursive: true);
        }
        await Task.CompletedTask;
    }
}
```

## Running Tests with Proper Setup/Teardown

All tests in your improved test files automatically benefit from setup/teardown:

```bash
# Run all tests (setup/teardown runs for each test)
dotnet test

# Run specific test class
dotnet test --filter "FullyQualifiedName~CarsControllerImprovedTests"

# Run with verbose output
dotnet test -v detailed
```

## Troubleshooting

### Issue: Setup takes too long
- **Solution**: Move expensive operations to collection-level fixtures (shared across all tests in collection)

### Issue: Tests are flaky/interdependent
- **Solution**: Ensure teardown properly cleans up state; verify isolation

### Issue: Async operations in setup aren't awaited
- **Solution**: Use `IAsyncLifetime.InitializeAsync()` instead of constructor

### Issue: Fixtures are shared but shouldn't be
- **Solution**: Remove `ICollectionFixture<>` and use direct fixture instantiation

## Reference Implementation Files

See the following files for complete examples:
- `CarRepositoryFixture.cs` - Async fixture with IAsyncLifetime
- `CarRepositoryIntegrationTests.cs` - Integration tests using fixtures
- `CarsControllerImprovedTests.cs` - Unit tests with mock fixtures
