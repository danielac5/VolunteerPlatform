namespace VolunteerPlatform.DTOs.Admin;

public class SetBlockedRequest
{
    public bool IsBlocked { get; set; }
    public string? Reason { get; set; } 
}
