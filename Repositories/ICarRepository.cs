using System.Collections.Generic;
using System.Threading.Tasks;
using CarApp.Models;

namespace CarApp.Repositories
{
    /// <summary>
    /// Defines the contract for data access operations on car entities.
    /// </summary>
    /// <remarks>
    public interface ICarRepository
    {
        Task<IEnumerable<Car>> GetAllAsync();
        Task<Car> GetByIdAsync(string id);
        Task AddAsync(Car car);
        Task UpdateAsync(Car car);
        Task DeleteAsync(string id);
        Task<bool> ExistsAsync(string id);
    }
}