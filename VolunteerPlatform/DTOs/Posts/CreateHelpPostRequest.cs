namespace VolunteerPlatform.DTOs.Posts;

public class CreateHelpPostRequest
{
    public string Type { get; set; } = "Request";
    public string Category { get; set; } = "Other";
    public string Title { get; set; } = "";
    public string Description { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public string City { get; set; } = "";
    public string Area { get; set; } = "";
    public string? AddressApprox { get; set; }
    public int NeededVolunteers { get; set; } = 1;
    public Guid? OrganizationId { get; set; }

    public string Urgency { get; set; } = "Low";          
    public string HelpType { get; set; } = "Individual";  
    public string? ContactTelegram { get; set; }
    public string? ContactViber { get; set; }
    public bool ContactAfterAccept { get; set; } = true;
}