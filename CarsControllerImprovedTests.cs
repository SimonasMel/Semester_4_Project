using BackEnd.Controllers;
using BackEnd.Repositories;
using Microsoft.AspNetCore.Mvc;
using Moq;
using Shared.Models;
using Xunit;

namespace BackEnd.Tests.Controllers
{
    /// <summary>
    /// Improved CarsController tests with proper fixture setup and teardown
    /// Uses constructor injection for IDisposable pattern
    /// </summary>
    public class CarsControllerFixture : IDisposable
    {
        public Mock<ICarRepository> RepositoryMock { get; }
        public CarsController Controller { get; }

        public CarsControllerFixture()
        {
            // Setup phase - initialize mocks and dependencies
            RepositoryMock = new Mock<ICarRepository>();
            Controller = new CarsController(RepositoryMock.Object);
        }

        public void Dispose()
            {
                // Teardown phase - clean up resources if needed
                // Mock objects don't need explicit disposal in Moq 4.x
                GC.SuppressFinalize(this);
            }

        public Car CreateTestCar(string id = "1", string brand = "Tesla", string model = "Model 3")
        {
            return new Car
            {
                Id = id,
                Brand = brand,
                Model = model,
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
    }

    /// <summary>
    /// Controller tests using fixture with proper setup/teardown phases
    /// Each test method receives a fresh fixture through constructor injection
    /// </summary>
    public class CarsControllerImprovedTests : IDisposable
    {
        private readonly CarsControllerFixture _fixture;

        public CarsControllerImprovedTests()
        {
            // Setup phase
            _fixture = new CarsControllerFixture();
        }

        public void Dispose()
        {
            // Teardown phase
            _fixture?.Dispose();
        }

        // ==================== GetAllCars Tests ====================

        [Fact]
        public async Task GetAllCars_WhenRepositoryReturnsCars_ReturnsOkWithData()
        {
            // Arrange - Setup phase for this test
            var cars = new[] 
            { 
                _fixture.CreateTestCar("1", "Tesla", "Model 3"), 
                _fixture.CreateTestCar("2", "BMW", "X5") 
            };
            _fixture.RepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cars);

            // Act
            var result = await _fixture.Controller.GetAllCars();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<Car>>(ok.Value);
            Assert.Equal(2, value.Count());

            // Verify the mock was called
            _fixture.RepositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Fact]
        public async Task GetAllCars_WhenRepositoryReturnsEmpty_ReturnsOkWithEmptyList()
        {
            // Arrange
            _fixture.RepositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(Array.Empty<Car>());

            // Act
            var result = await _fixture.Controller.GetAllCars();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<Car>>(ok.Value);
            Assert.Empty(value);
        }

        // ==================== GetCarById Tests ====================

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetCarById_WhenIdIsInvalid_ReturnsBadRequest(string? id)
        {
            // Arrange - No setup needed for invalid input test

            // Act
            var result = await _fixture.Controller.GetCarById(id!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);

            // Verify repository was never called with invalid ID
            _fixture.RepositoryMock.Verify(
                r => r.GetByIdAsync(It.IsAny<string>()), 
                Times.Never
            );
        }

        [Fact]
        public async Task GetCarById_WhenCarExists_ReturnsOkWithCar()
        {
            // Arrange
            var car = _fixture.CreateTestCar("1");
            _fixture.RepositoryMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(car);

            // Act
            var result = await _fixture.Controller.GetCarById("1");

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<Car>(ok.Value);
            Assert.Equal("1", value.Id);
            Assert.Equal("Tesla", value.Brand);

            // Verify repository call
            _fixture.RepositoryMock.Verify(r => r.GetByIdAsync("1"), Times.Once);
        }

        [Fact]
        public async Task GetCarById_WhenCarDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _fixture.RepositoryMock.Setup(r => r.GetByIdAsync(It.IsAny<string>()))
                .ReturnsAsync((Car?)null);

            // Act
            var result = await _fixture.Controller.GetCarById("nonexistent");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
        }

        // ==================== CreateCar Tests ====================

        [Fact]
        public async Task CreateCar_WhenCarIsNull_ReturnsBadRequest()
        {
            // Arrange - No mock setup needed for null input

            // Act
            var result = await _fixture.Controller.CreateCar(null!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);

            // Verify repository was never called
            _fixture.RepositoryMock.Verify(
                r => r.AddAsync(It.IsAny<Car>()), 
                Times.Never
            );
        }

        [Fact]
        public async Task CreateCar_WithValidCar_CallsRepositoryAddAsync()
        {
            // Arrange
            var newCar = _fixture.CreateTestCar("100");
            _fixture.RepositoryMock.Setup(r => r.AddAsync(newCar))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _fixture.Controller.CreateCar(newCar);

            // Assert
            // Verify the repository was called with the correct car
            _fixture.RepositoryMock.Verify(r => r.AddAsync(newCar), Times.Once);
        }

        // ==================== UpdateCar Tests ====================

        [Fact]
        public async Task UpdateCar_WhenCarDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            var updated = _fixture.CreateTestCar("10");
            _fixture.RepositoryMock.Setup(r => r.ExistsAsync("10")).ReturnsAsync(false);

            // Act
            var result = await _fixture.Controller.UpdateCar("10", updated);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);

            // Verify update was never called
            _fixture.RepositoryMock.Verify(
                r => r.UpdateAsync(It.IsAny<Car>()), 
                Times.Never
            );
        }

        [Fact]
        public async Task UpdateCar_WhenCarExists_UpdatesAndReturnsNoContent()
        {
            // Arrange
            var updatedCar = _fixture.CreateTestCar("1");
            updatedCar.Price = 50000;

            _fixture.RepositoryMock.Setup(r => r.ExistsAsync("1")).ReturnsAsync(true);
            _fixture.RepositoryMock.Setup(r => r.UpdateAsync(updatedCar))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _fixture.Controller.UpdateCar("1", updatedCar);

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify update was called
            _fixture.RepositoryMock.Verify(r => r.UpdateAsync(updatedCar), Times.Once);
        }

        // ==================== DeleteCar Tests ====================

        [Fact]
        public async Task DeleteCar_WhenCarExists_DeletesAndReturnsNoContent()
        {
            // Arrange
            _fixture.RepositoryMock.Setup(r => r.ExistsAsync("1")).ReturnsAsync(true);
            _fixture.RepositoryMock.Setup(r => r.DeleteAsync("1"))
                .Returns(Task.CompletedTask);

            // Act
            var result = await _fixture.Controller.DeleteCar("1");

            // Assert
            Assert.IsType<NoContentResult>(result);

            // Verify delete was called exactly once
            _fixture.RepositoryMock.Verify(r => r.DeleteAsync("1"), Times.Once);
        }

        [Fact]
        public async Task DeleteCar_WhenCarDoesNotExist_ReturnsNotFound()
        {
            // Arrange
            _fixture.RepositoryMock.Setup(r => r.ExistsAsync("999")).ReturnsAsync(false);

            // Act
            var result = await _fixture.Controller.DeleteCar("999");

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);

            // Verify delete was never called
            _fixture.RepositoryMock.Verify(
                r => r.DeleteAsync(It.IsAny<string>()), 
                Times.Never
            );
        }
    }
}
