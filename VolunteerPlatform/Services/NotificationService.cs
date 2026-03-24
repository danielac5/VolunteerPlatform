using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Entities;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Notifications;

namespace VolunteerPlatform.Services;

public interface INotificationService
{
    Task CreateAsync(Guid userId, NotificationType type,
                     string title, string body,
                     Guid? resourceId = null, string? resourceType = null);

    Task NotifyNewApplicationAsync(Guid postOwnerId, string applicantName,
                                   string postTitle, Guid requestId);

    Task NotifyApplicationAcceptedAsync(Guid applicantId, string postTitle,
                                        Guid applicationId);

    Task NotifyApplicationWithdrawnAsync(Guid postOwnerId, string applicantName,
                                         string postTitle, Guid requestId);

    Task NotifyRequestInProgressAsync(Guid postOwnerId, string postTitle,
                                      Guid requestId);

    Task NotifyRequestCompletedAsync(Guid userId, string postTitle, Guid requestId);

    Task NotifyFeedbackReceivedAsync(Guid userId, int stars,
                                     string postTitle, Guid requestId);

    Task NotifyTicketUpdatedAsync(Guid userId, string ticketNumber, Guid ticketId);

    Task<NotificationListDto> GetByUserAsync(Guid userId, int page = 1, int pageSize = 30);
    Task<int> GetUnreadCountAsync(Guid userId);
    Task MarkAsReadAsync(Guid notificationId, Guid userId);
    Task MarkAllAsReadAsync(Guid userId);
    Task DeleteAsync(Guid notificationId, Guid userId);
    Task DeleteAllReadAsync(Guid userId);
}

public class NotificationService : INotificationService
{
    private readonly AppDbContext _db;

    public NotificationService(AppDbContext db)
    {
        _db = db;
    }

    public async Task CreateAsync(
        Guid userId, NotificationType type,
        string title, string body,
        Guid? resourceId = null, string? resourceType = null)
    {
        _db.Notifications.Add(new Notification
        {
            UserId = userId,
            Type = type,
            Title = title,
            Body = body,
            ResourceId = resourceId,
            ResourceType = resourceType,
        });

        await _db.SaveChangesAsync();
    }

    public Task NotifyNewApplicationAsync(
        Guid postOwnerId, string applicantName, string postTitle, Guid requestId)
        => CreateAsync(
            postOwnerId,
            NotificationType.NewApplication,
            title: "Aplicație nouă",
            body: $"{applicantName} a aplicat la {postTitle}.",
            resourceId: requestId,
            resourceType: "request");

    public Task NotifyApplicationAcceptedAsync(
        Guid applicantId, string postTitle, Guid applicationId)
        => CreateAsync(
            applicantId,
            NotificationType.ApplicationAccepted,
            title: "Ai fost acceptat!",
            body: $"Ai fost acceptat ca voluntar pentru {postTitle}.",
            resourceId: applicationId,
            resourceType: "application");

    public Task NotifyApplicationWithdrawnAsync(
        Guid postOwnerId, string applicantName, string postTitle, Guid requestId)
        => CreateAsync(
            postOwnerId,
            NotificationType.ApplicationWithdrawn,
            title: "Aplicație retrasă",
            body: $"{applicantName} și-a retras oferta pentru {postTitle}.",
            resourceId: requestId,
            resourceType: "request");

    public Task NotifyRequestInProgressAsync(
        Guid postOwnerId, string postTitle, Guid requestId)
        => CreateAsync(
            postOwnerId,
            NotificationType.RequestInProgress,
            title: "Cerere în desfășurare",
            body: $"„{postTitle} a intrat în desfășurare.",
            resourceId: requestId,
            resourceType: "request");

    public Task NotifyRequestCompletedAsync(
        Guid userId, string postTitle, Guid requestId)
        => CreateAsync(
            userId,
            NotificationType.RequestCompleted,
            title: "Activitate finalizată",
            body: $"„{postTitle} a fost finalizată. Nu uita să lași un feedback!",
            resourceId: requestId,
            resourceType: "request");

    public Task NotifyFeedbackReceivedAsync(
        Guid userId, int stars, string postTitle, Guid requestId)
        => CreateAsync(
            userId,
            NotificationType.FeedbackReceived,
            title: "Feedback primit",
            body: $"Ai primit un feedback de {stars}★ pentru „{postTitle}",
            resourceId: requestId,
            resourceType: "request");

    public Task NotifyTicketUpdatedAsync(
    Guid userId, string ticketNumber, Guid ticketId)
    => CreateAsync(
        userId,
        NotificationType.TicketUpdated,
        title: "Ticket actualizat",
        body: $"Statusul sau răspunsul pentru ticketul tău #{ticketNumber} a fost actualizat.",
        resourceId: ticketId,
        resourceType: "ticket");

    public async Task<NotificationListDto> GetByUserAsync(
        Guid userId, int page = 1, int pageSize = 30)
    {
        var query = _db.Notifications.Where(n => n.UserId == userId);

        var total = await query.CountAsync();
        var unread = await query.CountAsync(n => !n.IsRead);

        var items = await query
            .OrderBy(n => n.IsRead)
            .ThenByDescending(n => n.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .AsNoTracking()
            .Select(n => new NotificationDto
            {
                Id = n.Id,
                Type = n.Type.ToString(),
                Title = n.Title,
                Body = n.Body,
                IsRead = n.IsRead,
                CreatedAt = n.CreatedAt,
                ResourceId = n.ResourceId,
                ResourceType = n.ResourceType,
            })
            .ToListAsync();

        return new NotificationListDto
        {
            Items = items,
            UnreadCount = unread,
            TotalCount = total,
        };
    }

    public Task<int> GetUnreadCountAsync(Guid userId)
        => _db.Notifications.CountAsync(n => n.UserId == userId && !n.IsRead);

    public async Task MarkAsReadAsync(Guid notificationId, Guid userId)
    {
        var n = await _db.Notifications
            .FirstOrDefaultAsync(n => n.Id == notificationId && n.UserId == userId);

        if (n is null) return;

        n.IsRead = true;
        await _db.SaveChangesAsync();
    }

    public Task MarkAllAsReadAsync(Guid userId)
        => _db.Notifications
            .Where(n => n.UserId == userId && !n.IsRead)
            .ExecuteUpdateAsync(s => s.SetProperty(n => n.IsRead, true));

    public Task DeleteAsync(Guid notificationId, Guid userId)
        => _db.Notifications
            .Where(n => n.Id == notificationId && n.UserId == userId)
            .ExecuteDeleteAsync();

    public Task DeleteAllReadAsync(Guid userId)
        => _db.Notifications
            .Where(n => n.UserId == userId && n.IsRead)
            .ExecuteDeleteAsync();

    public Task NotifyFeedbackAvailableAsync(
    Guid userId, string postTitle, Guid requestId)
    => CreateAsync(
        userId,
        NotificationType.RequestCompleted,
        title: "Lasă feedback",
        body: $"Activitatea „{postTitle}” s-a încheiat. Acum poți lăsa feedback.",
        resourceId: requestId,
        resourceType: "request");
}