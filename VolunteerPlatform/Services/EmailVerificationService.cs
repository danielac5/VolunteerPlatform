using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Entities;
using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.Services;

public class EmailVerificationService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher _hasher;

    public EmailVerificationService(AppDbContext db, PasswordHasher hasher)
    {
        _db = db;
        _hasher = hasher;
    }

    public async Task GenerateCode(User user, VerificationPurpose purpose)
    {
        var oldCodes = await _db.EmailVerificationCodes
            .Where(x =>
                x.UserId == user.Id &&
                x.Purpose == purpose &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var old in oldCodes)
        {
            old.IsUsed = true;
        }

        var code = Random.Shared.Next(10000, 99999).ToString();
        var codeHash = _hasher.Hash(code);

        var entity = new EmailVerificationCode
        {
            UserId = user.Id,
            CodeHash = codeHash,
            Purpose = purpose,
            ExpiresAt = DateTime.UtcNow.AddMinutes(10),
            IsUsed = false,
            Attempts = 0,
            MaxAttempts = 5
        };

        _db.EmailVerificationCodes.Add(entity);

        await _db.SaveChangesAsync();

        Console.WriteLine($"[{purpose}] {user.Email} -> {code}");
    }


    public async Task<bool> ConfirmEmail(string email, string code)
    {
        return await ValidateCode(
            email,
            code,
            VerificationPurpose.EmailConfirmation,
            user => user.EmailConfirmed = true
        );
    }

    public async Task<bool> ResetPassword(string email, string code, string newPasswordHash)
    {
        return await ValidateCode(
            email,
            code,
            VerificationPurpose.PasswordReset,
            user => user.PasswordHash = newPasswordHash
        );
    }

    private async Task<bool> ValidateCode(
        string email,
        string code,
        VerificationPurpose purpose,
        Action<User> onSuccess)
    {
        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null) return false;

        var verification = await _db.EmailVerificationCodes
            .Where(x =>
                x.UserId == user.Id &&
                x.Purpose == purpose &&
                !x.IsUsed &&
                x.ExpiresAt > DateTime.UtcNow &&
                x.Attempts < x.MaxAttempts)
            .OrderByDescending(x => x.CreatedAt)
            .FirstOrDefaultAsync();

        if (verification == null) return false;

        verification.Attempts++;

        var ok = _hasher.Verify(code, verification.CodeHash);
        if (!ok)
        {
            await _db.SaveChangesAsync();
            return false;
        }

        verification.IsUsed = true;
        onSuccess(user);

        await _db.SaveChangesAsync();
        return true;
    }
}
