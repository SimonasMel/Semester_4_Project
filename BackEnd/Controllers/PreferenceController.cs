using BackEnd.Data;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System.Security.Claims;

namespace BackEnd.Controllers
{
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class PreferencesController : ControllerBase
    {
        private readonly CarDbContext _db;

        public PreferencesController(CarDbContext db)
        {
            _db = db;
        }

        [HttpGet]
        public async Task<ActionResult<UserPreferences>> Get()
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var prefs = await _db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);
            if (prefs == null)
                return Ok(new UserPreferences()); // return empty prefs if none saved yet
            return Ok(prefs);
        }

        [HttpPost]
        public async Task<IActionResult> Save([FromBody] UserPreferences prefs)
        {
            var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
            var existing = await _db.UserPreferences.FirstOrDefaultAsync(p => p.UserId == userId);

            if (existing == null)
            {
                prefs.Id = 0;
                prefs.UserId = userId!;
                _db.UserPreferences.Add(prefs);
            }
            else
            {
                existing.PreferredBrand = prefs.PreferredBrand;
                existing.MinPrice = prefs.MinPrice;
                existing.MaxPrice = prefs.MaxPrice;
                existing.MinYear = prefs.MinYear;
                existing.MaxYear = prefs.MaxYear;
                existing.MaxMileageKm = prefs.MaxMileageKm;
                existing.MinEnginePowerKW = prefs.MinEnginePowerKW;
                existing.FuelType = prefs.FuelType;
                existing.Transmission = prefs.Transmission;
                existing.BodyType = prefs.BodyType;
            }

            await _db.SaveChangesAsync();
            return Ok();
        }
    }
}