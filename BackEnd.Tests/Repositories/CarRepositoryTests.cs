using Xunit;
using Shared.Models;
using BackEnd.Repositories;

namespace BackEnd.Tests.Repositories
{
    public class CarRepositoryTests
    {
        private readonly CarRepository _repository;

        public CarRepositoryTests()
        {
            _repository = new CarRepository();
        }

        private Car CreateTestCar(string id = "1")
        {
            return new Car
            {
                Id = id,
                Brand = "Tesla",
                Model = "Model 3",
                ProductionYear = 2024,
                FuelType = FuelCategory.Electric,
                Transmission = TransmissionCategory.Automatic,
                BodyType = BodyCategory.Sedan,
                EnginePowerKW = 150,
                Price = 45000,
                MileageKm = 1000,
                PrimaryImagePath = "image.jpg",
                Location = "Vilnius",
                ContactInfo = "info@example.com"
            };
        }

        // GetAllAsync Tests
        [Fact]
        public async Task GetAllAsync_WhenNoCarsExist_ReturnsEmpty()
        {
            var result = await _repository.GetAllAsync();
            Assert.Empty(result);
        }

        [Fact]
        public async Task GetAllAsync_WithMultipleCars_ReturnsAllCars()
        {
            var car1 = CreateTestCar("1");
            var car2 = CreateTestCar("2");
            await _repository.AddAsync(car1);
            await _repository.AddAsync(car2);

            var result = await _repository.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        // GetByIdAsync Tests
        [Fact]
        public async Task GetByIdAsync_WithValidId_ReturnsCar()
        {
            var car = CreateTestCar("1");
            await _repository.AddAsync(car);

            var result = await _repository.GetByIdAsync("1");

            Assert.NotNull(result);
            Assert.Equal("1", result.Id);
            Assert.Equal("Tesla", result.Brand);
        }

        [Fact]
        public async Task GetByIdAsync_WithInvalidId_ReturnsNull()
        {
            var result = await _repository.GetByIdAsync("nonexistent");
            Assert.Null(result);
        }

        // AddAsync Tests
        [Fact]
        public async Task AddAsync_AddsCarToRepository()
        {
            var car = CreateTestCar("1");

            await _repository.AddAsync(car);
            var result = await _repository.GetAllAsync();

            Assert.Single(result);
            Assert.Equal("1", result.First().Id);
        }

        [Fact]
        public async Task AddAsync_MultipleCars_IncrementsCount()
        {
            var car1 = CreateTestCar("1");
            var car2 = CreateTestCar("2");

            await _repository.AddAsync(car1);
            await _repository.AddAsync(car2);
            var result = await _repository.GetAllAsync();

            Assert.Equal(2, result.Count());
        }

        // UpdateAsync Tests
        [Fact]
        public async Task UpdateAsync_WithExistingId_ModifiesCar()
        {
            var car = CreateTestCar("1");
            await _repository.AddAsync(car);

            var updated = CreateTestCar("1");
            updated.Brand = "BMW";
            await _repository.UpdateAsync(updated);

            var result = await _repository.GetByIdAsync("1");
            Assert.Equal("BMW", result.Brand);
        }

        [Fact]
        public async Task UpdateAsync_WithNonExistingId_DoesNothing()
        {
            var car = CreateTestCar("nonexistent");
            await _repository.UpdateAsync(car);

            var result = await _repository.GetAllAsync();
            Assert.Empty(result);
        }

        // DeleteAsync Tests
        [Fact]
        public async Task DeleteAsync_WithExistingId_RemovesCar()
        {
            var car = CreateTestCar("1");
            await _repository.AddAsync(car);

            await _repository.DeleteAsync("1");
            var result = await _repository.GetAllAsync();

            Assert.Empty(result);
        }

        [Fact]
        public async Task DeleteAsync_WithNonExistingId_DoesNothing()
        {
            await _repository.DeleteAsync("nonexistent");
            var result = await _repository.GetAllAsync();

            Assert.Empty(result);
        }

        // ExistsAsync Tests
        [Fact]
        public async Task ExistsAsync_WithExistingId_ReturnsTrue()
        {
            var car = CreateTestCar("1");
            await _repository.AddAsync(car);

            var result = await _repository.ExistsAsync("1");

            Assert.True(result);
        }

        [Fact]
        public async Task ExistsAsync_WithNonExistingId_ReturnsFalse()
        {
            var result = await _repository.ExistsAsync("nonexistent");
            Assert.False(result);
        }
    }
}