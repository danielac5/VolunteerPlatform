using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Home;
using VolunteerPlatform.Services;

namespace VolunteerPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "VerifiedUser")]
public class HomeController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _current;

    public HomeController(AppDbContext db, CurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    [HttpGet]
    public async Task<IActionResult> GetHome()
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        var user = await _db.Users
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId);

        if (user == null)
            return Unauthorized(new { message = "Utilizatorul nu a fost găsit" });

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
                Meta = $"{p.StartsAt:dd.MM.yyyy} • {p.StartsAt:HH:mm} - {p.EndsAt:HH:mm} • {p.City}",
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
                Meta = $"{a.HelpPost.StartsAt:dd.MM.yyyy} • {a.HelpPost.StartsAt:HH:mm} - {a.HelpPost.EndsAt:HH:mm} • {a.HelpPost.City}",
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

        var recommendedBaseQuery = _db.HelpPosts
            .AsNoTracking()
            .Include(p => p.Organization)
            .Where(p =>
                p.CreatedByUserId != userId &&
                p.Status == HelpPostStatus.Pending);

        if (!string.IsNullOrWhiteSpace(user.City))
        {
            var city = user.City.Trim().ToLowerInvariant();
            recommendedBaseQuery = recommendedBaseQuery.Where(p => p.City.ToLower() == city);
        }

        if (preferredCategories.Count > 0)
        {
            recommendedBaseQuery = recommendedBaseQuery.Where(p => preferredCategories.Contains(p.Category));
        }

        var recommendedActivities = await recommendedBaseQuery
            .OrderByDescending(p => p.CreatedAt)
            .Take(2)
            .Select(p => new HomeRecommendation
            {
                Id = p.Id,
                Title = p.Title,
                OrganizationName = p.Organization != null ? p.Organization.Name : "Organizație",
                Meta = $"{p.StartsAt:dd.MM.yyyy} • {p.StartsAt:HH:mm} - {p.EndsAt:HH:mm} • {p.City}",
                Category = (int)p.Category
            })
            .ToListAsync();

        var quickCategories = preferredCategories
            .Select(x => (int)x)
            .Distinct()
            .ToList();

        if (quickCategories.Count == 0)
            quickCategories = new List<int> { 0, 1, 2, 3, 4, 99 };

        var now = DateTime.UtcNow;

        var currentMonthAssignments = await _db.HelpPostAssignments
            .AsNoTracking()
            .Include(x => x.HelpPost)
            .Include(x => x.Application)
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
            .Include(x => x.HelpPost)
            .Include(x => x.Application)
            .Where(x =>
                x.Application.ApplicantUserId == userId &&
                x.HelpPost.Status == HelpPostStatus.Completed)
            .ToListAsync();

        var totalHours = (int)Math.Round(
            allCompletedAssignments.Sum(x =>
                Math.Max(0, (x.HelpPost.EndsAt - x.HelpPost.StartsAt).TotalHours))
        );

        var volunteerCount = allCompletedAssignments.Count;

        var response = new HomeDashboard
        {
            Header = new HomeHeader
            {
                FullName = user.FullName,
                FirstName = GetFirstName(user.FullName),
                Level = CalculateLevel(totalHours),
                TotalHours = totalHours,
                VolunteerCount = volunteerCount,
                AvatarUrl = user.AvatarUrl,  
            },
            CurrentActivities = currentActivities,
            RecommendedActivities = recommendedActivities,
            QuickCategories = quickCategories.Take(6).ToList(),
            Impact = new HomeImpact
            {
                CurrentMonthHours = currentMonthHours,
                CurrentMonthVolunteerCount = currentMonthVolunteerCount
            }
        };

        return Ok(response);
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
        if (totalHours >= 120) return "Lider voluntar";
        if (totalHours >= 60) return "Implicat";
        if (totalHours >= 30) return "Activ";
        return "Începător";
    }
}