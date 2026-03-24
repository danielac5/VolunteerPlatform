using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Entities;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Auth;

namespace VolunteerPlatform.Services;

public class AuthService
{
    private readonly AppDbContext _db;
    private readonly PasswordHasher _hasher;
    private readonly JwtTokenService _jwt;
    private readonly EmailVerificationService _emailVerification;

    public AuthService(
        AppDbContext db,
        PasswordHasher hasher,
        JwtTokenService jwt,
        EmailVerificationService emailVerification)
    {
        _db = db;
        _hasher = hasher;
        _jwt = jwt;
        _emailVerification = emailVerification;
    }

    public async Task<AuthResponse> Register(RegisterRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();
        var fullName = request.FullName.Trim();
        var phone = "+373" + request.PhoneDigits.Trim();

        if (await _db.Users.AnyAsync(u => u.Email == email))
            throw new Exception("Email already exists");

        if (await _db.Users.AnyAsync(u => u.PhoneNumber == phone))
            throw new Exception("Phone number already exists");

        var today = DateOnly.FromDateTime(DateTime.UtcNow);
        var age = today.Year - request.DateOfBirth.Year;
        if (request.DateOfBirth > today.AddYears(-age)) age--;
        var isMinor = age < 18;

        var user = new User
        {
            FullName = fullName,
            Email = email,
            PhoneNumber = phone,
            DateOfBirth = request.DateOfBirth,
            IsMinor = isMinor,
            PasswordHash = _hasher.Hash(request.Password),
            Role = UserRole.Volunteer,
            EmailConfirmed = false,
            City = string.IsNullOrWhiteSpace(request.City) ? null : request.City.Trim(),
            Area = string.IsNullOrWhiteSpace(request.Area) ? null : request.Area.Trim()
        };

        _db.Users.Add(user);

        var distinctCategories = request.PreferredCategories
            .Distinct()
            .Take(2)
            .ToList();

        foreach (var categoryValue in distinctCategories)
        {
            if (!Enum.IsDefined(typeof(VolunteerCategory), categoryValue))
                continue;

            _db.UserCategoryPreferences.Add(new UserCategoryPreference
            {
                UserId = user.Id,
                Category = (VolunteerCategory)categoryValue
            });
        }

        await _db.SaveChangesAsync();
        await _emailVerification.GenerateCode(user, VerificationPurpose.EmailConfirmation);

        return new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Token = string.Empty
        };
    }

    public async Task<AuthResponse> Login(LoginRequest request)
    {
        var email = request.Email.Trim().ToLowerInvariant();

        var user = await _db.Users.FirstOrDefaultAsync(u => u.Email == email);
        if (user == null || !_hasher.Verify(request.Password, user.PasswordHash))
            throw new Exception("Parolă invalidă");

        if (!user.EmailConfirmed)
            throw new Exception("Emailul nu este confirmat");

        return new AuthResponse
        {
            UserId = user.Id,
            FullName = user.FullName,
            Role = user.Role.ToString(),
            Token = _jwt.CreateToken(user)
        };
    }
}