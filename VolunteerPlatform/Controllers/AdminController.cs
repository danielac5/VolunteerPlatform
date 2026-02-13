using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Admin;

namespace VolunteerPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "VerifiedUser", Roles = "Admin")]
public class AdminController : ControllerBase
{
    private readonly AppDbContext _db;

    public AdminController(AppDbContext db)
    {
        _db = db;
    }

    private static string NormalizeEmail(string email)
        => email.Trim().ToLowerInvariant();

    [HttpGet("users")]
    public async Task<IActionResult> GetUsers(
        [FromQuery] string? search,
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 20)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.Users.AsNoTracking();

        if (!string.IsNullOrWhiteSpace(search))
        {
            search = search.Trim();
            query = query.Where(u =>
                EF.Functions.Like(u.FullName, $"%{search}%") ||
                EF.Functions.Like(u.Email, $"%{search}%") ||
                EF.Functions.Like(u.PhoneNumber, $"%{search}%"));
        }

        var total = await query.CountAsync();

        var items = await query
            .OrderByDescending(u => u.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(u => new UserListItem
            {
                Id = u.Id,
                FullName = u.FullName,
                Email = u.Email,
                PhoneNumber = u.PhoneNumber,
                EmailConfirmed = u.EmailConfirmed,
                IsBlocked = u.IsBlocked,
                IsMinor = u.IsMinor,
                Role = u.Role.ToString(),
                CreatedAt = u.CreatedAt
            })
            .ToListAsync();

        return Ok(new { total, page, pageSize, items });
    }

    [HttpPatch("users/{id:guid}/block")]
    public async Task<IActionResult> SetBlocked(Guid id, [FromBody] SetBlockedRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "Utilizatorul nu a fost găsit" });

        user.IsBlocked = request.IsBlocked;
        await _db.SaveChangesAsync();

        return Ok(new { message = request.IsBlocked ? "Utilizator blocat" : "Utilizator deblocat" });
    }

    [HttpPatch("users/block-by-email")]
    public async Task<IActionResult> BlockByEmail([FromBody] BlockByEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email obligatoriu" });

        var email = NormalizeEmail(request.Email);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return NotFound(new { message = "Utilizatorul nu a fost găsit" });

        user.IsBlocked = request.IsBlocked;
        await _db.SaveChangesAsync();

        return Ok(new { message = request.IsBlocked ? "Utilizator blocat" : "Utilizator deblocat" });
    }

    [HttpPatch("users/{id:guid}/role")]
    public async Task<IActionResult> SetRole(Guid id, [FromBody] SetRoleRequest request)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "Utilizatorul nu a fost găsit" });

        if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
            return BadRequest(new { message = "Rol invalid. Folosește: Volunteer, ONG, Admin" });

        user.Role = parsedRole;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Rol schimbat în {user.Role}" });
    }

    [HttpPatch("users/role-by-email")]
    public async Task<IActionResult> SetRoleByEmail([FromBody] SetRoleByEmailRequest request)
    {
        if (string.IsNullOrWhiteSpace(request.Email))
            return BadRequest(new { message = "Email obligatoriu" });

        if (string.IsNullOrWhiteSpace(request.Role))
            return BadRequest(new { message = "Rol obligatoriu" });

        var email = NormalizeEmail(request.Email);

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null)
            return NotFound(new { message = "Utilizatorul nu a fost găsit" });

        if (!Enum.TryParse<UserRole>(request.Role, true, out var parsedRole))
            return BadRequest(new { message = "Rol invalid. Folosește: Volunteer, ONG, Admin" });

        user.Role = parsedRole;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Rol schimbat în {user.Role}" });
    }

    [HttpPatch("users/{id:guid}/set-email-confirmed")]
    public async Task<IActionResult> SetEmailConfirmed(Guid id, [FromQuery] bool value)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Id == id);
        if (user == null)
            return NotFound(new { message = "Utilizatorul nu a fost găsit" });

        user.EmailConfirmed = value;
        await _db.SaveChangesAsync();

        return Ok(new { message = value ? "Email marcat ca confirmat" : "Email marcat ca neconfirmat" });
    }
}
