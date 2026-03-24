using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.DTOs.Posts;

public class UpdateHelpPostRequest
{
    public string? Title { get; set; }
    public string? Description { get; set; }

    public DateTime? StartsAt { get; set; }
    public DateTime? EndsAt { get; set; }

    public string? City { get; set; }
    public string? Area { get; set; }
    public string? AddressApprox { get; set; }

    public int? NeededVolunteers { get; set; }

    public string? Category { get; set; }

    public string? Urgency { get; set; }
    public string? HelpType { get; set; }

    public string? ContactTelegram { get; set; }
    public string? ContactViber { get; set; }

    public bool? ContactAfterAccept { get; set; }
}