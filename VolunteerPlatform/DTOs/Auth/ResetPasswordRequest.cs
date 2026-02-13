using System.ComponentModel.DataAnnotations;

namespace VolunteerPlatform.DTOs.Auth;

public class ResetPasswordRequest
{
    [Required, EmailAddress, StringLength(200)]
    public string Email { get; set; } = string.Empty;

    [Required, RegularExpression(@"^\d{5}$", ErrorMessage = "Codul trebuie să conțină exact 5 cifre.")]
    public string Code { get; set; } = string.Empty;

    [Required, RegularExpression(@"^(?=.*[A-Z])(?=.*\d).{8,}$",
        ErrorMessage = "Parola nu bifează toate cerințele de securitate.")]
    public string NewPassword { get; set; } = string.Empty;

    [Required]
    public string ConfirmNewPassword { get; set; } = string.Empty;
}
