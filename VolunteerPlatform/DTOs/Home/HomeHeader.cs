namespace VolunteerPlatform.DTOs.Home;
public class HomeHeader
{
    public string FullName { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string Level { get; set; } = "Începător";
    public int TotalHours { get; set; }
    public int VolunteerCount { get; set; }
    public string? AvatarUrl { get; set; }  
}