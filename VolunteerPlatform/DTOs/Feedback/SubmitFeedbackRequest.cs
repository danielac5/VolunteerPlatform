namespace VolunteerPlatform.DTOs.Feedback;

public class SubmitFeedbackRequest
{
    public Guid ApplicationId { get; set; } 

    public int Safety { get; set; }        
    public int Communication { get; set; }  
    public int Reliability { get; set; }   

    public string? Comment { get; set; }
}
