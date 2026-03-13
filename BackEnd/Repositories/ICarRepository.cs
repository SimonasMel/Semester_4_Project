using CarApp.Models;

namespace CarApp.Repositories
{
    public interface ICarRepository
    {
        Task<IEnumerable<Car>> GetAllAsync();
        Task<Car?> GetByIdAsync(string id);
        Task AddAsync(Car car);
        Task UpdateAsync(Car car);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}