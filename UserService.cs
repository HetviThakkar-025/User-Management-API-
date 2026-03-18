using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

/// <summary>
/// In-memory implementation of IUserService.
/// Bug fixes applied with Copilot: duplicate email check, trimming input, thread-safe ID counter.
/// </summary>
public class UserService : IUserService
{
    private readonly List<User> _users = new();
    private int _nextId = 1;
    private readonly object _lock = new(); // Bug fix: Copilot suggested lock for thread safety

    public UserService()
    {
        _users.AddRange(new[]
        {
            new User { Id = _nextId++, FirstName = "Alice", LastName = "Johnson", Email = "alice.johnson@techhive.com", Department = "Engineering", Role = "Software Engineer", CreatedAt = DateTime.UtcNow },
            new User { Id = _nextId++, FirstName = "Bob",   LastName = "Smith",   Email = "bob.smith@techhive.com",    Department = "HR",          Role = "HR Manager",        CreatedAt = DateTime.UtcNow },
            new User { Id = _nextId++, FirstName = "Carol", LastName = "Davis",   Email = "carol.davis@techhive.com",  Department = "IT",          Role = "IT Administrator",  CreatedAt = DateTime.UtcNow },
        });
    }

    /// <summary>
    /// Bug fix: Copilot suggested returning IReadOnlyList instead of raw list reference
    /// to prevent external mutation of the internal collection.
    /// </summary>
    public IEnumerable<User> GetAllUsers() => _users.AsReadOnly();

    public User? GetUserById(int id) => _users.FirstOrDefault(u => u.Id == id);

    /// <summary>
    /// Bug fix: Added duplicate email check. Previously, the same email could be registered multiple times.
    /// Copilot suggested normalizing email to lowercase before comparison.
    /// Bug fix: Trim whitespace from string fields to prevent "  Alice  " being stored.
    /// </summary>
    public (User? User, string? Error) CreateUser(CreateUserDto dto)
    {
        var normalizedEmail = dto.Email.Trim().ToLowerInvariant();

        if (_users.Any(u => u.Email.ToLowerInvariant() == normalizedEmail))
            return (null, $"A user with email '{dto.Email}' already exists.");

        lock (_lock)
        {
            var user = new User
            {
                Id         = _nextId++,
                FirstName  = dto.FirstName.Trim(),
                LastName   = dto.LastName.Trim(),
                Email      = normalizedEmail,
                Department = dto.Department.Trim(),
                Role       = dto.Role.Trim(),
                CreatedAt  = DateTime.UtcNow,
                IsActive   = true
            };
            _users.Add(user);
            return (user, null);
        }
    }

    /// <summary>
    /// Bug fix: Copilot identified that updating email didn't check for duplicates.
    /// Also added trim to all string updates.
    /// </summary>
    public (User? User, string? Error) UpdateUser(int id, UpdateUserDto dto)
    {
        var user = GetUserById(id);
        if (user is null) return (null, null);

        if (dto.Email is not null)
        {
            var normalizedEmail = dto.Email.Trim().ToLowerInvariant();
            if (_users.Any(u => u.Id != id && u.Email.ToLowerInvariant() == normalizedEmail))
                return (null, $"Email '{dto.Email}' is already in use by another user.");
            user.Email = normalizedEmail;
        }

        user.FirstName  = dto.FirstName?.Trim()  ?? user.FirstName;
        user.LastName   = dto.LastName?.Trim()   ?? user.LastName;
        user.Department = dto.Department?.Trim() ?? user.Department;
        user.Role       = dto.Role?.Trim()       ?? user.Role;
        user.IsActive   = dto.IsActive           ?? user.IsActive;

        return (user, null);
    }

    public bool DeleteUser(int id)
    {
        var user = GetUserById(id);
        if (user is null) return false;
        _users.Remove(user);
        return true;
    }
}
