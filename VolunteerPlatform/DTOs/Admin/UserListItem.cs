namespace VolunteerPlatform.DTOs.Admin;

public class UserListItem
{
    public Guid Id { get; set; }
    public string FullName { get; set; } = "";
    public string Email { get; set; } = "";
    public string PhoneNumber { get; set; } = "";
    public bool EmailConfirmed { get; set; }
    public bool IsBlocked { get; set; }
    public bool IsMinor { get; set; }
    public string Role { get; set; } = "";
    public DateTime CreatedAt { get; set; }
}
