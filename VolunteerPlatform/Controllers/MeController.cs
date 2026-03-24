using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using System.ComponentModel.DataAnnotations;
using VolunteerPlatform.Data;
using VolunteerPlatform.Services;

namespace VolunteerPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "VerifiedUser")]
public class MeController : ControllerBase
{
    [HttpGet]
    public async Task<IActionResult> GetMe(
        [FromServices] AppDbContext db,
        [FromServices] CurrentUserService current)
    {
        var userId = current.UserId;
        if (userId == null)
            return Unauthorized(new { message = "Neautorizat" });

        var user = await db.Users.AsNoTracking()
            .Where(u => u.Id == userId.Value)
            .Select(u => new
            {
                id = u.Id,
                firstName = u.FirstName,
                lastName = u.LastName,
                email = u.Email,
                phone = u.PhoneNumber,
                telegram = u.Telegram,
                viber = u.Viber,
                isMinor = u.IsMinor,
                role = u.Role.ToString(),
                level = u.Level,
                totalHours = u.TotalHours,
                volunteerCount = u.VolunteerCount,
                avatarUrl = u.AvatarUrl,
                contactPreferences = new
                {
                    telegram = u.ContactTelegram,
                    viber = u.ContactViber,
                    whatsapp = u.ContactWhatsapp,
                    phone = u.ContactPhone,
                    email = u.ContactEmail,
                }
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return Unauthorized(new { message = "Neautorizat" });

        return Ok(user);
    }

    [HttpPatch]
    public async Task<IActionResult> UpdateMe(
        [FromServices] AppDbContext db,
        [FromServices] CurrentUserService current,
        [FromBody] UpdateMeRequest request)
    {
        if (!ModelState.IsValid)
            return BadRequest(ModelState);

        var userId = current.UserId;
        if (userId == null)
            return Unauthorized(new { message = "Neautorizat" });

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
            return Unauthorized(new { message = "Neautorizat" });

        if (request.FirstName is not null)
            user.FirstName = request.FirstName.Trim();

        if (request.LastName is not null)
            user.LastName = request.LastName.Trim();

        if (request.FirstName is not null || request.LastName is not null)
            user.FullName = $"{user.FirstName} {user.LastName}".Trim();

        if (request.Email is not null)
        {
            var emailTaken = await db.Users
                .AnyAsync(u => u.Email == request.Email.Trim() && u.Id != userId.Value);
            if (emailTaken)
                return Conflict(new { message = "Emailul este deja folosit de alt cont." });

            user.Email = request.Email.Trim();
        }

        if (request.Phone is not null)
            user.PhoneNumber = request.Phone.Trim();

        if (request.Telegram is not null)
            user.Telegram = request.Telegram.Trim();

        if (request.Viber is not null)
            user.Viber = request.Viber.Trim();

        if (request.ContactPreferences is not null)
        {
            var cp = request.ContactPreferences;
            if (cp.Telegram is not null) user.ContactTelegram = cp.Telegram.Value;
            if (cp.Viber is not null) user.ContactViber = cp.Viber.Value;
            if (cp.Whatsapp is not null) user.ContactWhatsapp = cp.Whatsapp.Value;
            if (cp.Phone is not null) user.ContactPhone = cp.Phone.Value;
            if (cp.Email is not null) user.ContactEmail = cp.Email.Value;
        }

        await db.SaveChangesAsync();
        return Ok(new { message = "Profil actualizat cu succes." });
    }

    [HttpPost("avatar")]
    public async Task<IActionResult> UploadAvatar(
        [FromServices] AppDbContext db,
        [FromServices] CurrentUserService current,
        IFormFile file,
        IWebHostEnvironment env)
    {
        var userId = current.UserId;
        if (userId == null)
            return Unauthorized(new { message = "Neautorizat" });

        if (file == null || file.Length == 0)
            return BadRequest(new { message = "Niciun fisier selectat." });

        if (file.Length > 5 * 1024 * 1024)
            return BadRequest(new { message = "Fisierul este prea mare. Maximum 5MB." });

        var allowed = new[] { "image/jpeg", "image/jpg", "image/png", "image/webp" };
        if (!allowed.Contains(file.ContentType.ToLower()))
            return BadRequest(new { message = "Doar imagini JPG, PNG sau WEBP sunt acceptate." });

        var user = await db.Users.FirstOrDefaultAsync(u => u.Id == userId.Value);
        if (user == null)
            return Unauthorized(new { message = "Neautorizat" });

        // ContentRootPath in loc de WebRootPath 
        var contentRoot = env.ContentRootPath;
        var uploadsFolder = Path.Combine(contentRoot, "wwwroot", "uploads", "avatars");
        Directory.CreateDirectory(uploadsFolder);

        // sterge poza veche daca exista
        if (!string.IsNullOrEmpty(user.AvatarUrl))
        {
            var oldFile = Path.Combine(contentRoot, "wwwroot", user.AvatarUrl.TrimStart('/'));
            if (System.IO.File.Exists(oldFile))
                System.IO.File.Delete(oldFile);
        }

        var ext = Path.GetExtension(file.FileName).ToLower();
        var fileName = $"{userId.Value}{ext}";
        var filePath = Path.Combine(uploadsFolder, fileName);

        using (var stream = new FileStream(filePath, FileMode.Create))
        {
            await file.CopyToAsync(stream);
        }

        user.AvatarUrl = $"/uploads/avatars/{fileName}";
        await db.SaveChangesAsync();

        return Ok(new
        {
            id = user.Id,
            fullName = user.FullName,
            firstName = user.FirstName,
            lastName = user.LastName,
            email = user.Email,
            phone = user.PhoneNumber,
            telegram = user.Telegram,
            viber = user.Viber,
            isMinor = user.IsMinor,
            role = user.Role.ToString(),
            level = user.Level,
            totalHours = user.TotalHours,
            volunteerCount = user.VolunteerCount,
            avatarUrl = user.AvatarUrl,
            contactPreferences = new
            {
                telegram = user.ContactTelegram,
                viber = user.ContactViber,
                whatsapp = user.ContactWhatsapp,
                phone = user.ContactPhone,
                email = user.ContactEmail,
            }
        });
    }

    public class UpdateMeRequest
    {
        [MaxLength(100)]
        public string? FirstName { get; set; }

        [MaxLength(100)]
        public string? LastName { get; set; }

        [EmailAddress, MaxLength(255)]
        public string? Email { get; set; }

        [Phone, MaxLength(30)]
        public string? Phone { get; set; }

        [MaxLength(100)]
        public string? Telegram { get; set; }

        [MaxLength(30)]
        public string? Viber { get; set; }

        public ContactPreferencesDto? ContactPreferences { get; set; }
    }

    public class ContactPreferencesDto
    {
        public bool? Telegram { get; set; }
        public bool? Viber { get; set; }
        public bool? Whatsapp { get; set; }
        public bool? Phone { get; set; }
        public bool? Email { get; set; }
    }
}