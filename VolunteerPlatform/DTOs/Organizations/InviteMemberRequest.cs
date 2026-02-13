namespace VolunteerPlatform.DTOs.Organizations;

public class InviteMemberRequest
{
    public string Email { get; set; } = string.Empty;
    public string Role { get; set; } = "Viewer"; 
}
