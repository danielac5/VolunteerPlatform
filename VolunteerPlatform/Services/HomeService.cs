using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Home;

namespace VolunteerPlatform.Services;

public class HomeService : IHomeService
{
    private readonly AppDbContext _db;

    public HomeService(AppDbContext db)
    {
        _db = db;
    }

    public async Task<HomeDashboard> GetDashboardAsync(Guid userId)
    {
        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            throw new Exception("Utilizatorul nu a fost găsit");

        var createdActivities = await _db.HelpPosts
            .AsNoTracking()
            .Where(p => p.CreatedByUserId == userId)
            .Select(p => new HomeActivity
            {
                Id = p.Id,
                Kind = "created_post",
                Title = p.Type == HelpPostType.Request
                    ? $"Ai o cerere activă: {p.Title}"
                    : $"Ai o ofertă activă: {p.Title}",
                Meta = FormatPostMeta(p.StartsAt, p.EndsAt, p.City, p.Area),
                ActionText = "Detalii",
                Status = p.Status.ToString(),
                SortDate = p.CreatedAt
            })
            .ToListAsync();

        var appliedActivities = await _db.HelpPostApplications
            .AsNoTracking()
            .Include(a => a.HelpPost)
            .Where(a => a.ApplicantUserId == userId)
            .Select(a => new HomeActivity
            {
                Id = a.HelpPostId,
                Kind = a.Status == ApplicationStatus.Accepted
                    ? "application_accepted"
                    : "application_pending",
                Title = a.Status == ApplicationStatus.Accepted
                    ? $"Ai fost acceptat(ă): {a.HelpPost.Title}"
                    : $"Ai aplicat: {a.HelpPost.Title}",
                Meta = FormatPostMeta(a.HelpPost.StartsAt, a.HelpPost.EndsAt, a.HelpPost.City, a.HelpPost.Area),
                ActionText = "Detalii",
                Status = a.Status.ToString(),
                SortDate = a.CreatedAt
            })
            .ToListAsync();

        var currentActivities = createdActivities
            .Concat(appliedActivities)
            .OrderByDescending(x => x.SortDate)
            .Take(3)
            .ToList();

        var preferredCategories = await _db.UserCategoryPreferences
            .AsNoTracking()
            .Where(x => x.UserId == userId)
            .Select(x => x.Category)
            .ToListAsync();

        var recommendedActivities = await GetRecommendedActivitiesAsync(userId, user.City, preferredCategories);

        var quickCategories = preferredCategories
            .Select(x => (int)x)
            .Distinct()
            .ToList();

        if (quickCategories.Count == 0)
        {
            quickCategories = new List<int> { 0, 1, 2, 3, 4, 99 };
        }

        var now = DateTime.UtcNow;

        var currentMonthAssignments = await _db.HelpPostAssignments
            .AsNoTracking()
            .Include(x => x.Application)
            .ThenInclude(a => a.HelpPost)
            .Where(x =>
                x.Application.ApplicantUserId == userId &&
                x.HelpPost.Status == HelpPostStatus.Completed &&
                x.HelpPost.StartsAt.Year == now.Year &&
                x.HelpPost.StartsAt.Month == now.Month)
            .ToListAsync();

        var currentMonthHours = (int)Math.Round(
            currentMonthAssignments.Sum(x =>
                Math.Max(0, (x.HelpPost.EndsAt - x.HelpPost.StartsAt).TotalHours))
        );

        var currentMonthVolunteerCount = currentMonthAssignments.Count;

        var allCompletedAssignments = await _db.HelpPostAssignments
            .AsNoTracking()
            .Include(x => x.Application)
            .ThenInclude(a => a.HelpPost)
            .Where(x =>
                x.Application.ApplicantUserId == userId &&
                x.HelpPost.Status == HelpPostStatus.Completed)
            .ToListAsync();

        var totalHours = (int)Math.Round(
            allCompletedAssignments.Sum(x =>
                Math.Max(0, (x.HelpPost.EndsAt - x.HelpPost.StartsAt).TotalHours))
        );

        var volunteerCount = allCompletedAssignments.Count;

        var header = new HomeHeader
        {
            FullName = user.FullName,
            FirstName = GetFirstName(user.FullName),
            Level = CalculateLevel(totalHours),
            TotalHours = totalHours,
            VolunteerCount = volunteerCount
        };

        return new HomeDashboard
        {
            Header = header,
            CurrentActivities = currentActivities,
            RecommendedActivities = recommendedActivities,
            QuickCategories = quickCategories.Take(6).ToList(),
            Impact = new HomeImpact
            {
                CurrentMonthHours = currentMonthHours,
                CurrentMonthVolunteerCount = currentMonthVolunteerCount
            }
        };
    }

    private async Task<List<HomeRecommendation>> GetRecommendedActivitiesAsync(
        Guid userId,
        string? userCity,
        List<VolunteerCategory> preferredCategories)
    {
        var baseQuery = _db.HelpPosts
            .AsNoTracking()
            .Include(p => p.Organization)
            .Where(p =>
                p.CreatedByUserId != userId &&
                p.Status == HelpPostStatus.Pending);

        var strictQuery = baseQuery;

        if (!string.IsNullOrWhiteSpace(userCity))
        {
            var cityNormalized = userCity.Trim().ToLowerInvariant();
            strictQuery = strictQuery.Where(p => p.City.ToLower() == cityNormalized);
        }

        if (preferredCategories.Count > 0)
        {
            strictQuery = strictQuery.Where(p => preferredCategories.Contains(p.Category));
        }

        var strictResults = await strictQuery
            .OrderByDescending(p => p.CreatedAt)
            .Take(2)
            .Select(p => new HomeRecommendation
            {
                Id = p.Id,
                Title = p.Title,
                OrganizationName = p.Organization != null ? p.Organization.Name : "Organizație",
                Meta = FormatPostMeta(p.StartsAt, p.EndsAt, p.City, p.Area),
                Category = (int)p.Category
            })
            .ToListAsync();

        if (strictResults.Count >= 2)
            return strictResults;

        var fallbackQuery = baseQuery;

        if (!string.IsNullOrWhiteSpace(userCity))
        {
            var cityNormalized = userCity.Trim().ToLowerInvariant();
            fallbackQuery = fallbackQuery.Where(p => p.City.ToLower() == cityNormalized);
        }

        var fallbackResults = await fallbackQuery
            .OrderByDescending(p => p.CreatedAt)
            .Take(6)
            .Select(p => new HomeRecommendation
            {
                Id = p.Id,
                Title = p.Title,
                OrganizationName = p.Organization != null ? p.Organization.Name : "Organizație",
                Meta = FormatPostMeta(p.StartsAt, p.EndsAt, p.City, p.Area),
                Category = (int)p.Category
            })
            .ToListAsync();

        var merged = strictResults
            .Concat(fallbackResults)
            .GroupBy(x => x.Id)
            .Select(g => g.First())
            .Take(2)
            .ToList();

        return merged;
    }

    private static string GetFirstName(string fullName)
    {
        if (string.IsNullOrWhiteSpace(fullName))
            return "utilizator";

        return fullName.Trim().Split(' ', StringSplitOptions.RemoveEmptyEntries).FirstOrDefault()
               ?? "utilizator";
    }

    private static string CalculateLevel(int totalHours)
    {
        if (totalHours >= 30) return "Avansat";
        if (totalHours >= 10) return "Activ";
        return "Începător";
    }

    private static string FormatPostMeta(DateTime startsAt, DateTime endsAt, string city, string area)
    {
        return $"{startsAt:dd.MM.yyyy} • {startsAt:HH:mm} - {endsAt:HH:mm} • {city}";
    }
}