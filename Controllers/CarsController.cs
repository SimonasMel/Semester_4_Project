using Microsoft.AspNetCore.Mvc;
using System.Collections.Generic;
using System.Linq;
using CarApp.Models;
using CarApp.Repositories;

namespace CarApp.Controllers
{
    /// <summary>
    /// Manages Create, read, update, delete operations for car listings.
    /// </summary>
    [ApiController]
    [Route("api/[controller]")]
    public class CarsController : ControllerBase
    {
        private readonly ICarRepository _repository;

        /// <summary>
        /// Initializes a new instance of the <see cref="CarsController"/> class.
        /// </summary>
        /// <param name="repository">The car repository instance injected via dependency injection.</param>
        public CarsController(ICarRepository repository)
        {
            _repository = repository;
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
                return StatusCode(400, new { error = "Invalid operation", details = ex.Message });
            }
            catch (System.Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while retrieving cars", details = ex.Message });
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
                return StatusCode(400, new { error = "Invalid operation", details = ex.Message });
            }
            catch (Exception ex)
            {
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
                    return BadRequest(new { error = "Validation failed", details = errors });
                }

                await _repository.AddAsync(newCar);
                
                // Returns a 201 Created status and points to the new GetCarById endpoint
                return CreatedAtAction(nameof(GetCarById), new { id = newCar.Id }, newCar);
            }
            catch (ArgumentException ex)
            {
                return StatusCode(400, new { error = "Invalid car data", details = ex.Message });
            }
            catch (Exception ex)
            {
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

                if (!await _repository.ExistsAsync(id))
                    return NotFound(new { error = $"Car with ID {id} not found" });

                if (!ModelState.IsValid)
                {
                    var errors = ModelState.Values.SelectMany(v => v.Errors)
                        .Select(e => e.ErrorMessage).ToList();
                    return BadRequest(new { error = "Validation failed", details = errors });
                }

                updatedCar.Id = id;
                await _repository.UpdateAsync(updatedCar);
                
                return NoContent();
            }
            catch (ArgumentException ex)
            {
                return StatusCode(400, new { error = "Invalid car data", details = ex.Message });
            }
            catch (Exception ex)
            {
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

                if (!await _repository.ExistsAsync(id))
                    return NotFound(new { error = $"Car with ID {id} not found" });

                await _repository.DeleteAsync(id);
                
                return NoContent();
            }
            catch (InvalidOperationException ex)
            {
                return StatusCode(400, new { error = "Cannot delete car", details = ex.Message });
            }
            catch (Exception ex)
            {
                return StatusCode(500, new { error = "An error occurred while deleting the car", details = ex.Message });
            }
        }
    }
}