namespace VolunteerPlatform.Domain.Entities;

public class HelpPostFeedback
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid HelpPostId { get; set; }
    public HelpPost HelpPost { get; set; } = null!;

    public Guid FromUserId { get; set; }
    public User FromUser { get; set; } = null!;

    public Guid ToUserId { get; set; }
    public User ToUser { get; set; } = null!;

    public Guid ApplicationId { get; set; }
    public HelpPostApplication Application { get; set; } = null!;

    public int Safety { get; set; }
    public int Communication { get; set; }
    public int Reliability { get; set; }

    public string? Comment { get; set; }

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
