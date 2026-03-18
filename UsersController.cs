using Microsoft.AspNetCore.Mvc;
using UserManagementAPI.Models;
using UserManagementAPI.Services;

namespace UserManagementAPI.Controllers;

/// <summary>
/// Manages user records for TechHive Solutions internal tools.
/// CRUD endpoints scaffolded with Microsoft Copilot assistance.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class UsersController : ControllerBase
{
    private readonly IUserService _userService;

    public UsersController(IUserService userService)
    {
        _userService = userService;
    }

    // ──────────────────────────────────────────────
    // GET api/users
    // ──────────────────────────────────────────────
    /// <summary>Retrieves all users.</summary>
    /// <returns>A list of all users.</returns>
    [HttpGet]
    [ProducesResponseType(typeof(IEnumerable<User>), StatusCodes.Status200OK)]
    public IActionResult GetAllUsers()
    {
        var users = _userService.GetAllUsers();
        return Ok(users);
    }

    // ──────────────────────────────────────────────
    // GET api/users/{id}
    // ──────────────────────────────────────────────
    /// <summary>Retrieves a specific user by ID.</summary>
    /// <param name="id">The user's unique identifier.</param>
    [HttpGet("{id:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult GetUserById(int id)
    {
        var user = _userService.GetUserById(id);
        if (user is null)
            return NotFound(new { message = $"User with ID {id} not found." });

        return Ok(user);
    }

    // ──────────────────────────────────────────────
    // POST api/users
    // ──────────────────────────────────────────────
    /// <summary>Creates a new user.</summary>
    /// <param name="dto">User creation payload.</param>
    [HttpPost]
    [ProducesResponseType(typeof(User), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    public IActionResult CreateUser([FromBody] CreateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var created = _userService.CreateUser(dto);
        return CreatedAtAction(nameof(GetUserById), new { id = created.Id }, created);
    }

    // ──────────────────────────────────────────────
    // PUT api/users/{id}
    // ──────────────────────────────────────────────
    /// <summary>Updates an existing user's details.</summary>
    /// <param name="id">The user's unique identifier.</param>
    /// <param name="dto">Fields to update (all optional).</param>
    [HttpPut("{id:int}")]
    [ProducesResponseType(typeof(User), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult UpdateUser(int id, [FromBody] UpdateUserDto dto)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var updated = _userService.UpdateUser(id, dto);
        if (updated is null)
            return NotFound(new { message = $"User with ID {id} not found." });

        return Ok(updated);
    }

    // ──────────────────────────────────────────────
    // DELETE api/users/{id}
    // ──────────────────────────────────────────────
    /// <summary>Removes a user by ID.</summary>
    /// <param name="id">The user's unique identifier.</param>
    [HttpDelete("{id:int}")]
    [ProducesResponseType(StatusCodes.Status204NoContent)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public IActionResult DeleteUser(int id)
    {
        var deleted = _userService.DeleteUser(id);
        if (!deleted)
            return NotFound(new { message = $"User with ID {id} not found." });

        return NoContent();
    }
}
