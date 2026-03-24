using VolunteerPlatform.DTOs.Home;

namespace VolunteerPlatform.Services;

public interface IHomeService
{
    Task<HomeDashboard> GetDashboardAsync(Guid userId);
}