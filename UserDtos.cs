using System.ComponentModel.DataAnnotations;

namespace UserManagementAPI.Models;

/// <summary>
/// DTO for creating a new user.
/// Copilot added stricter validation to prevent empty/whitespace inputs and invalid formats.
/// </summary>
public class CreateUserDto
{
    [Required(ErrorMessage = "First name is required.")]
    [MinLength(1, ErrorMessage = "First name cannot be empty.")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, or apostrophes.")]
    public string FirstName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Last name is required.")]
    [MinLength(1, ErrorMessage = "Last name cannot be empty.")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, or apostrophes.")]
    public string LastName { get; set; } = string.Empty;

    [Required(ErrorMessage = "Email is required.")]
    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string Email { get; set; } = string.Empty;

    [Required(ErrorMessage = "Department is required.")]
    [MaxLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
    public string Department { get; set; } = string.Empty;

    [Required(ErrorMessage = "Role is required.")]
    [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
    public string Role { get; set; } = string.Empty;
}

/// <summary>
/// DTO for updating an existing user. All fields optional for partial updates.
/// </summary>
public class UpdateUserDto
{
    [MinLength(1, ErrorMessage = "First name cannot be empty.")]
    [MaxLength(50, ErrorMessage = "First name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "First name can only contain letters, spaces, hyphens, or apostrophes.")]
    public string? FirstName { get; set; }

    [MinLength(1, ErrorMessage = "Last name cannot be empty.")]
    [MaxLength(50, ErrorMessage = "Last name cannot exceed 50 characters.")]
    [RegularExpression(@"^[a-zA-Z\s\-']+$", ErrorMessage = "Last name can only contain letters, spaces, hyphens, or apostrophes.")]
    public string? LastName { get; set; }

    [EmailAddress(ErrorMessage = "A valid email address is required.")]
    [MaxLength(100, ErrorMessage = "Email cannot exceed 100 characters.")]
    public string? Email { get; set; }

    [MaxLength(100, ErrorMessage = "Department cannot exceed 100 characters.")]
    public string? Department { get; set; }

    [MaxLength(50, ErrorMessage = "Role cannot exceed 50 characters.")]
    public string? Role { get; set; }

    public bool? IsActive { get; set; }
}
