using BackEnd.Models;
using BackEnd.Repositories;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Mvc;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize(Roles = "Admin")]
    public class AdminController : ControllerBase
    {
        private readonly UserManager<ApplicationUser> _userManager;
        private readonly ICarRepository _carRepository;
        private readonly ILogger<AdminController> _logger;

        public AdminController(
            UserManager<ApplicationUser> userManager,
            ICarRepository carRepository,
            ILogger<AdminController> logger)
        {
            _userManager = userManager;
            _carRepository = carRepository;
            _logger = logger;
        }

        // ─── USERS ───────────────────────────────────────────

        [HttpGet("users")]
        public IActionResult GetUsers()
        {
            var users = _userManager.Users
                .Select(u => new
                {
                    u.Id,
                    u.Email,
                    u.UserName
                })
                .ToList();

            return Ok(users);
        }

        [HttpDelete("users/{id}")]
        public async Task<IActionResult> DeleteUser(string id)
        {
            var user = await _userManager.FindByIdAsync(id);
            if (user is null)
                return NotFound(new { error = "User not found." });

            // Neleisti adminui ištrinti save patį
            var currentUserId = User.FindFirst(System.Security.Claims.ClaimTypes.NameIdentifier)?.Value;
            if (user.Id == currentUserId)
                return BadRequest(new { error = "You cannot delete your own account." });

            var result = await _userManager.DeleteAsync(user);
            if (!result.Succeeded)
            {
                var errors = result.Errors.Select(e => e.Description);
                return StatusCode(500, new { error = "Failed to delete user.", details = errors });
            }

            _logger.LogInformation("Admin deleted user {Email}", user.Email);
            return Ok(new { message = "User deleted successfully." });
        }

        // ─── CARS ────────────────────────────────────────────

        [HttpGet("cars")]
        public async Task<IActionResult> GetCars()
        {
            var cars = await _carRepository.GetAllAsync();
            return Ok(cars);
        }

        [HttpDelete("cars/{id}")]
        public async Task<IActionResult> DeleteCar(string id)
        {
            var car = await _carRepository.GetByIdAsync(id);
            if (car is null)
                return NotFound(new { error = "Car not found." });

            await _carRepository.DeleteAsync(id);
            _logger.LogInformation("Admin deleted car {Id}", id);
            return Ok(new { message = "Car deleted successfully." });
        }
    }
}