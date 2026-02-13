using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.Domain.Entities;

public class OrganizationInvite
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid OrganizationId { get; set; }
    public Organization Organization { get; set; } = null!;

    public string Email { get; set; } = string.Empty;  

    public OrgRole Role { get; set; } = OrgRole.Viewer;

    public bool IsUsed { get; set; } = false;
    public DateTime ExpiresAt { get; set; } = DateTime.UtcNow.AddDays(7);

    public Guid CreatedByUserId { get; set; }           
    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
