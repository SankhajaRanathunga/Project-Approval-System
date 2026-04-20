using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Data;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;
using ProjectApprovalSystem.Services;
using Xunit;

namespace ProjectApprovalSystem.Tests
{
    public class IntegrationTests
    {
        private DbContextOptions<ApplicationDbContext> GetDbOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task FullMatchingFlow_EndToEnd_IntegrationTest()
        {
            // 1. Setup Data
            var options = GetDbOptions();
            using var context = new ApplicationDbContext(options);
            
            var area = new ResearchArea { Name = "AI" };
            context.ResearchAreas.Add(area);
            
            var studentUser = new ApplicationUser { Id = "s-user", FullName = "John Student", Email = "john@test.com" };
            var studentProfile = new StudentProfile { UserId = "s-user", StudentId = "S001" };
            
            context.Users.Add(studentUser);
            context.StudentProfiles.Add(studentProfile);
            
            var supervisorUser = new ApplicationUser { Id = "t-user", FullName = "Dr. Smith", Email = "smith@test.com" };
            var supervisorProfile = new SupervisorProfile { UserId = "t-user", StaffId = "T001" };
            
            context.Users.Add(supervisorUser);
            context.SupervisorProfiles.Add(supervisorProfile);
            
            await context.SaveChangesAsync();

            // 2. Student Submits
            var proposalService = new ProposalService(context);
            var proposal = new ProjectProposal 
            { 
                Title = "AI Project", 
                Abstract = "Cool AI thing", 
                TechnicalStack = "Python",
                ResearchAreaId = area.Id,
                StudentId = studentProfile.Id
            };
            await proposalService.CreateProposalAsync(proposal);

            // 3. Supervisor Reviews Anonymously
            var matchingService = new MatchingService(context);
            var anonymousProposals = await matchingService.GetAnonymousProposalsAsync();
            
            Assert.Single(anonymousProposals);
            var anon = anonymousProposals.First();
            Assert.Equal("AI Project", anon.Title);
            
            // 4. Supervisor Expresses Interest
            await matchingService.ExpressInterestAsync(proposal.Id, supervisorProfile.Id);
            
            var updatedProposal = await context.ProjectProposals.FindAsync(proposal.Id);
            Assert.Equal(ProjectStatus.UnderReview, updatedProposal!.Status);

            // 5. Supervisor Confirms Match (Identity Reveal)
            await matchingService.ConfirmMatchAsync(proposal.Id, supervisorProfile.Id);
            
            // 6. Final Assertions
            var finalProposal = await context.ProjectProposals
                .Include(p => p.Student).ThenInclude(s => s!.User)
                .Include(p => p.MatchRecords)
                .FirstOrDefaultAsync(p => p.Id == proposal.Id);
            
            Assert.True(finalProposal!.IsMatched);
            Assert.Equal(ProjectStatus.Matched, finalProposal.Status);
            Assert.NotNull(finalProposal.RevealedAt);
            
            var details = await matchingService.GetMatchDetailsAsync(proposal.Id);
            Assert.NotNull(details);
            Assert.Equal("John Student", details!.StudentFullName);
            Assert.Equal("Dr. Smith", details!.SupervisorFullName);
        }
    }
}
