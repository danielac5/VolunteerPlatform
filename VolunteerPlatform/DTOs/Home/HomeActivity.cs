namespace VolunteerPlatform.DTOs.Home;

public class HomeActivity
{
    public Guid Id { get; set; }
    public string Kind { get; set; } = string.Empty;
    public string Title { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public string ActionText { get; set; } = "Detalii";
    public string Status { get; set; } = string.Empty;
    public DateTime SortDate { get; set; }
}