using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.DTOs.Feedback;
using VolunteerPlatform.Services;

namespace VolunteerPlatform.Controllers;

[ApiController]
[Route("api/feedback")]
[Authorize(Policy = "VerifiedUser")]
public class FeedbackController : ControllerBase
{
    private readonly HelpPostsService _helpPostsService;
    private readonly AppDbContext _db;
    private readonly CurrentUserService _current;

    public FeedbackController(HelpPostsService helpPostsService, AppDbContext db, CurrentUserService current)
    {
        _helpPostsService = helpPostsService;
        _db = db;
        _current = current;
    }

    // POST /api/feedback/{postId}
    // Trimite feedback pentru o aplicație acceptată dintr-un post InProgress
    [HttpPost("{postId:guid}")]
    public async Task<IActionResult> Submit(Guid postId, [FromBody] SubmitFeedbackRequest req)
    {
        try
        {
            await _helpPostsService.SubmitFeedback(postId, req);
            return Ok(new { message = "Feedback trimis cu succes" });
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }

    // GET /api/feedback/user/{userId}
    // Returnează toate feedback-urile primite de un user + medii
    [HttpGet("user/{userId:guid}")]
    public async Task<IActionResult> GetUserFeedback(Guid userId)
    {
        try
        {
            var feedbacks = await _db.HelpPostFeedbacks
                .AsNoTracking()
                .Where(f => f.ToUserId == userId)
                .OrderByDescending(f => f.CreatedAt)
                .Select(f => new FeedbackItemDto
                {
                    Id = f.Id,
                    HelpPostId = f.HelpPostId,
                    HelpPostTitle = f.HelpPost.Title,
                    FromUserId = f.FromUserId,
                    FromUserName = f.FromUser.FullName,
                    Safety = f.Safety,
                    Communication = f.Communication,
                    Reliability = f.Reliability,
                    Average = (int)Math.Round((f.Safety + f.Communication + f.Reliability) / 3.0),
                    Comment = f.Comment,
                    CreatedAt = f.CreatedAt
                })
                .ToListAsync();

            if (feedbacks.Count == 0)
            {
                return Ok(new UserFeedbackSummaryDto
                {
                    TotalCount = 0,
                    Items = []
                });
            }

            var summary = new UserFeedbackSummaryDto
            {
                TotalCount = feedbacks.Count,
                AverageSafety = Math.Round(feedbacks.Average(f => f.Safety), 1),
                AverageCommunication = Math.Round(feedbacks.Average(f => f.Communication), 1),
                AverageReliability = Math.Round(feedbacks.Average(f => f.Reliability), 1),
                OverallAverage = Math.Round(feedbacks.Average(f => f.Average), 1),
                Items = feedbacks
            };

            return Ok(summary);
        }
        catch (Exception ex)
        {
            return BadRequest(new { message = ex.Message });
        }
    }
}