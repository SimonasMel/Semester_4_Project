using System.Collections.Generic;
using System.Linq;
using System.Threading.Tasks;
using BackEnd.Controllers;
using BackEnd.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Models;
using Xunit;

namespace BackEnd.Tests.Controllers
{
    public class ParametrizedTests
    {
        private readonly Mock<ICarRepository> _repositoryMock;
        private readonly Mock<ILogger<CarsController>> _loggerMock;
        private readonly CarsController _controller;

        public ParametrizedTests()
        {
            _repositoryMock = new Mock<ICarRepository>();
            _loggerMock = new Mock<ILogger<CarsController>>();
            _controller = new CarsController(_repositoryMock.Object, _loggerMock.Object);
        }

        private static Car CreateTestCar(string id = "1") => new()
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

        [Theory]
        [InlineData("100")]
        [InlineData("nonexistent")]
        public async Task GetCarById_WhenCarDoesNotExist_ReturnsNotFound(string id)
        {
            // Arrange
            _repositoryMock.Setup(r => r.GetByIdAsync(id)).ReturnsAsync((Car?)null);

            // Act
            var result = await _controller.GetCarById(id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result.Result);
            _repositoryMock.Verify(r => r.GetByIdAsync(id), Times.Once);
        }

        [Theory]
        [InlineData(0)]
        [InlineData(1)]
        [InlineData(3)]
        public async Task GetAllCars_ReturnsExpectedNumberOfCars(int expectedCount)
        {
            // Arrange
            var cars = Enumerable.Range(1, expectedCount).Select(i => CreateTestCar(i.ToString())).ToArray();
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cars);

            // Act
            var result = await _controller.GetAllCars();

            // Assert
            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<Car>>(ok.Value);
            Assert.Equal(expectedCount, value.Count());
            _repositoryMock.Verify(r => r.GetAllAsync(), Times.Once);
        }

        [Theory]
        [InlineData(true)]
        [InlineData(false)]
        public async Task CreateCar_WhenCarIsNullOrModelInvalid_ReturnsBadRequest(bool isNull)
        {
            // Arrange
            Car? newCar = isNull ? null : CreateTestCar("1");
            if (!isNull)
            {
                // Simulate invalid model state
                _controller.ModelState.AddModelError("Brand", "Required");
            }

            // Act
            var result = await _controller.CreateCar(newCar!);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result.Result);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task UpdateCar_WhenIdIsInvalid_ReturnsBadRequest(string? id)
        {
            // Arrange
            var updated = CreateTestCar("1");

            // Act
            var result = await _controller.UpdateCar(id ?? string.Empty, updated);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
        }

        [Theory]
        [InlineData("10")]
        [InlineData("nonexistent")]
        public async Task UpdateCar_WhenCarDoesNotExist_ReturnsNotFound(string id)
        {
            // Arrange
            var updated = CreateTestCar(id);
            _repositoryMock.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _controller.UpdateCar(id, updated);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task DeleteCar_WhenIdIsInvalid_ReturnsBadRequest(string? id)
        {
            // Act
            var result = await _controller.DeleteCar(id ?? string.Empty);

            // Assert
            Assert.IsType<BadRequestObjectResult>(result);
            _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("10")]
        [InlineData("nonexistent")]
        public async Task DeleteCar_WhenCarDoesNotExist_ReturnsNotFound(string id)
        {
            // Arrange
            _repositoryMock.Setup(r => r.ExistsAsync(id)).ReturnsAsync(false);

            // Act
            var result = await _controller.DeleteCar(id);

            // Assert
            Assert.IsType<NotFoundObjectResult>(result);
            _repositoryMock.Verify(r => r.DeleteAsync(It.IsAny<string>()), Times.Never);
        }

        [Theory]
        [InlineData("5")]
        [InlineData("6")]
        public async Task CreateCar_WhenValid_ReturnsCreated(string id)
        {
            // Arrange
            var car = CreateTestCar(id);
            _repositoryMock.Setup(r => r.AddAsync(It.IsAny<Car>())).Returns(Task.CompletedTask);

            // Act
            var result = await _controller.CreateCar(car);

            // Assert
            var created = Assert.IsType<CreatedAtActionResult>(result.Result);
            var value = Assert.IsType<Car>(created.Value);
            Assert.Equal(id, value.Id);
            _repositoryMock.Verify(r => r.AddAsync(It.Is<Car>(c => c.Id == id)), Times.Once);
        }
    }
}
