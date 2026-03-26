using BackEnd.Repositories;
using Shared.Models;
using Xunit;

namespace BackEnd.Tests.Fixtures
{
    /// <summary>
    /// Fixture for CarRepository tests with proper setup and teardown
    /// Implements IAsyncLifetime for async initialization and cleanup
    /// </summary>
    public class CarRepositoryFixture : IAsyncLifetime
    {
        public CarRepository Repository { get; private set; } = null!;
        public List<Car> TestCars { get; private set; } = new();

        /// <summary>
        /// InitializeAsync is called before each test (Setup phase)
        /// </summary>
        public async Task InitializeAsync()
        {
            // Create fresh repository instance for each test
            Repository = new CarRepository();

            // Pre-populate with test data
            TestCars = new List<Car>
            {
                CreateTestCar("1", "Tesla", "Model 3"),
                CreateTestCar("2", "BMW", "X5"),
                CreateTestCar("3", "Audi", "A4")
            };

            // Seed the repository
            foreach (var car in TestCars)
            {
                await Repository.AddAsync(car);
            }
        }

        /// <summary>
        /// DisposeAsync is called after each test (Teardown phase)
        /// Clean up resources if needed
        /// </summary>
        public async Task DisposeAsync()
        {
            // Clear the repository to ensure test isolation
            // Get all cars first, then delete to avoid collection modification exception
            var allCars = (await Repository.GetAllAsync()).ToList();
            foreach (var car in allCars)
            {
                await Repository.DeleteAsync(car.Id);
            }

            TestCars.Clear();
        }

        public Car CreateTestCar(string id, string brand = "Tesla", string model = "Model 3")
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
    /// Collection definition for sharing fixtures across tests
    /// </summary>
    [CollectionDefinition("Car Repository Collection")]
    public class CarRepositoryCollection : ICollectionFixture<CarRepositoryFixture>
    {
        // This class has no code, it just defines the collection
    }
}
