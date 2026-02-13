namespace VolunteerPlatform.DTOs.Home;

public class HomeCardItem
{
    public Guid Id { get; set; }
    public string Type { get; set; } = "";
    public string Status { get; set; } = "";
    public string Title { get; set; } = "";
    public DateTime StartsAt { get; set; }
    public string City { get; set; } = "";
    public string Area { get; set; } = "";
}
