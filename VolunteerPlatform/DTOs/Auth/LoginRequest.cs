using System.ComponentModel.DataAnnotations;

namespace VolunteerPlatform.DTOs.Auth;

public class LoginRequest
{
    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required]
    public string Password { get; set; } = string.Empty;
}
