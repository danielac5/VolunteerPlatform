using VolunteerPlatform.Domain.Enums;
namespace VolunteerPlatform.Domain.Entities;
public class User
{
    public Guid Id { get; set; } = Guid.NewGuid();
    public string FullName { get; set; } = string.Empty;
    public string? FirstName { get; set; }
    public string? LastName { get; set; }
    public string? AvatarUrl { get; set; }
    public string Email { get; set; } = string.Empty;
    public bool EmailConfirmed { get; set; } = false;
    public string PhoneNumber { get; set; } = string.Empty;
    public DateOnly DateOfBirth { get; set; }
    public bool IsMinor { get; set; }
    public string PasswordHash { get; set; } = string.Empty;
    public bool IsBlocked { get; set; } = false;
    public UserRole Role { get; set; } = UserRole.Volunteer;
    public string? City { get; set; }
    public string? Area { get; set; }
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;

    public string? Telegram { get; set; }
    public string? Viber { get; set; }
    public string Level { get; set; } = "Începător";
    public int TotalHours { get; set; } = 0;
    public int VolunteerCount { get; set; } = 0;

    public bool ContactTelegram { get; set; } = false;
    public bool ContactViber { get; set; } = false;
    public bool ContactWhatsapp { get; set; } = false;
    public bool ContactPhone { get; set; } = false;
    public bool ContactEmail { get; set; } = true;
}