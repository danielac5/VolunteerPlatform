using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.Domain.Entities;

public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string FullName { get; set; } = string.Empty;

    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;

    public string PhoneNumber { get; set; } = string.Empty;

    public DateOnly DateOfBirth { get; set; }
    public bool IsMinor { get; set; }

    public string PasswordHash { get; set; } = string.Empty;

    public bool IsBlocked { get; set; } = false;

    public UserRole Role { get; set; } = UserRole.Volunteer;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
