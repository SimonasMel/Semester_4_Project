# Quick Reference: Setup/Teardown Phases

## At a Glance

| Aspect | Setup Phase | Teardown Phase |
|--------|-------------|-----------------|
| **When** | Before each test | After each test |
| **Purpose** | Initialize resources | Clean up resources |
| **Async** | Can be async | Can be async |
| **Runs** | Once per test | Once per test |

---

## Two Patterns

### Pattern A: IAsyncLifetime (For Async Operations)
```csharp
public class MyFixture : IAsyncLifetime
{
    public async Task InitializeAsync() { }  // SETUP
    public async Task DisposeAsync() { }     // TEARDOWN
}

public class MyTests : IAsyncLifetime
{
    private readonly MyFixture _fixture;

    public MyTests(MyFixture fixture) => _fixture = fixture;

    public async Task InitializeAsync() { }  // Optional
    public async Task DisposeAsync() { }     // Optional
}
```

### Pattern B: IDisposable (For Sync/Mocks)
```csharp
public class MyFixture : IDisposable
{
    public MyFixture() { }      // SETUP
    public void Dispose() { }   // TEARDOWN
}

public class MyTests : IDisposable
{
    private readonly MyFixture _fixture;

    public MyTests() => _fixture = new MyFixture();
    public void Dispose() => _fixture?.Dispose();
}
```

---

## Setup Checklist

- [ ] Create fresh instances
- [ ] Initialize dependencies
- [ ] Configure mocks
- [ ] Seed test data
- [ ] Open connections/resources

## Teardown Checklist

- [ ] Remove test data (convert to list first!)
- [ ] Close connections
- [ ] Dispose resources
- [ ] Clear collections
- [ ] Reset state

---

## Common Patterns

### Initialize Repository
```csharp
public async Task InitializeAsync()
{
    Repository = new CarRepository();
    foreach (var car in TestCars)
    {
        await Repository.AddAsync(car);
    }
}
```

### Clean Up Repository
```csharp
public async Task DisposeAsync()
{
    var allCars = (await Repository.GetAllAsync()).ToList();
    foreach (var car in allCars)
    {
        await Repository.DeleteAsync(car.Id);
    }
}
```

### Setup Mocks
```csharp
public YourFixture()
{
    ServiceMock = new Mock<IService>();
    ServiceMock.Setup(s => s.DoSomething())
        .Returns(expectedValue);
}
```

---

## Test Structure

```csharp
[Fact]
public async Task TestName()
{
    // ARRANGE (test-specific setup using fixture)
    var testData = _fixture.TestData.First();

    // ACT (execute what you're testing)
    var result = await _service.ProcessAsync(testData);

    // ASSERT (verify the result)
    Assert.NotNull(result);
    Assert.Equal(expectedValue, result.Value);
}
```

---

## Key Rules

1. ✅ Always clean up in teardown
2. ✅ Convert IEnumerable to list before iteration in teardown
3. ✅ Each test gets fresh fixture instance
4. ✅ Tests are independent and isolated
5. ✅ Setup runs before each test, teardown after

---

## Run Tests

```bash
dotnet test                          # Run all
dotnet test -v detailed              # Verbose
dotnet test --filter "ClassName"     # Specific class
```

---

## Files to Reference

| File | Purpose |
|------|---------|
| `CarRepositoryFixture.cs` | Async fixture example |
| `CarRepositoryIntegrationTests.cs` | Integration test example |
| `CarsControllerImprovedTests.cs` | Unit test example |
| `SETUP_TEARDOWN_GUIDE.md` | Complete reference |
| `TEMPLATE_TEST_CLASS.md` | Copy-paste templates |

---

## Execution Order

```
Setup → Test → Teardown → (repeat for each test)

1. Fixture.InitializeAsync()
2. Test.InitializeAsync()
3. TEST METHOD EXECUTES
4. Test.DisposeAsync()
5. Fixture.DisposeAsync()
```

---

**Pro Tip**: Copy the templates from `TEMPLATE_TEST_CLASS.md` and customize for your tests!
