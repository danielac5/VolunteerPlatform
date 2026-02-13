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
    public async Task<IActionResult> GetHome([FromQuery] Guid? orgId = null)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        // cererile/postarile create de mine
        var myCreated = await _db.HelpPosts.AsNoTracking()
            .Where(p => p.CreatedByUserId == userId)
            .OrderByDescending(p => p.CreatedAt)
            .Take(5)
            .Select(p => new HomeCardItem
            {
                Id = p.Id,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                Title = p.Title,
                StartsAt = p.StartsAt,
                City = p.City,
                Area = p.Area
            })
            .ToListAsync();

        // cererile la care am aplicat
        var myApplied = await _db.HelpPostApplications.AsNoTracking()
            .Where(a => a.ApplicantUserId == userId)
            .OrderByDescending(a => a.CreatedAt)
            .Take(5)
            .Select(a => new HomeCardItem
            {
                Id = a.HelpPostId,
                Type = a.HelpPost.Type.ToString(),
                Status = a.HelpPost.Status.ToString(),
                Title = a.HelpPost.Title,
                StartsAt = a.HelpPost.StartsAt,
                City = a.HelpPost.City,
                Area = a.HelpPost.Area
            })
            .ToListAsync();

        // recomandate matching
        var myCity = myCreated.FirstOrDefault()?.City ?? myApplied.FirstOrDefault()?.City;

        var recQ = _db.HelpPosts.AsNoTracking()
            .Where(p => p.Status == HelpPostStatus.Pending);

        if (!string.IsNullOrWhiteSpace(myCity))
        {
            var c = myCity.Trim().ToLowerInvariant();
            recQ = recQ.Where(p => p.City.ToLower() == c);
        }

        var recommended = await recQ
            .OrderByDescending(p => p.CreatedAt)
            .Take(10)
            .Select(p => new HomeCardItem
            {
                Id = p.Id,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                Title = p.Title,
                StartsAt = p.StartsAt,
                City = p.City,
                Area = p.Area
            })
            .ToListAsync();

        object? orgSection = null;

        // organizația mea
        if (orgId.HasValue)
        {
            var myRole = await _db.OrganizationMembers.AsNoTracking()
                .Where(m => m.OrganizationId == orgId.Value && m.UserId == userId && m.IsActive)
                .Select(m => (OrgRole?)m.Role)
                .FirstOrDefaultAsync();

            if (myRole == null)
                return Forbid();

            var orgPosts = await _db.HelpPosts.AsNoTracking()
                .Where(p => p.OrganizationId == orgId.Value)
                .OrderByDescending(p => p.CreatedAt)
                .Take(10)
                .Select(p => new HomeCardItem
                {
                    Id = p.Id,
                    Type = p.Type.ToString(),
                    Status = p.Status.ToString(),
                    Title = p.Title,
                    StartsAt = p.StartsAt,
                    City = p.City,
                    Area = p.Area
                })
                .ToListAsync();

            orgSection = new
            {
                orgId = orgId.Value,
                myRole = myRole.ToString(),
                canCreate = myRole != OrgRole.Viewer,
                viewerMessage = myRole == OrgRole.Viewer ? "Nu poți crea cereri momentan" : null,
                orgPosts,
                orgEmptyMessage = orgPosts.Count == 0 ? "Organizația nu are postări momentan" : null
            };
        }

        return Ok(new
        {
            myCreated,
            myCreatedEmptyMessage = myCreated.Count == 0 ? "Nu ai creat încă nimic" : null,

            myApplied,
            myAppliedEmptyMessage = myApplied.Count == 0 ? "Nu ai aplicat încă la nimic." : null,

            recommended,
            recommendedEmptyMessage = recommended.Count == 0 ? "Nu sunt recomandări momentan" : null,

            orgSection
        });
    }
}
