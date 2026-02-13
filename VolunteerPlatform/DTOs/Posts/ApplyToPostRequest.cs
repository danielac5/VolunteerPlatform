namespace VolunteerPlatform.DTOs.Posts;

public class ApplyToPostRequest
{
    public string? Message { get; set; }              
    public string? AvailabilityNote { get; set; }     

    public Guid? ApplicantOrganizationId { get; set; }
}
