namespace VolunteerPlatform.DTOs.Organizations;

public class CreateOrganizationRequest
{
    public string Name { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string PhoneNumber { get; set; } = string.Empty;

    public string? Description { get; set; }
    public string? Address { get; set; }
    public string? Idno { get; set; }
    public string? Category { get; set; }
}
