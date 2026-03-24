namespace VolunteerPlatform.DTOs.Home;

public class HomeRecommendation
{
    public Guid Id { get; set; }
    public string Title { get; set; } = string.Empty;
    public string OrganizationName { get; set; } = string.Empty;
    public string Meta { get; set; } = string.Empty;
    public int Category { get; set; }
}