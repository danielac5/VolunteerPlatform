namespace VolunteerPlatform.DTOs.Posts;

public class MyApplicationListItem
{
    public Guid ApplicationId { get; set; }
    public Guid PostId { get; set; }

    public string PostTitle { get; set; } = "";
    public string PostType { get; set; } = "";
    public string PostStatus { get; set; } = "";
    public string City { get; set; } = "";
    public string Area { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }

    public string ApplicationStatus { get; set; } = "";
    public string RejectReason { get; set; } = "";

    public DateTime CreatedAt { get; set; }
}
