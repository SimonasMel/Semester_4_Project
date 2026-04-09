using BackEnd.Controllers;
using BackEnd.Repositories;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Moq;
using Shared.Models;
using Xunit;

namespace BackEnd.Tests.Controllers
{
    public class CarsControllerTests
    {
        private readonly Mock<ICarRepository> _repositoryMock;
        private readonly Mock<ILogger<CarsController>> _loggerMock;
        private readonly CarsController _controller;

        public CarsControllerTests()
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

        [Fact]
        public async Task GetAllCars_WhenRepositoryReturnsCars_ReturnsOk()
        {
            var cars = new[] { CreateTestCar("1"), CreateTestCar("2") };
            _repositoryMock.Setup(r => r.GetAllAsync()).ReturnsAsync(cars);

            var result = await _controller.GetAllCars();

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsAssignableFrom<IEnumerable<Car>>(ok.Value);
            Assert.Equal(2, value.Count());
        }

        [Theory]
        [InlineData(null)]
        [InlineData("")]
        [InlineData(" ")]
        public async Task GetCarById_WhenIdIsInvalid_ReturnsBadRequest(string? id)
        {
            var result = await _controller.GetCarById(id!);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            _repositoryMock.Verify(r => r.GetByIdAsync(It.IsAny<string>()), Times.Never);
        }

        [Fact]
        public async Task GetCarById_WhenCarExists_ReturnsOk()
        {
            var car = CreateTestCar("1");
            _repositoryMock.Setup(r => r.GetByIdAsync("1")).ReturnsAsync(car);

            var result = await _controller.GetCarById("1");

            var ok = Assert.IsType<OkObjectResult>(result.Result);
            var value = Assert.IsType<Car>(ok.Value);
            Assert.Equal("1", value.Id);
        }

        [Fact]
        public async Task CreateCar_WhenCarIsNull_ReturnsBadRequest()
        {
            var result = await _controller.CreateCar(null!);

            Assert.IsType<BadRequestObjectResult>(result.Result);
            _repositoryMock.Verify(r => r.AddAsync(It.IsAny<Car>()), Times.Never);
        }

        [Fact]
        public async Task UpdateCar_WhenCarDoesNotExist_ReturnsNotFound()
        {
            var updated = CreateTestCar("10");
            _repositoryMock.Setup(r => r.ExistsAsync("10")).ReturnsAsync(false);

            var result = await _controller.UpdateCar("10", updated);

            Assert.IsType<NotFoundObjectResult>(result);
            _repositoryMock.Verify(r => r.UpdateAsync(It.IsAny<Car>()), Times.Never);
        }

        [Fact]
        public async Task DeleteCar_WhenCarExists_DeletesAndReturnsNoContent()
        {
            _repositoryMock.Setup(r => r.ExistsAsync("1")).ReturnsAsync(true);
            _repositoryMock.Setup(r => r.DeleteAsync("1")).Returns(Task.CompletedTask);

            var result = await _controller.DeleteCar("1");

            Assert.IsType<NoContentResult>(result);
            _repositoryMock.Verify(r => r.DeleteAsync("1"), Times.Once);
        }
    }
}