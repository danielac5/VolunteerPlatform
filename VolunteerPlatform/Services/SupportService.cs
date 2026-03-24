using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Entities;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Support;

namespace VolunteerPlatform.Services;

public interface ISupportService
{
    Task<TicketDto> CreateAsync(Guid userId, CreateTicketRequest req);
    Task<TicketListDto> GetMyTicketsAsync(Guid userId);
    Task<TicketDto> GetByIdAsync(Guid ticketId, Guid userId);

    Task<TicketListDto> GetAllAsync(int page = 1, int pageSize = 30);
    Task<TicketDto> UpdateStatusAsync(Guid ticketId, Guid adminId, UpdateTicketStatusRequest req);
}

public class SupportService : ISupportService
{
    private readonly AppDbContext _db;
    private readonly INotificationService _notifications;

    public SupportService(AppDbContext db, INotificationService notifications)
    {
        _db = db;
        _notifications = notifications;
    }

    public async Task<TicketDto> CreateAsync(Guid userId, CreateTicketRequest req)
    {
        if (string.IsNullOrWhiteSpace(req.Subject))
            throw new Exception("Subiectul este obligatoriu.");

        if (string.IsNullOrWhiteSpace(req.Description) || req.Description.Trim().Length < 20)
            throw new Exception("Descrierea trebuie să aibă cel puțin 20 de caractere.");

        var ticket = new SupportTicket
        {
            UserId = userId,
            Subject = req.Subject.Trim(),
            Description = req.Description.Trim(),
            Category = req.Category.Trim().ToLowerInvariant(),
            Status = TicketStatus.Open,
        };

        _db.SupportTickets.Add(ticket);
        await _db.SaveChangesAsync();

        return ToDto(ticket);
    }

    public async Task<TicketListDto> GetMyTicketsAsync(Guid userId)
    {
        var items = await _db.SupportTickets
            .AsNoTracking()
            .Where(t => t.UserId == userId)
            .OrderByDescending(t => t.CreatedAt)
            .Select(t => ToDto(t))
            .ToListAsync();

        return new TicketListDto { Items = items, TotalCount = items.Count };
    }

    public async Task<TicketDto> GetByIdAsync(Guid ticketId, Guid userId)
    {
        var ticket = await _db.SupportTickets
            .AsNoTracking()
            .FirstOrDefaultAsync(t => t.Id == ticketId && t.UserId == userId);

        if (ticket == null)
            throw new Exception("Ticketul nu a fost găsit.");

        return ToDto(ticket);
    }

    
    public async Task<TicketListDto> GetAllAsync(int page = 1, int pageSize = 30)
    {
        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var query = _db.SupportTickets.AsNoTracking();
        var total = await query.CountAsync();

        var items = await query
            .OrderBy(t => t.Status == TicketStatus.Open ? 0 :
                          t.Status == TicketStatus.InProgress ? 1 :
                          t.Status == TicketStatus.Resolved ? 2 : 3)
            .ThenByDescending(t => t.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(t => ToDto(t))
            .ToListAsync();

        return new TicketListDto { Items = items, TotalCount = total };
    }

    public async Task<TicketDto> UpdateStatusAsync(
        Guid ticketId, Guid adminId, UpdateTicketStatusRequest req)
    {
        if (!Enum.TryParse<TicketStatus>(req.Status, ignoreCase: true, out var newStatus))
            throw new Exception("Status invalid. Valori acceptate: open, in_progress, resolved, closed.");

        var ticket = await _db.SupportTickets
            .FirstOrDefaultAsync(t => t.Id == ticketId);

        if (ticket == null)
            throw new Exception("Ticketul nu a fost găsit.");

        var statusChanged = ticket.Status != newStatus;
        ticket.Status = newStatus;
        ticket.UpdatedAt = DateTime.UtcNow;

        if (!string.IsNullOrWhiteSpace(req.AdminReply))
        {
            ticket.AdminReply = req.AdminReply.Trim();
            ticket.RepliedByAdminId = adminId;
            ticket.RepliedAt = DateTime.UtcNow;
        }

        await _db.SaveChangesAsync();

        if (statusChanged || !string.IsNullOrWhiteSpace(req.AdminReply))
        {
            var ticketNumber = ticket.Id.ToString()[..8].ToUpperInvariant();

            await _notifications.NotifyTicketUpdatedAsync(
                userId: ticket.UserId,
                ticketNumber: ticketNumber,
                ticketId: ticket.Id);
        }

        return ToDto(ticket);
    }

    private static TicketDto ToDto(SupportTicket t) => new()
    {
        Id = t.Id,
        Subject = t.Subject,
        Description = t.Description,
        Category = t.Category,
        Status = t.Status.ToString().ToLowerInvariant()
                       .Replace("inprogress", "in_progress"), 
        AdminReply = t.AdminReply,
        CreatedAt = t.CreatedAt,
        UpdatedAt = t.UpdatedAt,
        RepliedAt = t.RepliedAt,
    };
}