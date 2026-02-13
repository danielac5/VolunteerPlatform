using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.Domain.Entities;

public class HelpPost
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public HelpPostType Type { get; set; }
    public HelpPostStatus Status { get; set; } = HelpPostStatus.Pending;

    public VolunteerCategory Category { get; set; } = VolunteerCategory.Other;

    public string Title { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;

    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    public string City { get; set; } = string.Empty;
    public string Area { get; set; } = string.Empty;
    public string? AddressApprox { get; set; }

    public int NeededVolunteers { get; set; } = 1;

    // creator e mereu user 
    public Guid CreatedByUserId { get; set; }
    public User CreatedByUser { get; set; } = null!;

    // pentru ong
    public Guid? OrganizationId { get; set; }
    public Organization? Organization { get; set; }

    public string? NeedsReviewReason { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
