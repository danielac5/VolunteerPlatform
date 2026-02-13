using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Feedback;
using VolunteerPlatform.DTOs.Posts;
using VolunteerPlatform.Services;

namespace VolunteerPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "VerifiedUser")]
public class HelpPostsController : ControllerBase
{
    private readonly HelpPostsService _service;
    private readonly AppDbContext _db;

    public HelpPostsController(HelpPostsService service, AppDbContext db)
    {
        _service = service;
        _db = db;
    }

    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateHelpPostRequest req)
    {
        try
        {
            var post = await _service.Create(req);
            return Ok(new
            {
                message = "Postare creată",
                postId = post.Id,
                status = post.Status.ToString()
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet]
    public async Task<IActionResult> List(
        [FromQuery] string? type,
        [FromQuery] string? status,
        [FromQuery] string? city,
        [FromQuery] string? category,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.HelpPosts.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<HelpPostType>(type, true, out var t))
            q = q.Where(p => p.Type == t);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<HelpPostStatus>(status, true, out var s))
            q = q.Where(p => p.Status == s);

        if (!string.IsNullOrWhiteSpace(city))
        {
            var c = city.Trim().ToLowerInvariant();
            q = q.Where(p => p.City.ToLower().Contains(c));
        }

        if (!string.IsNullOrWhiteSpace(category) && Enum.TryParse<VolunteerCategory>(category, true, out var cat))
            q = q.Where(p => p.Category == cat);

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                Category = p.Category.ToString(),
                p.Title,
                p.City,
                p.Area,
                p.StartsAt,
                p.EndsAt,
                p.NeededVolunteers,
                p.OrganizationId
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("{id:guid}")]
    public async Task<IActionResult> Details(Guid id)
    {
        var post = await _db.HelpPosts.AsNoTracking()
            .Where(p => p.Id == id)
            .Select(p => new
            {
                p.Id,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                Category = p.Category.ToString(),
                p.Title,
                p.Description,
                p.City,
                p.Area,
                p.AddressApprox,
                p.StartsAt,
                p.EndsAt,
                p.NeededVolunteers,
                p.OrganizationId,
                p.CreatedByUserId,
                p.CreatedAt
            })
            .FirstOrDefaultAsync();

        if (post == null)
            return NotFound(new { message = "Postarea nu a fost găsită" });

        return Ok(post);
    }

    [HttpPost("{id:guid}/apply")]
    public async Task<IActionResult> Apply(Guid id, [FromBody] ApplyToPostRequest req)
    {
        try
        {
            var app = await _service.Apply(id, req);
            return Ok(new
            {
                message = "Aplicația a fost trimisă",
                applicationId = app.Id
            });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/withdraw")]
    public async Task<IActionResult> Withdraw(Guid id)
    {
        try
        {
            await _service.Withdraw(id);
            return Ok(new { message = "Aplicația a fost retrasă" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{postId:guid}/applications/{applicationId:guid}/accept")]
    public async Task<IActionResult> Accept(Guid postId, Guid applicationId)
    {
        try
        {
            await _service.AcceptApplication(postId, applicationId);
            return Ok(new { message = "Aplicația a fost acceptată" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{id:guid}/cancel")]
    public async Task<IActionResult> Cancel(Guid id)
    {
        try
        {
            await _service.CancelPost(id);
            return Ok(new { message = "Postarea a fost anulată" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // list aplicatii pentru post pt creator
    [HttpGet("{id:guid}/applications")]
    public async Task<IActionResult> ListApplications(Guid id)
    {
        var apps = await _db.HelpPostApplications.AsNoTracking()
            .Where(a => a.HelpPostId == id)
            .OrderByDescending(a => a.CreatedAt)
            .Select(a => new
            {
                a.Id,
                a.ApplicantUserId,
                ApplicantName = a.ApplicantUser.FullName,
                ApplicantEmail = a.ApplicantUser.Email,

                Status = a.Status.ToString(),
                RejectReason = a.RejectReason.ToString(),

                a.Message,
                a.AvailabilityNote,

                a.CreatedAt
            })
            .ToListAsync();

        return Ok(apps);
    }

    [HttpGet("my-posts")]
    public async Task<IActionResult> MyPosts([FromQuery] int page = 1, [FromQuery] int pageSize = 20, [FromQuery] string? status = null)
    {
        try
        {
            var result = await _service.MyPosts(page, pageSize, status);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("{postId:guid}/feedback-required")]
    public async Task<IActionResult> FeedbackRequired(Guid postId)
    {
        try
        {
            var result = await _service.FeedbackRequired(postId);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("{postId:guid}/finalize")]
    public async Task<IActionResult> Finalize(Guid postId)
    {
        try
        {
            await _service.FinalizePost(postId);
            return Ok(new { message = "Postarea a fost finalizată" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpGet("my")]
    public async Task<IActionResult> MyPosts(
    [FromServices] CurrentUserService current,
    [FromQuery] string? status,
    [FromQuery] string? type,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        var userId = current.UserId ?? throw new Exception("Neautentificat");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.HelpPosts.AsNoTracking()
            .Where(p => p.CreatedByUserId == userId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<HelpPostStatus>(status, true, out var s))
            q = q.Where(p => p.Status == s);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<HelpPostType>(type, true, out var t))
            q = q.Where(p => p.Type == t);

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new MyPostListItem
            {
                Id = p.Id,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                Category = p.Category.ToString(),
                Title = p.Title,
                City = p.City,
                Area = p.Area,
                StartsAt = p.StartsAt,
                EndsAt = p.EndsAt,
                NeededVolunteers = p.NeededVolunteers,
                OrganizationId = p.OrganizationId,
                CreatedAt = p.CreatedAt,

                ApplicationsCount = _db.HelpPostApplications.Count(a => a.HelpPostId == p.Id),
                AcceptedCount = _db.HelpPostApplications.Count(a => a.HelpPostId == p.Id && a.Status == ApplicationStatus.Accepted)
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("my-applications")]
    public async Task<IActionResult> MyApplications(
    [FromServices] CurrentUserService current,
    [FromQuery] string? status,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        var userId = current.UserId ?? throw new Exception("Neautentificat");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.HelpPostApplications.AsNoTracking()
            .Where(a => a.ApplicantUserId == userId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApplicationStatus>(status, true, out var s))
            q = q.Where(a => a.Status == s);

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new MyApplicationListItem
            {
                ApplicationId = a.Id,
                PostId = a.HelpPostId,
                PostTitle = a.HelpPost.Title,
                PostType = a.HelpPost.Type.ToString(),
                PostStatus = a.HelpPost.Status.ToString(),
                City = a.HelpPost.City,
                Area = a.HelpPost.Area,
                StartsAt = a.HelpPost.StartsAt,
                EndsAt = a.HelpPost.EndsAt,
                ApplicationStatus = a.Status.ToString(),
                RejectReason = a.RejectReason.ToString(),
                CreatedAt = a.CreatedAt
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpGet("org/{orgId:guid}")]
    public async Task<IActionResult> OrgPosts(
    Guid orgId,
    [FromServices] CurrentUserService current,
    [FromQuery] string? status,
    [FromQuery] string? type,
    [FromQuery] int page = 1,
    [FromQuery] int pageSize = 20)
    {
        var userId = current.UserId ?? throw new Exception("Neautentificat");

        var myRole = await _db.OrganizationMembers.AsNoTracking()
            .Where(m => m.OrganizationId == orgId && m.UserId == userId && m.IsActive)
            .Select(m => (OrgRole?)m.Role)
            .FirstOrDefaultAsync();

        if (myRole == null)
            return Forbid();

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.HelpPosts.AsNoTracking()
            .Where(p => p.OrganizationId == orgId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<HelpPostStatus>(status, true, out var s))
            q = q.Where(p => p.Status == s);

        if (!string.IsNullOrWhiteSpace(type) && Enum.TryParse<HelpPostType>(type, true, out var t))
            q = q.Where(p => p.Type == t);

        var total = await q.CountAsync();

        var items = await q
            .OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new MyPostListItem
            {
                Id = p.Id,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                Category = p.Category.ToString(),
                Title = p.Title,
                City = p.City,
                Area = p.Area,
                StartsAt = p.StartsAt,
                EndsAt = p.EndsAt,
                NeededVolunteers = p.NeededVolunteers,
                OrganizationId = p.OrganizationId,
                CreatedAt = p.CreatedAt,

                ApplicationsCount = _db.HelpPostApplications.Count(a => a.HelpPostId == p.Id),
                AcceptedCount = _db.HelpPostApplications.Count(a => a.HelpPostId == p.Id && a.Status == ApplicationStatus.Accepted)
            })
            .ToListAsync();

        return Ok(new
        {
            total,
            page,
            pageSize,
            items,
            myRole = myRole.ToString(),
            canCreate = myRole != OrgRole.Viewer,
            viewerMessage = myRole == OrgRole.Viewer ? "Nu poți crea cereri momentan." : null
        });
    }
}
