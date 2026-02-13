namespace VolunteerPlatform.DTOs.Admin;

public class SetRoleByEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = string.Empty; 
}