namespace VolunteerPlatform.DTOs.Admin;

public class BlockByEmailRequest
{
    public string Email { get; set; } = string.Empty;
    public bool IsBlocked { get; set; }
}