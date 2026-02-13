using System.ComponentModel.DataAnnotations;

namespace VolunteerPlatform.DTOs.Auth;

public class ForgotPasswordRequest
{
    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;
}
