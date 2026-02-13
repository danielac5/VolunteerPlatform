using VolunteerPlatform.Domain.Enums;

namespace VolunteerPlatform.Domain.Entities;

public class Organization
{
    public Guid Id { get; set; } = Guid.NewGuid();

    public string Name { get; set; } = string.Empty;        
    public string? Description { get; set; }                  

    public string Email { get; set; } = string.Empty;         
    public string PhoneNumber { get; set; } = string.Empty;   
    public string Address { get; set; }                      
    public string Idno { get; set; }                         
    public string? Category { get; set; }                     

    public OrganizationVerificationStatus VerificationStatus { get; set; } = OrganizationVerificationStatus.Pending;
    public string? VerificationNote { get; set; }             
    public DateTime? VerifiedAt { get; set; }
    public Guid? VerifiedByAdminId { get; set; }              

    public DateTime CreatedAt { get; set; } = DateTime.UtcNow;
}
