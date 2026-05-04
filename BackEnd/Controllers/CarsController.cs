using BackEnd.Repositories;
using BackEnd.Services;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Shared.Models;
using System.Security.Claims;
using BackEnd.Data;
using Azure.Storage.Blobs;
using Azure.Storage.Blobs.Models;

namespace BackEnd.Controllers
{
    /// <summary>
    /// Manages Create, read, update, delete operations for car listings.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    [Authorize]
    public class CarsController : ControllerBase
    {
        private readonly ICarRepository _repository;
        private readonly ILogger<CarsController> _logger;
        private readonly HeicImageConverter _imageConverter;

        public sealed class LikeCarRequest
        {
            public string? OwnerId { get; set; }
        }

        /// <summary>
        /// Initializes a new instance of the <see cref="CarsController"/> class.
        /// </summary>
        /// <param name="repository">The car repository instance injected via dependency injection.</param>
        public CarsController(ICarRepository repository, ILogger<CarsController> logger, HeicImageConverter? imageConverter = null)
        {
            _repository = repository;
            _logger = logger;
            _imageConverter = imageConverter ?? new HeicImageConverter();
        }

        /// <summary>
        /// Retrieves all cars from the repository.
        /// </summary>
        /// <remarks>
        /// Returns a collection of all car listings currently stored in the database.
        /// In case of errors, returns appropriate HTTP status codes with error details.
        /// </remarks>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// Returns <see cref="OkResult"/> with a collection of <see cref="Car"/> objects on success.
        /// Returns <see cref="StatusCodeResult"/> 400 for invalid operations.
        /// Returns <see cref="StatusCodeResult"/> 500 for unexpected server errors.
        /// </returns>
        /// <response code="200">Returns the list of cars successfully.</response>
        /// <response code="400">Invalid operation occurred while retrieving cars.</response>
        /// <response code="500">An unexpected error occurred on the server.</response>
        [HttpGet]
        public async Task<ActionResult<IEnumerable<Car>>> GetAllCars()
        {
            try
            {
                return Ok(await _repository.GetAllAsync());
            }
            catch (System.InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while retrieving cars");
                return StatusCode(400, new { error = "Invalid operation", details = ex.Message });
            }
            catch (System.Exception ex)
            {
                _logger.LogError(ex, "Error retrieving cars");
                return StatusCode(500, new { error = "An error occurred while retrieving cars", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves cars belonging to the current user.
        /// </summary>
        [HttpGet("my")]
        public async Task<ActionResult<IEnumerable<Car>>> GetMyCars()
        {
            try
            {
                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrEmpty(userId))
                    return Unauthorized(new { error = "User not logged in" });

                return Ok(await _repository.GetUserCarsAsync(userId));
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving user's cars");
                return StatusCode(500, new { error = "An error occurred while retrieving your cars", details = ex.Message });
            }
        }

        /// <summary>
        /// Retrieves a specific car by its unique identifier.
        /// </summary>
        /// <remarks>
        /// Fetches a single car record from the database using the provided car ID.
        /// The ID must be a valid non-empty string. Returns a 404 error if the car is not found.
        /// </remarks>
        /// <param name="id">The unique identifier of the car to retrieve.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// Returns <see cref="OkResult"/> with the <see cref="Car"/> object on success.
        /// Returns <see cref="BadRequestResult"/> if ID is empty or null.
        /// Returns <see cref="NotFoundResult"/> if no car with the specified ID exists.
        /// Returns <see cref="StatusCodeResult"/> 400 for invalid operations.
        /// Returns <see cref="StatusCodeResult"/> 500 for unexpected server errors.
        /// </returns>
        /// <response code="200">Returns the car with the specified ID.</response>
        /// <response code="400">ID is empty or invalid operation occurred.</response>
        /// <response code="404">Car with the specified ID was not found.</response>
        /// <response code="500">An unexpected error occurred on the server.</response>
        [HttpGet("{id}")]
        public async Task<ActionResult<Car>> GetCarById(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(new { error = "ID cannot be empty" });

                var car = await _repository.GetByIdAsync(id);
                if (car == null)
                    return NotFound(new { error = $"Car with ID {id} not found" });

                return Ok(car);
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while retrieving car {CarId}", id);
                return StatusCode(400, new { error = "Invalid operation", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error retrieving car {CarId}", id);
                return StatusCode(500, new { error = "An error occurred while retrieving the car", details = ex.Message });
            }
        }

        /// <summary>
        /// Creates a new car record in the system.
        /// </summary>
        /// <remarks>
        /// Accepts a new <see cref="Car"/> object and adds it to the database.
        /// The car must pass all model validation rules defined in the Car class.
        /// On successful creation, returns a 201 Created response with the new car object and its location.
        /// </remarks>
        /// <param name="newCar">The car object containing the details to be created.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// Returns <see cref="CreatedAtActionResult"/> with the newly created <see cref="Car"/> on success.
        /// Returns <see cref="BadRequestResult"/> if the car data is null, empty, or fails validation.
        /// Returns <see cref="StatusCodeResult"/> 400 for invalid car data.
        /// Returns <see cref="StatusCodeResult"/> 500 for unexpected server errors.
        /// </returns>
        /// <response code="201">Car created successfully. Returns the created car object</response>
        /// <response code="400">Car data is required, invalid, or validation failed.</response>
        /// <response code="500">An unexpected error occurred on the server.</response>
        [HttpPost]
        public async Task<ActionResult<Car>> CreateCar([FromBody] Car newCar)
        {
            try
            {
                if (newCar == null)
                    return BadRequest(new { error = "Car data is required" });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).ToList();
                    _logger.LogWarning("Validation errors while creating car: {Errors}", string.Join(", ", errors));
                    return BadRequest(new { error = "Validation failed", details = errors });
                }

                newCar.UserId = User.FindFirstValue(ClaimTypes.NameIdentifier) ?? "default";

                await _repository.AddAsync(newCar);
                return CreatedAtAction(nameof(GetCarById), new { id = newCar.Id }, newCar);
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid car data while creating car");
                return StatusCode(400, new { error = "Invalid car data", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error creating car");
                return StatusCode(500, new { error = "An error occurred while creating the car", details = ex.Message });
            }
        }

        /// <summary>
        /// Updates an existing car record.
        /// </summary>
        /// <remarks>
        /// Modifies an existing car record identified by the provided ID.
        /// The car must exist in the database and the updated data must pass all validation rules.
        /// Returns 204 No Content on successful update, indicating the operation completed without response body.
        /// </remarks>
        /// <param name="id">The unique identifier of the car to update.</param>
        /// <param name="updatedCar">The car object containing the updated details.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// Returns <see cref="NoContentResult"/> on successful update.
        /// Returns <see cref="BadRequestResult"/> if ID is empty or car data is null/invalid.
        /// Returns <see cref="NotFoundResult"/> if no car with the specified ID exists.
        /// Returns <see cref="StatusCodeResult"/> 400 for invalid car data.
        /// Returns <see cref="StatusCodeResult"/> 500 for unexpected server errors.
        /// </returns>
        /// <response code="204">Car updated successfully. No content returned.</response>
        /// <response code="400">ID is empty, car data is invalid, or validation failed.</response>
        /// <response code="404">Car with the specified ID was not found.</response>
        /// <response code="500">An unexpected error occurred on the server.</response>
        [HttpPut("{id}")]
        public async Task<IActionResult> UpdateCar(string id, [FromBody] Car updatedCar)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(new { error = "ID cannot be empty" });

                if (updatedCar == null)
                    return BadRequest(new { error = "Car data is required" });

                var existingCar = await _repository.GetByIdAsync(id);
                if (existingCar == null)
                    return NotFound(new { error = $"Car with ID {id} not found" });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (existingCar.UserId != userId && userId != null)
                    return Forbid();

                updatedCar.Id = id;
                updatedCar.UserId = existingCar.UserId; // Preserve the original owner
                await _repository.UpdateAsync(updatedCar);

                return NoContent();
            }
            catch (ArgumentException ex)
            {
                _logger.LogError(ex, "Invalid car data while updating car {CarId}", id);
                return StatusCode(400, new { error = "Invalid car data", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error updating car {CarId}", id);
                return StatusCode(500, new { error = "An error occurred while updating the car", details = ex.Message });
            }
        }

        /// <summary>
        /// Deletes an existing car record from the system.
        /// </summary>
        /// <remarks>
        /// Removes a car record from the database. The car must exist before deletion.
        /// Once deleted, the record cannot be recovered. Returns 204 No Content on successful deletion.
        /// </remarks>
        /// <param name="id">The unique identifier of the car to delete.</param>
        /// <returns>
        /// A <see cref="Task{TResult}"/> representing the asynchronous operation.
        /// Returns <see cref="NoContentResult"/> on successful deletion.
        /// Returns <see cref="BadRequestResult"/> if ID is empty.
        /// Returns <see cref="NotFoundResult"/> if no car with the specified ID exists.
        /// Returns <see cref="StatusCodeResult"/> 400 for invalid operations.
        /// Returns <see cref="StatusCodeResult"/> 500 for unexpected server errors.
        /// </returns>
        /// <response code="204">Car deleted successfully. No content returned.</response>
        /// <response code="400">ID is empty or invalid operation occurred.</response>
        /// <response code="404">Car with the specified ID was not found.</response>
        /// <response code="500">An unexpected error occurred on the server.</response>
        [HttpDelete("{id}")]
        public async Task<IActionResult> DeleteCar(string id)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(id))
                    return BadRequest(new { error = "ID cannot be empty" });

                var car = await _repository.GetByIdAsync(id);
                if (car == null)
                    return NotFound(new { error = $"Car with ID {id} not found" });

                var userId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (car.UserId != userId && userId != null)
                    return Forbid();

                await _repository.DeleteAsync(id);
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                _logger.LogError(ex, "Invalid operation while deleting car {CarId}", id);
                return StatusCode(400, new { error = "Cannot delete car", details = ex.Message });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error deleting car {CarId}", id);
                return StatusCode(500, new { error = "An error occurred while deleting the car", details = ex.Message });
            }
        }

        /// <summary>
        /// Uploads images for a car.
        /// </summary>
        [HttpPost("upload")]
        [AllowAnonymous]
        public async Task<IActionResult> UploadImages(
    IFormFileCollection files,
    [FromServices] IConfiguration configuration)
        {
            try
            {
                if (files == null || files.Count == 0)
                    return BadRequest(new { error = "No files uploaded" });

                var connectionString = configuration["AzureStorage:ConnectionString"];
                var containerName = configuration["AzureStorage:ContainerName"] ?? "car-images";

                // Fall back to local storage if no Azure config (for local development)
                if (string.IsNullOrEmpty(connectionString))
                {
                    var uploadedLocalUrls = new List<string>();
                    var uploadsFolder = Path.Combine(Directory.GetCurrentDirectory(), "wwwroot", "images");
                    if (!Directory.Exists(uploadsFolder))
                        Directory.CreateDirectory(uploadsFolder);

                    foreach (var file in files)
                    {
                        if (file.Length > 0)
                        {
                            var (stream, fileName, contentType) = await _imageConverter.PrepareForUploadAsync(file);
                            using (stream)
                            {
                                var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                                var filePath = Path.Combine(uploadsFolder, uniqueFileName);
                                await using var output = new FileStream(filePath, FileMode.Create);
                                await stream.CopyToAsync(output);
                                uploadedLocalUrls.Add($"images/{uniqueFileName}");
                            }
                        }
                    }
                    return Ok(new { urls = uploadedLocalUrls });
                }

                // Azure Blob Storage upload
                var blobServiceClient = new BlobServiceClient(connectionString);
                var containerClient = blobServiceClient.GetBlobContainerClient(containerName);
                await containerClient.CreateIfNotExistsAsync(PublicAccessType.Blob);

                var uploadedUrls = new List<string>();

                foreach (var file in files)
                {
                    if (file.Length > 0)
                    {
                        var (stream, fileName, contentType) = await _imageConverter.PrepareForUploadAsync(file);
                        using (stream)
                        {
                            var uniqueFileName = Guid.NewGuid().ToString() + "_" + fileName;
                            var blobClient = containerClient.GetBlobClient(uniqueFileName);

                            await blobClient.UploadAsync(stream, new BlobHttpHeaders
                            {
                                ContentType = contentType
                            });

                            // Full HTTPS URL — permanent, survives restarts
                            uploadedUrls.Add(blobClient.Uri.ToString());
                        }
                    }
                }

                return Ok(new { urls = uploadedUrls });
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error uploading images");
                return StatusCode(500, new { error = "An error occurred while uploading images", details = ex.Message });
            }
        }

        /// <summary>
        /// Likes a car and creates a mutual match when both users liked each other's cars.
        /// </summary>
        [HttpPost("{carId}/like")]
        public async Task<ActionResult<MutualMatch>> LikeCar(
            string carId,
            [FromBody] LikeCarRequest? request,
            [FromServices] CarDbContext db)
        {
            try
            {
                if (string.IsNullOrWhiteSpace(carId))
                    return BadRequest(new { error = "Car ID cannot be empty" });

                var currentUserId = User.FindFirstValue(ClaimTypes.NameIdentifier);
                if (string.IsNullOrWhiteSpace(currentUserId))
                    return Unauthorized(new { error = "User not logged in" });

                var car = await _repository.GetByIdAsync(carId);
                if (car == null)
                    return NotFound(new { error = $"Car with ID {carId} not found" });

                var carOwnerId = car.UserId;
                if (string.IsNullOrWhiteSpace(carOwnerId))
                    return BadRequest(new { error = "Car owner is missing" });

                if (carOwnerId == currentUserId)
                    return BadRequest(new { error = "You cannot like your own car" });

                var existingLike = await db.CarLikes
                    .FirstOrDefaultAsync(l => l.LikerUserId == currentUserId && l.LikedCarId == carId);

                if (existingLike == null)
                {
                    db.CarLikes.Add(new CarLike
                    {
                        LikerUserId = currentUserId,
                        LikedCarId = carId,
                        LikedCarOwnerId = carOwnerId,
                        CreatedAt = DateTime.UtcNow
                    });

                    await db.SaveChangesAsync();
                }

                var reverseLike = await db.CarLikes
                    .Where(l => l.LikerUserId == carOwnerId && l.LikedCarOwnerId == currentUserId)
                    .OrderByDescending(l => l.CreatedAt)
                    .FirstOrDefaultAsync();

                if (reverseLike == null)
                    return NoContent();

                var existingMatch = await db.MutualMatches
                    .FirstOrDefaultAsync(m => m.IsActive &&
                        ((m.CurrentUserId == currentUserId && m.MatchedUserId == carOwnerId) ||
                         (m.CurrentUserId == carOwnerId && m.MatchedUserId == currentUserId)));

                if (existingMatch != null)
                    return Ok(existingMatch);

                var newMatch = new MutualMatch
                {
                    CurrentUserId = currentUserId,
                    MatchedUserId = carOwnerId,
                    CurrentUserCarId = reverseLike.LikedCarId,
                    MatchedUserCarId = carId,
                    MatchedDate = DateTime.UtcNow,
                    IsActive = true
                };

                db.MutualMatches.Add(newMatch);
                await db.SaveChangesAsync();

                return Ok(newMatch);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error liking car {CarId}. Request ownerId: {OwnerId}", carId, request?.OwnerId);
                return StatusCode(500, new { error = "An error occurred while liking the car", details = ex.Message });
            }
        }
    }
}
