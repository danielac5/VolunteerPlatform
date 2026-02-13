namespace VolunteerPlatform.Domain.Entities;

public class HelpPostAssignment
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid HelpPostId { get; set; }
    public HelpPost HelpPost { get; set; } = null!;

    public Guid ApplicationId { get; set; }
    public HelpPostApplication Application { get; set; } = null!;

    public DateTime AssignedAt { get; set; } = DateTime.UtcNow;
}
