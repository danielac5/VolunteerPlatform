using Microsoft.EntityFrameworkCore;
using VolunteerPlatform.Domain.Entities;

namespace VolunteerPlatform.Data;

public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<User> Users => Set<User>();
    public DbSet<EmailVerificationCode> EmailVerificationCodes => Set<EmailVerificationCode>();
    public DbSet<Organization> Organizations => Set<Organization>();
    public DbSet<OrganizationMember> OrganizationMembers => Set<OrganizationMember>();
    public DbSet<OrganizationInvite> OrganizationInvites => Set<OrganizationInvite>();
    public DbSet<HelpPost> HelpPosts => Set<HelpPost>();
    public DbSet<HelpPostApplication> HelpPostApplications => Set<HelpPostApplication>();
    public DbSet<HelpPostAssignment> HelpPostAssignments => Set<HelpPostAssignment>();
    public DbSet<HelpPostFeedback> HelpPostFeedbacks => Set<HelpPostFeedback>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<User>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.FullName)
                .HasMaxLength(200)
                .IsRequired();

            e.Property(x => x.Email)
                .HasMaxLength(200)
                .IsRequired();

            e.HasIndex(x => x.Email)
                .IsUnique();

            e.Property(x => x.PhoneNumber)
                .HasMaxLength(20)
                .IsRequired();

            e.HasIndex(x => x.PhoneNumber)
                .IsUnique();

            e.Property(x => x.PasswordHash)
                .IsRequired();

            e.Property(x => x.DateOfBirth)
                .IsRequired();

            e.Property(x => x.IsMinor)
                .IsRequired();

            e.Property(x => x.EmailConfirmed)
                .IsRequired();

            e.Property(x => x.IsBlocked)
                .IsRequired();
        });

        modelBuilder.Entity<EmailVerificationCode>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.CodeHash)
                .IsRequired();

            e.Property(x => x.Purpose)
                .IsRequired();

            e.Property(x => x.Attempts)
                .IsRequired();

            e.Property(x => x.MaxAttempts)
                .IsRequired();

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<Organization>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Name).HasMaxLength(200).IsRequired();
            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.Property(x => x.PhoneNumber).HasMaxLength(20).IsRequired();

            e.Property(x => x.Idno).HasMaxLength(20).IsRequired();

            e.HasIndex(x => x.Idno).IsUnique();

            e.Property(x => x.Category).HasMaxLength(100);
            e.Property(x => x.Address).HasMaxLength(300);

            e.HasIndex(x => x.Email).IsUnique();
        });

        modelBuilder.Entity<OrganizationMember>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.User)
                .WithMany()
                .HasForeignKey(x => x.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasIndex(x => new { x.OrganizationId, x.UserId }).IsUnique();
        });

        modelBuilder.Entity<OrganizationInvite>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Email).HasMaxLength(200).IsRequired();
            e.HasIndex(x => new { x.OrganizationId, x.Email, x.IsUsed });

            e.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<HelpPost>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Title).HasMaxLength(200).IsRequired();
            e.Property(x => x.Description).HasMaxLength(2000).IsRequired();

            e.Property(x => x.City).HasMaxLength(100).IsRequired();
            e.Property(x => x.Area).HasMaxLength(100).IsRequired();
            e.Property(x => x.AddressApprox).HasMaxLength(300);

            e.Property(x => x.NeededVolunteers).IsRequired();

            e.Property(x => x.NeedsReviewReason).HasMaxLength(300);

            e.HasOne(x => x.CreatedByUser)
                .WithMany()
                .HasForeignKey(x => x.CreatedByUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.Organization)
                .WithMany()
                .HasForeignKey(x => x.OrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.Type, x.Status });
        });

        modelBuilder.Entity<HelpPostApplication>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Message).HasMaxLength(500);
            e.Property(x => x.AvailabilityNote).HasMaxLength(300);

            e.HasIndex(x => new { x.HelpPostId, x.ApplicantUserId }).IsUnique();

            e.HasOne(x => x.HelpPost)
                .WithMany()
                .HasForeignKey(x => x.HelpPostId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.ApplicantUser)
                .WithMany()
                .HasForeignKey(x => x.ApplicantUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ApplicantOrganization)
                .WithMany()
                .HasForeignKey(x => x.ApplicantOrganizationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasIndex(x => new { x.HelpPostId, x.Status });
        });

        modelBuilder.Entity<HelpPostAssignment>(e =>
        {
            e.HasKey(x => x.Id);

            e.HasIndex(x => x.ApplicationId).IsUnique();
            e.HasIndex(x => x.HelpPostId); 

            e.HasOne(x => x.HelpPost)
                .WithMany()
                .HasForeignKey(x => x.HelpPostId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Application)
                .WithMany()
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);
        });

        modelBuilder.Entity<HelpPostFeedback>(e =>
        {
            e.HasKey(x => x.Id);

            e.Property(x => x.Comment).HasMaxLength(500);

            e.HasIndex(x => new { x.HelpPostId, x.ApplicationId, x.FromUserId, x.ToUserId }).IsUnique();

            e.HasOne(x => x.HelpPost)
                .WithMany()
                .HasForeignKey(x => x.HelpPostId)
                .OnDelete(DeleteBehavior.Cascade);

            e.HasOne(x => x.Application)
                .WithMany()
                .HasForeignKey(x => x.ApplicationId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.FromUser)
                .WithMany()
                .HasForeignKey(x => x.FromUserId)
                .OnDelete(DeleteBehavior.Restrict);

            e.HasOne(x => x.ToUser)
                .WithMany()
                .HasForeignKey(x => x.ToUserId)
                .OnDelete(DeleteBehavior.Restrict);
        });

    }
}
