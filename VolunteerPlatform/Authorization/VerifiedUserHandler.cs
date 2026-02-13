using Microsoft.AspNetCore.Authorization;
using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Services;

namespace VolunteerPlatform.Authorization;

public class VerifiedUserHandler : AuthorizationHandler<VerifiedUserRequirement>
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _current;

    public VerifiedUserHandler(AppDbContext db, CurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    protected override async Task HandleRequirementAsync(
        AuthorizationHandlerContext context,
        VerifiedUserRequirement requirement)
    {
        var userId = _current.UserId;
        if (userId == null) return;

        var user = await _db.Users.AsNoTracking()
            .FirstOrDefaultAsync(u => u.Id == userId.Value);

        if (user == null) return;
        if (user.IsBlocked) return;
        if (!user.EmailConfirmed) return;

        context.Succeed(requirement);
    }
}
