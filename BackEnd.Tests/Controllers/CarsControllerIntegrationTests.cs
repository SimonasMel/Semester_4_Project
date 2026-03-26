using System.Net;
using System.Net.Http.Json;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using BackEnd.Repositories;
using Shared.Models;
using Xunit;

namespace BackEnd.Tests.Integration
{
    /// <summary>
    /// Integration tests for CarsController.
    /// Each test gets its own fresh in-memory API so they never interfere with each other.
    /// No mocks — the real Controller, Repository, routing and JSON serialization are all tested together.
    /// </summary>
    public class CarsControllerIntegrationTests
    {
        // Creates a brand new in-memory API for each test — completely isolated, no shared state
        private static HttpClient CreateFreshClient()
        {
            var factory = new WebApplicationFactory<Program>()
                .WithWebHostBuilder(builder =>
                {
                    builder.ConfigureServices(services =>
                    {
                        // Remove the scoped repository registered in Program.cs
                        var descriptor = services.SingleOrDefault(
                            d => d.ServiceType == typeof(ICarRepository));
                        if (descriptor != null)
                            services.Remove(descriptor);

                        // Register a fresh empty repository for this test only
                        services.AddSingleton<ICarRepository, CarRepository>();
                    });
                });

            return factory.CreateClient();
        }

        // Helper: creates a valid Car object for use in tests
        private static Car CreateTestCar(string id = "test-1") => new()
        {
            Id = id,
            Brand = "Toyota",
            Model = "Corolla",
            ProductionYear = 2022,
            FuelType = FuelCategory.Petrol,
            Transmission = TransmissionCategory.Automatic,
            BodyType = BodyCategory.Sedan,
            EnginePowerKW = 100,
            Price = 20000,
            MileageKm = 15000,
            PrimaryImagePath = "corolla.jpg",
            Location = "Vilnius",
            ContactInfo = "test@example.com",
            Description = "A reliable family car in great condition.",
            VIN = "1HGBH41JXMN109186"
        };

        // GET /api/cars

        [Fact]
        public async Task GetAllCars_WhenNoCarsExist_ReturnsOkWithEmptyList()
        {
            var client = CreateFreshClient();

            var response = await client.GetAsync("/api/cars");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var cars = await response.Content.ReadFromJsonAsync<List<Car>>();
            Assert.NotNull(cars);
            Assert.Empty(cars);
        }

        [Fact]
        public async Task GetAllCars_AfterAddingCars_ReturnsAllCars()
        {
            var client = CreateFreshClient();

            var car1 = CreateTestCar("all-1");
            var car2 = CreateTestCar("all-2");
            car2.Brand = "Honda";

            await client.PostAsJsonAsync("/api/cars", car1);
            await client.PostAsJsonAsync("/api/cars", car2);

            var response = await client.GetAsync("/api/cars");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var cars = await response.Content.ReadFromJsonAsync<List<Car>>();
            Assert.NotNull(cars);
            Assert.Contains(cars, c => c.Id == "all-1");
            Assert.Contains(cars, c => c.Id == "all-2");
        }

        // GET /api/cars/{id}

        [Fact]
        public async Task GetCarById_WhenCarExists_ReturnsOkWithCar()
        {
            var client = CreateFreshClient();

            var car = CreateTestCar("get-1");
            await client.PostAsJsonAsync("/api/cars", car);

            var response = await client.GetAsync("/api/cars/get-1");

            Assert.Equal(HttpStatusCode.OK, response.StatusCode);
            var result = await response.Content.ReadFromJsonAsync<Car>();
            Assert.NotNull(result);
            Assert.Equal("get-1", result.Id);
            Assert.Equal("Toyota", result.Brand);
        }

        [Fact]
        public async Task GetCarById_WhenCarDoesNotExist_ReturnsNotFound()
        {
            var client = CreateFreshClient();

            var response = await client.GetAsync("/api/cars/does-not-exist");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // POST /api/cars

        [Fact]
        public async Task CreateCar_WithValidData_ReturnsCreatedAndCarIsRetrievable()
        {
            var client = CreateFreshClient();

            var car = CreateTestCar("create-1");

            var postResponse = await client.PostAsJsonAsync("/api/cars", car);
            Assert.Equal(HttpStatusCode.Created, postResponse.StatusCode);

            var getResponse = await client.GetAsync("/api/cars/create-1");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);

            var result = await getResponse.Content.ReadFromJsonAsync<Car>();
            Assert.NotNull(result);
            Assert.Equal("Toyota", result.Brand);
            Assert.Equal("Corolla", result.Model);
        }

        [Fact]
        public async Task CreateCar_WithMissingRequiredFields_ReturnsBadRequest()
        {
            var client = CreateFreshClient();

            var invalidCar = new Car
            {
                Id = "invalid-1",
                Brand = "",           // violates [Required]
                Model = "Corolla",
                ProductionYear = 2022,
                FuelType = FuelCategory.Petrol,
                Transmission = TransmissionCategory.Automatic,
                BodyType = BodyCategory.Sedan,
                EnginePowerKW = 100,
                Price = 20000,
                MileageKm = 15000,
                PrimaryImagePath = "img.jpg",
                Location = "",        // violates [Required]
                ContactInfo = ""      // violates [Required]
            };

            var response = await client.PostAsJsonAsync("/api/cars", invalidCar);

            Assert.Equal(HttpStatusCode.BadRequest, response.StatusCode);
        }

        // PUT /api/cars/{id}

        [Fact]
        public async Task UpdateCar_WhenCarExists_ReturnsNoContentAndUpdatesData()
        {
            var client = CreateFreshClient();

            var car = CreateTestCar("update-1");
            await client.PostAsJsonAsync("/api/cars", car);

            car.Brand = "Mazda";
            car.Price = 18000;

            var putResponse = await client.PutAsJsonAsync("/api/cars/update-1", car);
            Assert.Equal(HttpStatusCode.NoContent, putResponse.StatusCode);

            var getResponse = await client.GetAsync("/api/cars/update-1");
            var updated = await getResponse.Content.ReadFromJsonAsync<Car>();
            Assert.NotNull(updated);
            Assert.Equal("Mazda", updated.Brand);
            Assert.Equal(18000, updated.Price);
        }

        [Fact]
        public async Task UpdateCar_WhenCarDoesNotExist_ReturnsNotFound()
        {
            var client = CreateFreshClient();

            var car = CreateTestCar("ghost-1");

            var response = await client.PutAsJsonAsync("/api/cars/ghost-1", car);

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // DELETE /api/cars/{id}

        [Fact]
        public async Task DeleteCar_WhenCarExists_ReturnsNoContentAndCarIsGone()
        {
            var client = CreateFreshClient();

            var car = CreateTestCar("delete-1");
            await client.PostAsJsonAsync("/api/cars", car);

            var deleteResponse = await client.DeleteAsync("/api/cars/delete-1");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            var getResponse = await client.GetAsync("/api/cars/delete-1");
            Assert.Equal(HttpStatusCode.NotFound, getResponse.StatusCode);
        }

        [Fact]
        public async Task DeleteCar_WhenCarDoesNotExist_ReturnsNotFound()
        {
            var client = CreateFreshClient();

            var response = await client.DeleteAsync("/api/cars/never-existed");

            Assert.Equal(HttpStatusCode.NotFound, response.StatusCode);
        }

        // Full lifecycle test

        [Fact]
        public async Task FullLifecycle_CreateReadUpdateDelete_WorksCorrectly()
        {
            var client = CreateFreshClient();

            var car = CreateTestCar("lifecycle-1");

            // 1. CREATE
            var createResponse = await client.PostAsJsonAsync("/api/cars", car);
            Assert.Equal(HttpStatusCode.Created, createResponse.StatusCode);

            // 2. READ
            var getResponse = await client.GetAsync("/api/cars/lifecycle-1");
            Assert.Equal(HttpStatusCode.OK, getResponse.StatusCode);
            var fetched = await getResponse.Content.ReadFromJsonAsync<Car>();
            Assert.Equal("Toyota", fetched!.Brand);

            // 3. UPDATE
            car.Brand = "Nissan";
            var updateResponse = await client.PutAsJsonAsync("/api/cars/lifecycle-1", car);
            Assert.Equal(HttpStatusCode.NoContent, updateResponse.StatusCode);

            // 4. Verify update
            var getUpdated = await client.GetAsync("/api/cars/lifecycle-1");
            var updated = await getUpdated.Content.ReadFromJsonAsync<Car>();
            Assert.Equal("Nissan", updated!.Brand);

            // 5. DELETE
            var deleteResponse = await client.DeleteAsync("/api/cars/lifecycle-1");
            Assert.Equal(HttpStatusCode.NoContent, deleteResponse.StatusCode);

            // 6. Verify deletion
            var getDeleted = await client.GetAsync("/api/cars/lifecycle-1");
            Assert.Equal(HttpStatusCode.NotFound, getDeleted.StatusCode);
        }
        
       
    }

}
