using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers;

/// <summary>
/// Manages user records for TechHive Solutions internal tools.
/// Bug fixes applied with Microsoft Copilot: try-catch on all endpoints,
/// duplicate email handling, invalid ID validation, and performance optimization.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;
    private readonly ILogger<UsersController> _logger;

    // Bug fix: Copilot suggested injecting ILogger for structured error logging
    public UsersController(IUserService userService, ILogger<UsersController> logger)
    {
        _userService = userService;
        _logger = logger;
    }

    // ── GET api/users ──────────────────────────────────────────────────────────
    /// <summary>Retrieves all users.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetAllUsers()
    {
        try
        {
            // Performance fix: Copilot suggested materializing with ToList() once
            // to avoid multiple enumerations of the underlying collection.
            var users = _userService.GetAllUsers().ToList();
            _logger.LogInformation("Retrieved {Count} users.", users.Count);
            return Ok(users);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetAllUsers.");
            return StatusCode(500, new { message = "An unexpected error occurred while retrieving users." });
        }
    }

    // ── GET api/users/{id} ────────────────────────────────────────────────────
    /// <summary>Retrieves a specific user by ID.</summary>
    /// <param name="id">The user's unique identifier.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult GetUserById(int id)
    {
        // Bug fix: Copilot identified missing validation — negative/zero IDs caused
        // confusing 404s instead of a clear 400 Bad Request.
        if (id <= 0)
            return BadRequest(new { message = "ID must be a positive integer." });

        try
        {
            var user = _userService.GetUserById(id);
            if (user is null)
            {
                _logger.LogWarning("User with ID {Id} not found.", id);
                return NotFound(new { message = $"User with ID {id} was not found." });
            }
            return Ok(user);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in GetUserById for ID {Id}.", id);
            return StatusCode(500, new { message = "An unexpected error occurred while retrieving the user." });
        }
    }

    // ── POST api/users ────────────────────────────────────────────────────────
    /// <summary>Creates a new user.</summary>
    /// <param name="dto">User creation payload.</param>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult CreateUser([FromBody] CreateUserDto dto)
    {
        // [ApiController] handles ModelState automatically, but we add try-catch
        // around the service call to catch duplicate email conflicts and runtime errors.
        try
        {
            var (created, error) = _userService.CreateUser(dto);

            if (created is null)
            {
                // Bug fix: Previously returned 400 for duplicates. Copilot suggested
                // 409 Conflict is the correct HTTP status for duplicate resource errors.
                _logger.LogWarning("Duplicate email attempt: {Email}", dto.Email);
                return Conflict(new { message = error });
            }

            _logger.LogInformation("Created user {Id} ({Email}).", created.Id, created.Email);
            return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in CreateUser.");
            return StatusCode(500, new { message = "An unexpected error occurred while creating the user." });
        }
    }

    // ── PUT api/users/{id} ────────────────────────────────────────────────────
    /// <summary>Updates an existing user's details.</summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <param name="dto">Fields to update (all optional).</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        if (id <= 0)
            return BadRequest(new { message = "ID must be a positive integer." });

        try
        {
            var (updated, error) = _userService.UpdateUser(id, dto);

            if (error is not null)
            {
                _logger.LogWarning("Update conflict for ID {Id}: {Error}", id, error);
                return Conflict(new { message = error });
            }

            if (updated is null)
            {
                _logger.LogWarning("Update attempted on non-existent user ID {Id}.", id);
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            _logger.LogInformation("Updated user {Id}.", id);
            return Ok(updated);
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in UpdateUser for ID {Id}.", id);
            return StatusCode(500, new { message = "An unexpected error occurred while updating the user." });
        }
    }

    // ── DELETE api/users/{id} ─────────────────────────────────────────────────
    /// <summary>Removes a user by ID.</summary>
    /// <param name="id">The user's unique identifier.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status500InternalServerError)]
    public IActionResult DeleteUser(int id)
    {
        if (id <= 0)
            return BadRequest(new { message = "ID must be a positive integer." });

        try
        {
            var deleted = _userService.DeleteUser(id);
            if (!deleted)
            {
                _logger.LogWarning("Delete attempted on non-existent user ID {Id}.", id);
                return NotFound(new { message = $"User with ID {id} was not found." });
            }

            _logger.LogInformation("Deleted user {Id}.", id);
            return NoContent();
        }
        catch (Exception ex)
        {
            _logger.LogError(ex, "Unexpected error in DeleteUser for ID {Id}.", id);
            return StatusCode(500, new { message = "An unexpected error occurred while deleting the user." });
        }
    }
}
