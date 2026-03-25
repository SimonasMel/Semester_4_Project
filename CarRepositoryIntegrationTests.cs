using Xunit;
using Shared.Models;
using BackEnd.Repositories;
using BackEnd.Tests.Fixtures;

namespace BackEnd.Tests.Repositories
{
    /// <summary>
    /// Improved CarRepository tests using fixture for proper setup/teardown
    /// Each test method gets a fresh repository instance through the fixture
    /// </summary>
    [Collection("Car Repository Collection")]
    public class CarRepositoryIntegrationTests : IAsyncLifetime
    {
        private readonly CarRepositoryFixture _fixture;
        private CarRepository _repository = null!;

        public CarRepositoryIntegrationTests(CarRepositoryFixture fixture)
        {
            _fixture = fixture;
        }

        /// <summary>
        /// Test-specific setup (runs after fixture initialization)
        /// </summary>
        public async Task InitializeAsync()
        {
            // Create a fresh repository instance for this test
            _repository = new CarRepository();

            // Optionally seed with test data if needed
            foreach (var car in _fixture.TestCars)
            {
                await _repository.AddAsync(car);
            }
        }

        /// <summary>
        /// Test-specific teardown (runs after test completion)
        /// </summary>
        public async Task DisposeAsync()
        {
            // Clean up test-specific resources
            // Get all cars first, then delete to avoid collection modification exception
            var allCars = (await _repository.GetAllAsync()).ToList();
            foreach (var car in allCars)
            {
                await _repository.DeleteAsync(car.Id);
            }
        }

        // ==================== GetAllAsync Tests ====================

        [Fact]
        public async Task GetAllAsync_WhenNoCarsExist_ReturnsEmpty()
        {
            // Arrange - Clear repository for clean state
            var freshRepo = new CarRepository();

            // Act
            var result = await freshRepo.GetAllAsync();

            // Assert
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleCars_ReturnsAllCars()
        {
            // Act
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(3, result.Count()); // Should have 3 cars from fixture
            Assert.Contains(result, c => c.Brand == "Tesla");
            Assert.Contains(result, c => c.Brand == "BMW");
            Assert.Contains(result, c => c.Brand == "Audi");
        }

        // ==================== GetByIdAsync Tests ====================

        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsCar()
        {
            // Act
            var result = await _repository.GetByIdAsync("1");

            // Assert
            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("Tesla", result.Brand);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            // Act
            var result = await _repository.GetByIdAsync("nonexistent");

            // Assert
            Assert.Null(result);
        }

        // ==================== AddAsync Tests ====================

        [Fact]
        public async Task AddAsync_AddsNewCar_IncreasesCount()
        {
            // Arrange
            var newCar = _fixture.CreateTestCar("4", "Mercedes", "C-Class");
            var initialCount = (await _repository.GetAllAsync()).Count();

            // Act
            await _repository.AddAsync(newCar);
            var finalCount = (await _repository.GetAllAsync()).Count();

            // Assert
            Assert.Equal(initialCount + 1, finalCount);
            var addedCar = await _repository.GetByIdAsync("4");
            Assert.NotNull(addedCar);
            Assert.Equal("Mercedes", addedCar.Brand);
        }

        [Fact]
        public async Task AddAsync_MultipleCars_AllAreRetrievable()
        {
            // Arrange
            var car4 = _fixture.CreateTestCar("4", "Porsche", "911");
            var car5 = _fixture.CreateTestCar("5", "Ferrari", "F8");

            // Act
            await _repository.AddAsync(car4);
            await _repository.AddAsync(car5);
            var result = await _repository.GetAllAsync();

            // Assert
            Assert.Equal(5, result.Count());
            Assert.NotNull(await _repository.GetByIdAsync("4"));
            Assert.NotNull(await _repository.GetByIdAsync("5"));
        }

        // ==================== UpdateAsync Tests ====================

        [Fact]
        public async Task UpdateAsync_UpdatesExistingCar_ChangesArePersisted()
        {
            // Arrange
            var carToUpdate = await _repository.GetByIdAsync("1");
            Assert.NotNull(carToUpdate);
            carToUpdate.Price = 50000;
            carToUpdate.MileageKm = 5000;

            // Act
            await _repository.UpdateAsync(carToUpdate);
            var updatedCar = await _repository.GetByIdAsync("1");

            // Assert
            Assert.NotNull(updatedCar);
            Assert.Equal(50000, updatedCar.Price);
            Assert.Equal(5000, updatedCar.MileageKm);
        }

        // ==================== DeleteAsync Tests ====================

        [Fact]
        public async Task DeleteAsync_RemovesExistingCar_CarNoLongerRetrievable()
        {
            // Arrange
            var initialCount = (await _repository.GetAllAsync()).Count();

            // Act
            await _repository.DeleteAsync("1");
            var finalCount = (await _repository.GetAllAsync()).Count();
            var deletedCar = await _repository.GetByIdAsync("1");

            // Assert
            Assert.Equal(initialCount - 1, finalCount);
            Assert.Null(deletedCar);
        }

        [Fact]
        public async Task DeleteAsync_WithInvalidId_DoesNotThrow()
        {
            // Act
            var exception = await Record.ExceptionAsync(async () => 
                await _repository.DeleteAsync("nonexistent")
            );

            // Assert
            Assert.Null(exception);
        }

        // ==================== ExistsAsync Tests ====================

        [Fact]
        public async Task ExistsAsync_WithValidId_ReturnsTrue()
        {
            // Act
            var exists = await _repository.ExistsAsync("1");

            // Assert
            Assert.True(exists);
        }

        [Fact]
        public async Task ExistsAsync_WithInvalidId_ReturnsFalse()
        {
            // Act
            var exists = await _repository.ExistsAsync("nonexistent");

            // Assert
            Assert.False(exists);
        }
    }
}
