using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.Domain.Entities;

public class HelpPostApplication
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid HelpPostId { get; set; }
    public HelpPost HelpPost { get; set; } = null!;

    public Guid ApplicantUserId { get; set; }
    public User ApplicantUser { get; set; } = null!;

    // daca aplica ong
    public Guid? ApplicantOrganizationId { get; set; }
    public Organization? ApplicantOrganization { get; set; }

    public string? Message { get; set; }

    public string? AvailabilityNote { get; set; }

    public ApplicationStatus Status { get; set; } = ApplicationStatus.Pending;
    public ApplicationRejectReason RejectReason { get; set; } = ApplicationRejectReason.None;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
    public DateTime? UpdatedAt { get; set; }
}
