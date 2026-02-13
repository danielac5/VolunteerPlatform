using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Auth;
using VolunteerPlatform.Services;

namespace VolunteerPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
public class AuthController : ControllerBase
{
    private readonly AuthService _auth;

    public AuthController(AuthService auth)
    {
        _auth = auth;
    }

    [HttpPost("register")]
    public async Task<IActionResult> Register([FromBody] RegisterRequest request)
    {
        try
        {
            var result = await _auth.Register(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("login")]
    public async Task<IActionResult> Login([FromBody] LoginRequest request)
    {
        try
        {
            var result = await _auth.Login(request);
            return Ok(result);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    [HttpPost("confirm-email")]
    public async Task<IActionResult> ConfirmEmail(
    [FromBody] ConfirmEmailRequest request,
    [FromServices] EmailVerificationService service)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var code = request.Code.Trim();

        var success = await service.ConfirmEmail(email, code);

        if (!success)
            return BadRequest(new { message = "Cod invalid, expirat sau prea multe încercări" });

        return Ok(new { message = "Email confirmat cu succes" });
    }

    [HttpPost("resend-email-code")]
    public async Task<IActionResult> ResendEmailCode(
    [FromBody] ResendEmailCodeRequest request,
    [FromServices] AppDbContext db,
    [FromServices] EmailVerificationService service)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user != null && !user.EmailConfirmed)
        {
            await service.GenerateCode(user, VolunteerPlatform.Domain.Enums.VerificationPurpose.EmailConfirmation);
        }

        return Ok(new
        {
            message = "Un cod a fost trimis la adresă indicată"
        });
    }

    [HttpPost("forgot-password")]
    public async Task<IActionResult> ForgotPassword(
    [FromBody] ForgotPasswordRequest request,
    [FromServices] AppDbContext db,
    [FromServices] EmailVerificationService service)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user != null && !user.IsBlocked)
        {
            await service.GenerateCode(user, VerificationPurpose.PasswordReset);
        }

        return Ok(new { message = "Un cod a fost trimis la adresa indicată" });
    }

    [HttpPost("reset-password")]
    public async Task<IActionResult> ResetPassword(
    [FromBody] ResetPasswordRequest request,
    [FromServices] EmailVerificationService service,
    [FromServices] PasswordHasher hasher)
    {
        if (request.NewPassword != request.ConfirmNewPassword)
            return BadRequest(new { message = "Parolele nu coincid" });

        var email = request.Email.Trim().ToLowerInvariant();
        var code = request.Code.Trim();

        var newHash = hasher.Hash(request.NewPassword);

        var success = await service.ResetPassword(email, code, newHash);

        if (!success)
            return BadRequest(new { message = "Cod invalid, expirat sau prea multe încercări" });

        return Ok(new { message = "Parola a fost resetată cu succes" });
    }

    [HttpPost("resend-password-reset-code")]
    public async Task<IActionResult> ResendPasswordResetCode(
    [FromBody] ForgotPasswordRequest request,
    [FromServices] AppDbContext db,
    [FromServices] EmailVerificationService service)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var user = await db.Users.FirstOrDefaultAsync(u => u.Email == email);

        if (user != null && !user.IsBlocked)
        {
            await service.GenerateCode(user, VerificationPurpose.PasswordReset);
        }

        return Ok(new { message = "Un cod a fost trimis la adresa indicată" });
    }
}
