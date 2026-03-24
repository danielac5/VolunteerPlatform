namespace VolunteerPlatform.DTOs.Feedback;

public class UserFeedbackSummaryDto
{
    public double AverageSafety { get; set; }
    public double AverageCommunication { get; set; }
    public double AverageReliability { get; set; }
    public double OverallAverage { get; set; }
    public int TotalCount { get; set; }
    public List<FeedbackItemDto> Items { get; set; } = [];
}

public class FeedbackItemDto
{
    public Guid Id { get; set; }
    public Guid HelpPostId { get; set; }
    public string HelpPostTitle { get; set; } = string.Empty;
    public Guid FromUserId { get; set; }
    public string FromUserName { get; set; } = string.Empty;
    public int Safety { get; set; }
    public int Communication { get; set; }
    public int Reliability { get; set; }
    public int Average { get; set; }
    public string? Comment { get; set; }
    public DateTime CreatedAt { get; set; }
}