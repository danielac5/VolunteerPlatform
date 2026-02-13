using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Services;

namespace VolunteerPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class MeController : ControllerBase
{
    [HttpGet]
    [Authorize(Policy = "VerifiedUser")]
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
                u.Id,
                u.FullName,
                u.Email,
                u.PhoneNumber,
                u.IsMinor,
                Role = u.Role.ToString()
            })
            .FirstOrDefaultAsync();

        if (user == null)
            return Unauthorized(new { message = "Neautorizat" });

        return Ok(user);
    }
}
