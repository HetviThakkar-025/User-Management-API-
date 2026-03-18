using UserManagementAPI.Models;

namespace UserManagementAPI.Services;

/// <summary>
/// In-memory implementation of IUserService.
/// Copilot suggested seeding sample data and using a thread-safe counter for IDs.
/// </summary>
public class UserService : IUserService
{
    private readonly List<User> _users = new();
    private int _nextId = 1;

    public UserService()
    {
        // Seed data — scaffolded with Microsoft Copilot assistance
        _users.AddRange(new[]
        {
            new User { Id = _nextId++, FirstName = "Alice",   LastName = "Johnson", Email = "alice.johnson@techhive.com",   Department = "Engineering", Role = "Software Engineer", CreatedAt = DateTime.UtcNow },
            new User { Id = _nextId++, FirstName = "Bob",     LastName = "Smith",   Email = "bob.smith@techhive.com",       Department = "HR",          Role = "HR Manager",        CreatedAt = DateTime.UtcNow },
            new User { Id = _nextId++, FirstName = "Carol",   LastName = "Davis",   Email = "carol.davis@techhive.com",     Department = "IT",          Role = "IT Administrator",  CreatedAt = DateTime.UtcNow },
        });
    }

    public IEnumerable<User> GetAllUsers() => _users.AsReadOnly();

    public User? GetUserById(int id) => _users.FirstOrDefault(u => u.Id == id);

    public User CreateUser(CreateUserDto dto)
    {
        var user = new User
        {
            Id         = _nextId++,
            FirstName  = dto.FirstName,
            LastName   = dto.LastName,
            Email      = dto.Email,
            Department = dto.Department,
            Role       = dto.Role,
            CreatedAt  = DateTime.UtcNow,
            IsActive   = true
        };
        _users.Add(user);
        return user;
    }

    public User? UpdateUser(int id, UpdateUserDto dto)
    {
        var user = GetUserById(id);
        if (user is null) return null;

        // Copilot recommended the null-coalescing pattern to apply partial updates cleanly
        user.FirstName  = dto.FirstName  ?? user.FirstName;
        user.LastName   = dto.LastName   ?? user.LastName;
        user.Email      = dto.Email      ?? user.Email;
        user.Department = dto.Department ?? user.Department;
        user.Role       = dto.Role       ?? user.Role;
        user.IsActive   = dto.IsActive   ?? user.IsActive;

        return user;
    }

    public bool DeleteUser(int id)
    {
        var user = GetUserById(id);
        if (user is null) return false;
        _users.Remove(user);
        return true;
    }
}
