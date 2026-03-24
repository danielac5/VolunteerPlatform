using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using System.Security.Claims;
using VolunteerPlatform.DTOs.Support;
using VolunteerPlatform.Services;

namespace VolunteerPlatform.Controllers;

[ApiController]
[Route("api/support")]
[Authorize]
public class SupportController : ControllerBase
{
    private readonly ISupportService _svc;

    public SupportController(ISupportService svc)
    {
        _svc = svc;
    }

    private Guid CurrentUserId =>
        Guid.Parse(User.FindFirstValue(ClaimTypes.NameIdentifier)!);

    private bool IsAdmin =>
        User.IsInRole("PlatformAdmin");

    [HttpPost("tickets")]
    public async Task<IActionResult> Create([FromBody] CreateTicketRequest req)
    {
        var ticket = await _svc.CreateAsync(CurrentUserId, req);
        return Ok(ticket);
    }

    [HttpGet("tickets/me")]
    public async Task<IActionResult> GetMine()
    {
        var result = await _svc.GetMyTicketsAsync(CurrentUserId);
        return Ok(result.Items); 
    }

    [HttpGet("tickets/{id:guid}")]
    public async Task<IActionResult> GetOne(Guid id)
    {
        var ticket = await _svc.GetByIdAsync(id, CurrentUserId);
        return Ok(ticket);
    }

    [HttpGet("tickets")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> GetAll(
        [FromQuery] int page = 1,
        [FromQuery] int pageSize = 30)
    {
        var result = await _svc.GetAllAsync(page, pageSize);
        return Ok(result);
    }

    [HttpPatch("tickets/{id:guid}/status")]
    [Authorize(Roles = "PlatformAdmin")]
    public async Task<IActionResult> UpdateStatus(
        Guid id, [FromBody] UpdateTicketStatusRequest req)
    {
        var ticket = await _svc.UpdateStatusAsync(id, CurrentUserId, req);
        return Ok(ticket);
    }
}