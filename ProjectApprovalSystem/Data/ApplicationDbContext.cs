using Microsoft.AspNetCore.Identity;
using Microsoft.AspNetCore.Identity.EntityFrameworkCore;
using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Models;

namespace ProjectApprovalSystem.Data
{
    public class ApplicationDbContext : IdentityDbContext<ApplicationUser, IdentityRole, string>
    {
        public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options)
            : base(options)
        {
        }

        public DbSet<StudentProfile> StudentProfiles { get; set; } = null!;
        public DbSet<SupervisorProfile> SupervisorProfiles { get; set; } = null!;
        public DbSet<ResearchArea> ResearchAreas { get; set; } = null!;
        public DbSet<SupervisorResearchArea> SupervisorResearchAreas { get; set; } = null!;
        public DbSet<ProjectProposal> ProjectProposals { get; set; } = null!;
        public DbSet<MatchRecord> MatchRecords { get; set; } = null!;
        public DbSet<AuditLog> AuditLogs { get; set; } = null!;

        protected override void OnModelCreating(ModelBuilder builder)
        {
            base.OnModelCreating(builder);

            // Configure StudentProfile -> ApplicationUser
            builder.Entity<StudentProfile>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<StudentProfile>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure SupervisorProfile -> ApplicationUser
            builder.Entity<SupervisorProfile>()
                .HasOne(s => s.User)
                .WithOne()
                .HasForeignKey<SupervisorProfile>(s => s.UserId)
                .OnDelete(DeleteBehavior.Cascade);

            // Configure ProjectProposal relationships
            builder.Entity<ProjectProposal>()
                .HasOne(p => p.Student)
                .WithMany(s => s.Proposals)
                .HasForeignKey(p => p.StudentId)
                .OnDelete(DeleteBehavior.Restrict);

            builder.Entity<ProjectProposal>()
                .HasOne(p => p.ResearchArea)
                .WithMany()
                .HasForeignKey(p => p.ResearchAreaId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure MatchRecord relationships
            builder.Entity<MatchRecord>()
                .HasOne(m => m.ProjectProposal)
                .WithMany(p => p.MatchRecords)
                .HasForeignKey(m => m.ProjectProposalId)
                .OnDelete(DeleteBehavior.Cascade);

            builder.Entity<MatchRecord>()
                .HasOne(m => m.Supervisor)
                .WithMany()
                .HasForeignKey(m => m.SupervisorId)
                .OnDelete(DeleteBehavior.Restrict);

            // Configure SupervisorResearchArea (Many-to-Many join)
            builder.Entity<SupervisorResearchArea>()
                .HasOne(sra => sra.Supervisor)
                .WithMany(s => s.Expertise)
                .HasForeignKey(sra => sra.SupervisorId);

            builder.Entity<SupervisorResearchArea>()
                .HasOne(sra => sra.ResearchArea)
                .WithMany()
                .HasForeignKey(sra => sra.ResearchAreaId);
        }
    }
}
