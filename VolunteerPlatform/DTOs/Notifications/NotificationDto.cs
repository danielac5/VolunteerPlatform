namespace VolunteerPlatform.DTOs.Notifications;

public class NotificationDto
{
    public Guid Id { get; set; }
    public string Type { get; set; } = string.Empty; 
    public string Title { get; set; } = string.Empty;
    public string Body { get; set; } = string.Empty;
    public bool IsRead { get; set; }
    public DateTime CreatedAt { get; set; }
    public Guid? ResourceId { get; set; }
    public string? ResourceType { get; set; }
}

public class NotificationListDto
{
    public List<NotificationDto> Items { get; set; } = [];
    public int UnreadCount { get; set; }
    public int TotalCount { get; set; }
}

public class UnreadCountDto
{
    public int Count { get; set; }
}