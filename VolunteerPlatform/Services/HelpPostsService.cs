using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Entities;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Posts;

namespace VolunteerPlatform.Services;

public class HelpPostsService
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _current;

    public HelpPostsService(AppDbContext db, CurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    private static bool IsWithinSafeHours(DateTime startsAt)
    {
        var hour = startsAt.Hour;
        return hour >= 6 && hour <= 21;
    }

    public async Task<HelpPost> Create(CreateHelpPostRequest req)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        // basic validations
        if (string.IsNullOrWhiteSpace(req.Title))
            throw new Exception("Titlul este obligatoriu");

        if (string.IsNullOrWhiteSpace(req.Description))
            throw new Exception("Descrierea este obligatorie");

        if (string.IsNullOrWhiteSpace(req.City))
            throw new Exception("Localitatea este obligatorie");

        if (string.IsNullOrWhiteSpace(req.Area))
            throw new Exception("Zona este obligatorie");

        if (!Enum.TryParse<HelpPostType>(req.Type, true, out var type))
            throw new Exception("Tip invalid. Folosește: Request sau Offer");

        if (!Enum.TryParse<VolunteerCategory>(req.Category, true, out var cat))
            cat = VolunteerCategory.Other;

        // interval validations
        if (req.EndsAt <= req.StartsAt)
            throw new Exception("Interval invalid");

        if (req.NeededVolunteers < 1 || req.NeededVolunteers > 50)
            throw new Exception("Numărul de persoane necesare este invalid");

        // daca postezi pe organizatie trebuie sa fii membru activ
        if (req.OrganizationId.HasValue)
        {
            var orgId = req.OrganizationId.Value;

            var myRole = await _db.OrganizationMembers.AsNoTracking()
                .Where(m => m.OrganizationId == orgId && m.UserId == userId && m.IsActive)
                .Select(m => (OrgRole?)m.Role)
                .FirstOrDefaultAsync();

            if (myRole == null)
                throw new Exception("Nu ești membru al acestei organizații");

            if (myRole == OrgRole.Viewer)
                throw new Exception("Nu ai permisiunea să creezi cereri pentru această organizație");
        }

        // safety rule 
        var needsReview = false;
        string? reason = null;

        if (!IsWithinSafeHours(req.StartsAt))
        {
            needsReview = true;
            reason = "Ora selectată este în afara intervalului permis (06:00–21:00).";
        }

        // detector termeni interziși 

        // create
        var post = new HelpPost
        {
            Type = type,
            Category = cat,

            Title = req.Title.Trim(),
            Description = req.Description.Trim(),

            StartsAt = req.StartsAt,
            EndsAt = req.EndsAt,

            City = req.City.Trim(),
            Area = req.Area.Trim(),

            AddressApprox = string.IsNullOrWhiteSpace(req.AddressApprox) ? null : req.AddressApprox.Trim(),

            NeededVolunteers = req.NeededVolunteers,

            CreatedByUserId = userId,
            OrganizationId = req.OrganizationId,

            Status = needsReview ? HelpPostStatus.NeedsReview : HelpPostStatus.Pending,
            NeedsReviewReason = reason,
            CreatedAt = DateTime.UtcNow
        };

        _db.HelpPosts.Add(post);
        await _db.SaveChangesAsync();
        return post;
    }

    public async Task<HelpPostApplication> Apply(Guid postId, ApplyToPostRequest req)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        var post = await _db.HelpPosts.FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null)
            throw new Exception("Postarea nu a fost găsită");

        if (post.Status is HelpPostStatus.Cancelled or HelpPostStatus.Completed or HelpPostStatus.NeedsReview)
            throw new Exception("Nu se poate aplica la această postare");

        if (post.CreatedByUserId == userId)
            throw new Exception("Nu poți aplica la propria postare");

        var exists = await _db.HelpPostApplications
            .AnyAsync(a => a.HelpPostId == postId && a.ApplicantUserId == userId);

        if (exists)
            throw new Exception("Ai aplicat deja la această postare");

        var acceptedCount = await _db.HelpPostApplications
            .Where(a => a.HelpPostId == postId && a.Status == ApplicationStatus.Accepted)
            .CountAsync();

        if (acceptedCount >= post.NeededVolunteers)
            throw new Exception("Numărul de persoane necesare a fost atins. Mulțumim pentru interes");

        var app = new HelpPostApplication
        {
            HelpPostId = postId,
            ApplicantUserId = userId,
            ApplicantOrganizationId = req.ApplicantOrganizationId,

            Message = string.IsNullOrWhiteSpace(req.Message) ? null : req.Message.Trim(),
            AvailabilityNote = string.IsNullOrWhiteSpace(req.AvailabilityNote) ? null : req.AvailabilityNote.Trim(),

            Status = ApplicationStatus.Pending,
            RejectReason = ApplicationRejectReason.None,
            CreatedAt = DateTime.UtcNow
        };

        _db.HelpPostApplications.Add(app);
        await _db.SaveChangesAsync();

        return app;
    }

    public async Task Withdraw(Guid postId)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        var app = await _db.HelpPostApplications
            .FirstOrDefaultAsync(a => a.HelpPostId == postId && a.ApplicantUserId == userId);

        if (app == null) throw new Exception("Nu există aplicație de retras");

        // stergem assignment
        if (app.Status == ApplicationStatus.Accepted)
        {
            var assignment = await _db.HelpPostAssignments.FirstOrDefaultAsync(x => x.ApplicationId == app.Id);
            if (assignment != null) _db.HelpPostAssignments.Remove(assignment);
        }

        app.Status = ApplicationStatus.Withdrawn;
        app.RejectReason = ApplicationRejectReason.None;
        app.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();

        // dupa withdraw recalculam status post
        var post = await _db.HelpPosts.FirstAsync(p => p.Id == postId);
        if (post.Status == HelpPostStatus.InProgress)
        {
            var acceptedCount = await _db.HelpPostApplications
                .Where(a => a.HelpPostId == postId && a.Status == ApplicationStatus.Accepted)
                .CountAsync();

            if (acceptedCount == 0)
            {
                post.Status = HelpPostStatus.Pending;
                post.UpdatedAt = DateTime.UtcNow;
                await _db.SaveChangesAsync();
            }
        }
    }

    public async Task AcceptApplication(Guid postId, Guid applicationId)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        var post = await _db.HelpPosts.FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null) throw new Exception("Postarea nu a fost găsită");

        if (post.CreatedByUserId != userId)
            throw new Exception("Nu ai permisiunea să accepți aplicații la această postare");

        if (post.Status is HelpPostStatus.Cancelled or HelpPostStatus.Completed or HelpPostStatus.NeedsReview)
            throw new Exception("Postarea nu poate primi acceptări în acest status");

        var app = await _db.HelpPostApplications.FirstOrDefaultAsync(a => a.Id == applicationId && a.HelpPostId == postId);
        if (app == null) throw new Exception("Aplicația nu a fost găsită");

        if (app.Status == ApplicationStatus.Withdrawn)
            throw new Exception("Aplicația a fost retrasă");

        // verificare nr acceptati
        var acceptedCount = await _db.HelpPostApplications
            .Where(a => a.HelpPostId == postId && a.Status == ApplicationStatus.Accepted)
            .CountAsync();

        if (acceptedCount >= post.NeededVolunteers)
            throw new Exception("S-au ocupat deja toate locurile");

        app.Status = ApplicationStatus.Accepted;
        app.RejectReason = ApplicationRejectReason.None;
        app.UpdatedAt = DateTime.UtcNow;


        var alreadyAssigned = await _db.HelpPostAssignments.AnyAsync(x => x.ApplicationId == app.Id);
        if (!alreadyAssigned)
        {
            _db.HelpPostAssignments.Add(new HelpPostAssignment
            {
                HelpPostId = postId,
                ApplicationId = app.Id
            });
        }

        await _db.SaveChangesAsync();

        // dupa accept daca s-a ajuns la quota se resping restul pending ca CapacityFilled 
        acceptedCount = await _db.HelpPostApplications
            .Where(a => a.HelpPostId == postId && a.Status == ApplicationStatus.Accepted)
            .CountAsync();

        if (acceptedCount >= post.NeededVolunteers)
        {
            var pendingOthers = await _db.HelpPostApplications
                .Where(a => a.HelpPostId == postId && a.Status == ApplicationStatus.Pending)
                .ToListAsync();

            foreach (var other in pendingOthers)
            {
                other.Status = ApplicationStatus.Rejected;
                other.RejectReason = ApplicationRejectReason.CapacityFilled;
                other.UpdatedAt = DateTime.UtcNow;
            }
        }

        // status post
        post.Status = HelpPostStatus.InProgress;
        post.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task CancelPost(Guid postId)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        var post = await _db.HelpPosts.FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null) throw new Exception("Postarea nu a fost găsită");

        if (post.CreatedByUserId != userId)
            throw new Exception("Nu ai permisiunea să anulezi această postare");

        if (post.Status == HelpPostStatus.Completed)
            throw new Exception("Postarea este deja finalizată");

        post.Status = HelpPostStatus.Cancelled;
        post.UpdatedAt = DateTime.UtcNow;

        await _db.SaveChangesAsync();
    }

    public async Task<object> MyPosts(int page, int pageSize, string? status)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.HelpPosts.AsNoTracking()
            .Where(p => p.CreatedByUserId == userId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<HelpPostStatus>(status, true, out var s))
            q = q.Where(p => p.Status == s);

        var total = await q.CountAsync();

        var items = await q.OrderByDescending(p => p.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(p => new
            {
                p.Id,
                Type = p.Type.ToString(),
                Status = p.Status.ToString(),
                Category = p.Category.ToString(),
                p.Title,
                p.City,
                p.Area,
                p.StartsAt,
                p.EndsAt,
                p.NeededVolunteers,
                p.CreatedAt
            })
            .ToListAsync();

        return new { total, page, pageSize, items };
    }

    public async Task<object> MyApplications(int page, int pageSize, string? status)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        page = Math.Max(1, page);
        pageSize = Math.Clamp(pageSize, 1, 100);

        var q = _db.HelpPostApplications.AsNoTracking()
            .Where(a => a.ApplicantUserId == userId);

        if (!string.IsNullOrWhiteSpace(status) && Enum.TryParse<ApplicationStatus>(status, true, out var s))
            q = q.Where(a => a.Status == s);

        var total = await q.CountAsync();

        var items = await q.OrderByDescending(a => a.CreatedAt)
            .Skip((page - 1) * pageSize)
            .Take(pageSize)
            .Select(a => new
            {
                a.Id,
                a.HelpPostId,
                PostTitle = a.HelpPost.Title,
                PostStatus = a.HelpPost.Status.ToString(),
                a.Status,
                RejectReason = a.RejectReason.ToString(),
                a.Message,
                a.AvailabilityNote,
                a.CreatedAt
            })
            .ToListAsync();

        return new { total, page, pageSize, items };
    }

    private static void ValidateRating(int v, string field)
    {
        if (v < 1 || v > 5) throw new Exception($"{field} trebuie să fie între 1 și 5");
    }

    public async Task SubmitFeedback(Guid postId, DTOs.Feedback.SubmitFeedbackRequest req)
    {
        var fromUserId = _current.UserId ?? throw new Exception("Neautentificat");

        ValidateRating(req.Safety, "Siguranță");
        ValidateRating(req.Communication, "Comunicare");
        ValidateRating(req.Reliability, "Încredere");

        var post = await _db.HelpPosts.FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null) throw new Exception("Postarea nu a fost găsită");

        if (post.Status != HelpPostStatus.InProgress)
            throw new Exception("Feedback-ul se poate trimite doar pentru postări în desfășurare");

        var app = await _db.HelpPostApplications
            .Include(a => a.ApplicantUser)
            .FirstOrDefaultAsync(a => a.Id == req.ApplicationId && a.HelpPostId == postId);

        if (app == null) throw new Exception("Aplicația nu a fost găsită");

        if (app.Status != ApplicationStatus.Accepted)
            throw new Exception("Poți lăsa feedback doar pentru aplicații acceptate");

        // cine are voie să trimită feedback
        var isCreator = post.CreatedByUserId == fromUserId;
        var isVolunteer = app.ApplicantUserId == fromUserId;

        if (!isCreator && !isVolunteer)
            throw new Exception("Nu ai permisiunea să trimiți feedback pentru această activitate");

        // catre cine e feedback-ul
        var toUserId = isCreator ? app.ApplicantUserId : post.CreatedByUserId;

        // prevenire duplicate
        var exists = await _db.HelpPostFeedbacks.AnyAsync(f =>
            f.HelpPostId == postId &&
            f.ApplicationId == app.Id &&
            f.FromUserId == fromUserId &&
            f.ToUserId == toUserId);

        if (exists)
            throw new Exception("Ai trimis deja feedback pentru această aplicație");

        _db.HelpPostFeedbacks.Add(new HelpPostFeedback
        {
            HelpPostId = postId,
            ApplicationId = app.Id,
            FromUserId = fromUserId,
            ToUserId = toUserId,
            Safety = req.Safety,
            Communication = req.Communication,
            Reliability = req.Reliability,
            Comment = string.IsNullOrWhiteSpace(req.Comment) ? null : req.Comment.Trim()
        });

        await _db.SaveChangesAsync();
    }

    public async Task<object> FeedbackRequired(Guid postId)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        var post = await _db.HelpPosts.AsNoTracking().FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null) throw new Exception("Postarea nu a fost găsită");

        // doar creatorul sau voluntarii acceptați pot vedea
        var acceptedApps = await _db.HelpPostApplications.AsNoTracking()
            .Where(a => a.HelpPostId == postId && a.Status == ApplicationStatus.Accepted)
            .Select(a => new { a.Id, a.ApplicantUserId })
            .ToListAsync();

        var isCreator = post.CreatedByUserId == userId;
        var isAcceptedVolunteer = acceptedApps.Any(x => x.ApplicantUserId == userId);

        if (!isCreator && !isAcceptedVolunteer)
            throw new Exception("Nu ai acces la această informație");
        
        // pentru fiecare aplicație acceptată, trebuie 2 feedback-uri
        var required = new List<object>();

        foreach (var a in acceptedApps)
        {
            var creatorToVol = await _db.HelpPostFeedbacks.AsNoTracking().AnyAsync(f =>
                f.HelpPostId == postId && f.ApplicationId == a.Id &&
                f.FromUserId == post.CreatedByUserId && f.ToUserId == a.ApplicantUserId);

            var volToCreator = await _db.HelpPostFeedbacks.AsNoTracking().AnyAsync(f =>
                f.HelpPostId == postId && f.ApplicationId == a.Id &&
                f.FromUserId == a.ApplicantUserId && f.ToUserId == post.CreatedByUserId);

            required.Add(new
            {
                applicationId = a.Id,
                volunteerUserId = a.ApplicantUserId,
                creatorToVolunteer = creatorToVol,
                volunteerToCreator = volToCreator
            });
        }

        return new { postId, required };
    }

    public async Task FinalizePost(Guid postId)
    {
        var userId = _current.UserId ?? throw new Exception("Neautentificat");

        var post = await _db.HelpPosts.FirstOrDefaultAsync(p => p.Id == postId);
        if (post == null) throw new Exception("Postarea nu a fost găsită");

        if (post.CreatedByUserId != userId)
            throw new Exception("Doar creatorul poate finaliza postarea");

        if (post.Status != HelpPostStatus.InProgress)
            throw new Exception("Doar postările în desfășurare pot fi finalizate");

        var acceptedApps = await _db.HelpPostApplications.AsNoTracking()
            .Where(a => a.HelpPostId == postId && a.Status == ApplicationStatus.Accepted)
            .Select(a => new { a.Id, a.ApplicantUserId })
            .ToListAsync();

        if (acceptedApps.Count == 0)
            throw new Exception("Nu există voluntari acceptați pentru această postare");

        foreach (var a in acceptedApps)
        {
            var creatorToVol = await _db.HelpPostFeedbacks.AsNoTracking().AnyAsync(f =>
                f.HelpPostId == postId && f.ApplicationId == a.Id &&
                f.FromUserId == post.CreatedByUserId && f.ToUserId == a.ApplicantUserId);

            var volToCreator = await _db.HelpPostFeedbacks.AsNoTracking().AnyAsync(f =>
                f.HelpPostId == postId && f.ApplicationId == a.Id &&
                f.FromUserId == a.ApplicantUserId && f.ToUserId == post.CreatedByUserId);

            if (!creatorToVol || !volToCreator)
                throw new Exception("Nu se poate finaliza, lipsesc feedback-uri obligatorii");
        }

        post.Status = HelpPostStatus.Completed;
        post.UpdatedAt = DateTime.UtcNow;
        await _db.SaveChangesAsync();
    }

}
