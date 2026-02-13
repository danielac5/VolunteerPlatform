using System.Security.Claims;

namespace VolunteerPlatform.Services;

public class CurrentUserService
{
    private readonly IHttpContextAccessor _http;

    public CurrentUserService(IHttpContextAccessor http)
    {
        _http = http;
    }

    public Guid? UserId
    {
        get
        {
            var id = _http.HttpContext?.User?.FindFirstValue("sub")
                     ?? _http.HttpContext?.User?.FindFirstValue(ClaimTypes.NameIdentifier);

            return Guid.TryParse(id, out var guid) ? guid : null;
        }
    }

    public string? Role => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Role);
    public string? Email => _http.HttpContext?.User?.FindFirstValue(ClaimTypes.Email);
}
