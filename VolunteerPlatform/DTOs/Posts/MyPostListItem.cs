namespace VolunteerPlatform.DTOs.Posts;

public class MyPostListItem
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string Category { get; set; } = "";
    public string Title { get; set; } = "";
    public string City { get; set; } = "";
    public string Area { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public DateTime EndsAt { get; set; }
    public int NeededVolunteers { get; set; }
    public int AcceptedCount { get; set; }
    public int ApplicationsCount { get; set; }
    public Guid? OrganizationId { get; set; }
    public DateTime CreatedAt { get; set; }
}
