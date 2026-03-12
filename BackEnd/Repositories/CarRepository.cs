using CarApp.Models;

namespace CarApp.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly List<Car> _cars = new List<Car>();

        public async Task<IEnumerable<Car>> GetAllAsync()
            => await Task.FromResult(_cars);

        public async Task<Car?> GetByIdAsync(string id)
            => await Task.FromResult(_cars.FirstOrDefault(c => c.Id == id));

        public async Task AddAsync(Car car)
        {
            _cars.Add(car);
            await Task.CompletedTask;
        }

        public async Task UpdateAsync(Car car)
        {
            var index = _cars.FindIndex(c => c.Id == car.Id);
            if (index != -1)
                _cars[index] = car;
            await Task.CompletedTask;
        }

        public async Task DeleteAsync(string id)
        {
            var car = _cars.FirstOrDefault(c => c.Id == id);
            if (car != null)
                _cars.Remove(car);
            await Task.CompletedTask;
        }

        public async Task<bool> ExistsAsync(string id)
            => await Task.FromResult(_cars.Any(c => c.Id == id));
    }
}