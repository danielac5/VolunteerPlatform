using System.ComponentModel.DataAnnotations;

namespace VolunteerPlatform.DTOs.Auth;

public class RegisterRequest
{
    [Required, StringLength(200, MinimumLength = 2)]
    public string FullName { get; set; } = string.Empty;

    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{8}$", ErrorMessage = "Numărul de telefon nu este introdus în format internațional")]
    public string PhoneDigits { get; set; } = string.Empty;

    [Required]
    public DateOnly DateOfBirth { get; set; }

    [Required, RegularExpression(@"^(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Parola nu bifează toate cerințele de securitate")]
    public string Password { get; set; } = string.Empty;
}
