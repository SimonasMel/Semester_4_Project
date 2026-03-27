using Microsoft.EntityFrameworkCore;
using Shared.Models;
using BackEnd.Data;

namespace BackEnd.Repositories
{
    public class CarRepository : ICarRepository
    {
        private readonly CarDbContext _context;

        public CarRepository(CarDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<Car>> GetAllAsync()
            => await _context.Cars.ToListAsync();

        public async Task<Car?> GetByIdAsync(string id)
            => await _context.Cars.FirstOrDefaultAsync(c => c.Id == id);

        public async Task AddAsync(Car car)
        {
            _context.Cars.Add(car);
            await _context.SaveChangesAsync();
        }

        public async Task UpdateAsync(Car car)
        {
            var existing = await _context.Cars.FirstOrDefaultAsync(c => c.Id == car.Id);
            if (existing != null)
            {
                _context.Entry(existing).CurrentValues.SetValues(car);
                await _context.SaveChangesAsync();
            }
        }

        public async Task DeleteAsync(string id)
        {
            var car = await GetByIdAsync(id);
            if (car != null)
            {
                _context.Cars.Remove(car);
                await _context.SaveChangesAsync();
            }
        }

        public async Task<bool> ExistsAsync(string id)
            => await _context.Cars.AnyAsync(c => c.Id == id);
    }
}