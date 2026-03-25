# Template: Creating a New Test Class with Setup/Teardown

Use this template as a starting point for creating new test classes with proper setup and teardown phases.

## Template: Async Fixture Pattern (Recommended for Integration Tests)

```csharp
using Xunit;
using YourProject.Services;
using YourProject.Repositories;

namespace YourProject.Tests.YourNamespace
{
    /// <summary>
    /// FIXTURE: Reusable async setup/teardown
    /// Implement IAsyncLifetime for operations that need await
    /// </summary>
    public class YourAsyncFixture : IAsyncLifetime
    {
        public IYourRepository Repository { get; private set; }
        public List<YourEntity> TestData { get; private set; }

        /// <summary>
        /// SETUP PHASE: Runs before each test
        /// </summary>
        public async Task InitializeAsync()
        {
            // Create fresh instances
            Repository = new YourRepository();

            // Prepare test data
            TestData = new List<YourEntity>
            {
                CreateTestEntity("1", "Item 1"),
                CreateTestEntity("2", "Item 2"),
                CreateTestEntity("3", "Item 3")
            };

            // Seed the repository
            foreach (var entity in TestData)
            {
                await Repository.AddAsync(entity);
            }
        }

        /// <summary>
        /// TEARDOWN PHASE: Runs after each test
        /// </summary>
        public async Task DisposeAsync()
        {
            // Convert to list first to avoid "collection modified during iteration" error
            var allEntities = (await Repository.GetAllAsync()).ToList();
            foreach (var entity in allEntities)
            {
                await Repository.DeleteAsync(entity.Id);
            }

            TestData?.Clear();
        }

        private YourEntity CreateTestEntity(string id, string name)
        {
            return new YourEntity
            {
                Id = id,
                Name = name,
                // Set other properties
            };
        }
    }

    /// <summary>
    /// TEST CLASS: Uses async fixture
    /// </summary>
    public class YourTests : IAsyncLifetime
    {
        private readonly YourAsyncFixture _fixture;

        public YourTests(YourAsyncFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// Optional: Test-specific setup
        /// </summary>
        public async Task InitializeAsync()
        {
            // Additional setup if needed
            await Task.CompletedTask;
        }

        /// <summary>
        /// Optional: Test-specific teardown
        /// </summary>
        public async Task DisposeAsync()
        {
            // Additional cleanup if needed
            await Task.CompletedTask;
        }

        // ==================== Tests ====================

        [Fact]
        public async Task GetAll_ReturnsAllItems()
        {
            // ARRANGE - Setup is already done by fixture

            // ACT
            var result = await _fixture.Repository.GetAllAsync();

            // ASSERT
            Assert.Equal(3, result.Count());
        }

        [Fact]
        public async Task GetById_WithValidId_ReturnsItem()
        {
            // ARRANGE
            var expectedItem = _fixture.TestData.First();

            // ACT
            var result = await _fixture.Repository.GetByIdAsync(expectedItem.Id);

            // ASSERT
            Assert.NotNull(result);
            Assert.Equal(expectedItem.Name, result.Name);
        }

        [Fact]
        public async Task Add_NewItem_IncrementsCount()
        {
            // ARRANGE
            var newItem = new YourEntity { Id = "4", Name = "New Item" };
            var initialCount = (await _fixture.Repository.GetAllAsync()).Count();

            // ACT
            await _fixture.Repository.AddAsync(newItem);
            var finalCount = (await _fixture.Repository.GetAllAsync()).Count();

            // ASSERT
            Assert.Equal(initialCount + 1, finalCount);
        }

        [Fact]
        public async Task Delete_ExistingItem_RemovesItem()
        {
            // ARRANGE
            var itemToDelete = _fixture.TestData.First();

            // ACT
            await _fixture.Repository.DeleteAsync(itemToDelete.Id);
            var result = await _fixture.Repository.GetByIdAsync(itemToDelete.Id);

            // ASSERT
            Assert.Null(result);
        }
    }
}
```

## Template: Sync Fixture Pattern (For Unit Tests with Mocks)

```csharp
using Xunit;
using Moq;
using YourProject.Services;

namespace YourProject.Tests.YourNamespace
{
    /// <summary>
    /// FIXTURE: Synchronous setup/teardown using IDisposable
    /// </summary>
    public class YourSyncFixture : IDisposable
    {
        public Mock<IYourService> ServiceMock { get; }
        public Mock<IYourDependency> DependencyMock { get; }

        /// <summary>
        /// Constructor serves as SETUP phase
        /// </summary>
        public YourSyncFixture()
        {
            // Initialize mocks
            ServiceMock = new Mock<IYourService>();
            DependencyMock = new Mock<IYourDependency>();

            // Configure default behavior
            ServiceMock.Setup(s => s.DoSomething())
                .Returns(true);
        }

        /// <summary>
        /// TEARDOWN phase
        /// </summary>
        public void Dispose()
        {
            // Clean up - mocks don't need explicit disposal in Moq 4.x
            GC.SuppressFinalize(this);
        }
    }

    /// <summary>
    /// TEST CLASS: Uses sync fixture
    /// </summary>
    public class YourUnitTests : IDisposable
    {
        private readonly YourSyncFixture _fixture;

        public YourUnitTests()
        {
            // SETUP phase
            _fixture = new YourSyncFixture();
        }

        public void Dispose()
        {
            // TEARDOWN phase
            _fixture?.Dispose();
        }

        [Fact]
        public void DoSomething_WhenConditionMet_ReturnsTrue()
        {
            // ARRANGE
            var input = "test";

            // ACT
            var result = _fixture.ServiceMock.Object.DoSomething();

            // ASSERT
            Assert.True(result);
            _fixture.ServiceMock.Verify(s => s.DoSomething(), Times.Once);
        }
    }
}
```

## Execution Flow

### For IAsyncLifetime Pattern:
```
1. Fixture.InitializeAsync()      ← SETUP
2. Test.InitializeAsync()         ← TEST-SPECIFIC SETUP
3. TEST METHOD EXECUTES           ← ARRANGE, ACT, ASSERT
4. Test.DisposeAsync()            ← TEST-SPECIFIC TEARDOWN
5. Fixture.DisposeAsync()         ← TEARDOWN
```

### For IDisposable Pattern:
```
1. Fixture constructor            ← SETUP
2. Test constructor               ← TEST-SPECIFIC SETUP
3. TEST METHOD EXECUTES           ← ARRANGE, ACT, ASSERT
4. Test.Dispose()                 ← TEST-SPECIFIC TEARDOWN
5. Fixture.Dispose()              ← TEARDOWN
```

## Key Points

### ✅ DO:
- Convert results to `.ToList()` before iterating in teardown to avoid collection modification errors
- Keep setup/teardown focused on initialization/cleanup, not test logic
- Create fresh instances in setup
- Clean up all resources in teardown
- Use meaningful test names that describe what they test

### ❌ DON'T:
- Leave setup/teardown logic missing - it breaks test isolation
- Modify collections while iterating over them
- Share mutable state between tests
- Put test assertions in setup/teardown
- Forget to handle async operations properly

## Choosing Your Pattern

| Pattern | Best For | Async Support | Reusability |
|---------|----------|---------------|-------------|
| IAsyncLifetime | Integration tests, databases | ✅ Yes | ✅ Good |
| IDisposable | Unit tests, mocks | ❌ No | ✅ Good |
| Constructor only | Simple tests | ❌ No | ❌ Limited |

## Common Mistakes to Avoid

### ❌ Mistake: Modifying collection during iteration
```csharp
// BAD
var items = await repo.GetAllAsync();
foreach (var item in items)  // ❌ Will throw if items is modified
{
    await repo.DeleteAsync(item.Id);  // ❌ Modifies the collection
}
```

### ✅ Correct: Convert to list first
```csharp
// GOOD
var items = (await repo.GetAllAsync()).ToList();  // ✅ Create new list
foreach (var item in items)  // ✅ Iterate over the list
{
    await repo.DeleteAsync(item.Id);  // ✅ Safe now
}
```

### ❌ Mistake: Forgetting to clean up
```csharp
// BAD
public async Task DisposeAsync()
{
    // Nothing - resources not cleaned up!
}
```

### ✅ Correct: Always cleanup
```csharp
// GOOD
public async Task DisposeAsync()
{
    var items = (await _repo.GetAllAsync()).ToList();
    foreach (var item in items)
    {
        await _repo.DeleteAsync(item.Id);
    }
}
```

## Real-World Examples in Your Project

See these files for working examples:
- `CarRepositoryFixture.cs` - Integration test fixture with IAsyncLifetime
- `CarRepositoryIntegrationTests.cs` - Integration tests using the fixture
- `CarsControllerImprovedTests.cs` - Unit tests with IDisposable and mocks
- `SETUP_TEARDOWN_GUIDE.md` - Complete reference documentation

## Testing Your Setup/Teardown

```bash
# Run with verbose output to see fixture initialization
dotnet test -v detailed

# Run specific test and observe setup/teardown
dotnet test --filter "FullyQualifiedName~YourTest.Method"

# Count total tests to verify they all run
dotnet test --collect:"XPlat Code Coverage"
```
