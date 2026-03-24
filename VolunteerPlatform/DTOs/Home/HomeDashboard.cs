namespace VolunteerPlatform.DTOs.Home;

public class HomeDashboard
{
    public HomeHeader Header { get; set; } = new();
    public List<HomeActivity> CurrentActivities { get; set; } = new();
    public List<HomeRecommendation> RecommendedActivities { get; set; } = new();
    public List<int> QuickCategories { get; set; } = new();
    public HomeImpact Impact { get; set; } = new();
}