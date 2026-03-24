using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.DTOs.Support;

public class CreateTicketRequest
{
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = "general";
}

public class UpdateTicketStatusRequest
{
    public string Status { get; set; } = string.Empty; 
    public string? AdminReply { get; set; }
}

public class TicketDto
{
    public Guid Id { get; set; }
    public string Subject { get; set; } = string.Empty;
    public string Description { get; set; } = string.Empty;
    public string Category { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty; 
    public string? AdminReply { get; set; }
    public DateTime CreatedAt { get; set; }
    public DateTime? UpdatedAt { get; set; }
    public DateTime? RepliedAt { get; set; }
}

public class TicketListDto
{
    public List<TicketDto> Items { get; set; } = [];
    public int TotalCount { get; set; }
}