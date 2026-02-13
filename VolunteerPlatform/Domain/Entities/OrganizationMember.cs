using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.Domain.Entities;

public class OrganizationMember
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public OrgRole Role { get; set; } = OrgRole.Viewer;

    public bool IsActive { get; set; } = true;
    public DateTime JoinedAt { get; set; } = DateTime.UtcNow;
}
