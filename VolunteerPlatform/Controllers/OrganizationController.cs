using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Data;
using VolunteerPlatform.Domain.Entities;
using VolunteerPlatform.Domain.Enums;
using VolunteerPlatform.DTOs.Organizations;
using VolunteerPlatform.Services;

namespace VolunteerPlatform.Controllers;

[ApiController]
[Route("api/[controller]")]
[Authorize(Policy = "VerifiedUser")]
public class OrganizationsController : ControllerBase
{
    private readonly AppDbContext _db;
    private readonly CurrentUserService _current;

    public OrganizationsController(AppDbContext db, CurrentUserService current)
    {
        _db = db;
        _current = current;
    }

    private static string NormalizeEmail(string email) => email.Trim().ToLowerInvariant();

    // creare organizație
    [HttpPost]
    public async Task<IActionResult> Create([FromBody] CreateOrganizationRequest request)
    {
        var userId = _current.UserId!.Value;

        var org = new Organization
        {
            Name = request.Name.Trim(),
            Email = NormalizeEmail(request.Email),
            PhoneNumber = request.PhoneNumber.Trim(),
            Description = request.Description?.Trim(),
            Address = request.Address?.Trim(),
            Idno = request.Idno?.Trim(),
            Category = request.Category?.Trim(),
            VerificationStatus = OrganizationVerificationStatus.Pending
        };

        _db.Organizations.Add(org);

        _db.OrganizationMembers.Add(new OrganizationMember
        {
            OrganizationId = org.Id,
            UserId = userId,
            Role = OrgRole.Owner
        });

        await _db.SaveChangesAsync();

        return Ok(new { message = "Organizația a fost creată", organizationId = org.Id });
    }

    // identificare utilizator in organizatii
    [HttpGet("my")]
    public async Task<IActionResult> MyOrganizations()
    {
        var userId = _current.UserId!.Value;

        var items = await _db.OrganizationMembers.AsNoTracking()
            .Where(m => m.UserId == userId && m.IsActive)
            .Select(m => new
            {
                m.OrganizationId,
                OrganizationName = m.Organization.Name,
                OrgEmail = m.Organization.Email,
                m.Role,
                VerificationStatus = m.Organization.VerificationStatus.ToString()
            })
            .ToListAsync();

        return Ok(items);
    }

    // se verifica rolul utilizatorului
    private async Task<OrgRole?> GetMyRole(Guid orgId)
    {
        var userId = _current.UserId!.Value;

        var role = await _db.OrganizationMembers.AsNoTracking()
            .Where(m => m.OrganizationId == orgId && m.UserId == userId && m.IsActive)
            .Select(m => (OrgRole?)m.Role)
            .FirstOrDefaultAsync();

        return role;
    }

    private static bool IsOrgAdmin(OrgRole role) => role == OrgRole.Owner || role == OrgRole.Admin;

    // listare membri
    [HttpGet("{orgId:guid}/members")]
    public async Task<IActionResult> GetMembers(Guid orgId)
    {
        var myRole = await GetMyRole(orgId);
        if (myRole == null) return Forbid();

        var members = await _db.OrganizationMembers.AsNoTracking()
            .Where(m => m.OrganizationId == orgId && m.IsActive)
            .Select(m => new
            {
                m.Id,
                m.UserId,
                m.User.FullName,
                m.User.Email,
                m.Role,
                m.JoinedAt
            })
            .ToListAsync();

        return Ok(members);
    }

    // invitare membri
    [HttpPost("{orgId:guid}/invite")]
    public async Task<IActionResult> Invite(Guid orgId, [FromBody] InviteMemberRequest request)
    {
        var myRole = await GetMyRole(orgId);
        if (myRole == null || !IsOrgAdmin(myRole.Value)) return Forbid();

        var email = NormalizeEmail(request.Email);

        if (!Enum.TryParse<OrgRole>(request.Role, true, out var role))
            return BadRequest(new { message = "Rol invalid. Folosește: Viewer, Coordinator, Admin" });

        if (role == OrgRole.Owner)
            return BadRequest(new { message = "Rolul Owner nu se oferă prin invitație" });

        // daca e deja membru nu se invita
        var alreadyMember = await _db.OrganizationMembers.AnyAsync(m =>
            m.OrganizationId == orgId && m.User.Email == email && m.IsActive);

        if (alreadyMember)
            return BadRequest(new { message = "Utilizatorul este deja membru în organizație" });

        // invalidare invitatiile vechi nefolosite pentru acelasi email
        var oldInvites = await _db.OrganizationInvites
            .Where(i => i.OrganizationId == orgId && i.Email == email && !i.IsUsed && i.ExpiresAt > DateTime.UtcNow)
            .ToListAsync();

        foreach (var inv in oldInvites) inv.IsUsed = true;

        var invite = new OrganizationInvite
        {
            OrganizationId = orgId,
            Email = email,
            Role = role,
            CreatedByUserId = _current.UserId!.Value,
            ExpiresAt = DateTime.UtcNow.AddDays(7),
            IsUsed = false
        };

        _db.OrganizationInvites.Add(invite);
        await _db.SaveChangesAsync();

        // invitatie in consola
        Console.WriteLine($"[ORG INVITE] org={orgId} email={email} role={role} inviteId={invite.Id}");

        return Ok(new { message = "Invitația a fost creată", inviteId = invite.Id });
    }

    // acceptare invitatie
    [HttpPost("invites/{inviteId:guid}/accept")]
    public async Task<IActionResult> AcceptInvite(Guid inviteId)
    {
        var userId = _current.UserId!.Value;
        var myEmail = NormalizeEmail(_current.Email ?? "");

        var invite = await _db.OrganizationInvites
            .FirstOrDefaultAsync(i => i.Id == inviteId);

        if (invite == null)
            return NotFound(new { message = "Invitația nu a fost găsită" });

        if (invite.IsUsed || invite.ExpiresAt <= DateTime.UtcNow)
            return BadRequest(new { message = "Invitația este expirată sau deja folosită" });

        if (invite.Email != myEmail)
            return Forbid();

        // daca intre timp a devenit membru se marcheaza invite used și out
        var exists = await _db.OrganizationMembers.AnyAsync(m =>
            m.OrganizationId == invite.OrganizationId && m.UserId == userId && m.IsActive);

        invite.IsUsed = true;

        if (!exists)
        {
            _db.OrganizationMembers.Add(new OrganizationMember
            {
                OrganizationId = invite.OrganizationId,
                UserId = userId,
                Role = invite.Role,
                IsActive = true
            });
        }

        await _db.SaveChangesAsync();

        return Ok(new { message = "Invitația a fost acceptată" });
    }

    // refuzare invitatie
    [HttpPost("invites/{inviteId:guid}/decline")]
    public async Task<IActionResult> DeclineInvite(Guid inviteId)
    {
        var myEmail = NormalizeEmail(_current.Email ?? "");

        var invite = await _db.OrganizationInvites.FirstOrDefaultAsync(i => i.Id == inviteId);
        if (invite == null)
            return NotFound(new { message = "Invitația nu a fost găsită" });

        if (invite.Email != myEmail)
            return Forbid();

        invite.IsUsed = true;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Invitația a fost respinsă" });
    }

    // schimbare rol
    [HttpPatch("{orgId:guid}/members/{memberId:guid}/role")]
    public async Task<IActionResult> ChangeMemberRole(Guid orgId, Guid memberId, [FromBody] ChangeMemberRoleRequest request)
    {
        var myRole = await GetMyRole(orgId);
        if (myRole == null || !IsOrgAdmin(myRole.Value)) return Forbid();

        if (!Enum.TryParse<OrgRole>(request.Role, true, out var newRole))
            return BadRequest(new { message = "Rol invalid. Folosește: Viewer, Coordinator, Admin" });

        if (newRole == OrgRole.Owner)
            return BadRequest(new { message = "Rolul Owner nu se setează aici" });

        var member = await _db.OrganizationMembers.FirstOrDefaultAsync(m => m.Id == memberId && m.OrganizationId == orgId);
        if (member == null)
            return NotFound(new { message = "Membrul nu a fost găsit" });

        // owner nu poate fi modificat
        if (member.Role == OrgRole.Owner)
            return BadRequest(new { message = "Owner-ul nu poate fi modificat aici" });

        member.Role = newRole;
        await _db.SaveChangesAsync();

        return Ok(new { message = $"Rol schimbat în {member.Role}" });
    }

    // stergere membru
    [HttpDelete("{orgId:guid}/members/{memberId:guid}")]
    public async Task<IActionResult> RemoveMember(Guid orgId, Guid memberId)
    {
        var myRole = await GetMyRole(orgId);
        if (myRole == null || !IsOrgAdmin(myRole.Value)) return Forbid();

        var member = await _db.OrganizationMembers.FirstOrDefaultAsync(m => m.Id == memberId && m.OrganizationId == orgId);
        if (member == null)
            return NotFound(new { message = "Membrul nu a fost găsit" });

        if (member.Role == OrgRole.Owner)
            return BadRequest(new { message = "Owner-ul nu poate fi eliminat" });

        member.IsActive = false;
        await _db.SaveChangesAsync();

        return Ok(new { message = "Membrul a fost eliminat" });
    }
}
