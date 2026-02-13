using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.Domain.Entities;

public class EmailVerificationCode
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public Guid UserId { get; set; }
    public User User { get; set; } = null!;

    public string CodeHash { get; set; } = string.Empty;

    public VerificationPurpose Purpose { get; set; }

    public DateTime ExpiresAt { get; set; }
    public bool IsUsed { get; set; } = false;

    public int Attempts { get; set; } = 0;
    public int MaxAttempts { get; set; } = 5;

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}