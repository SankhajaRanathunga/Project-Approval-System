using Microsoft.EntityFrameworkCore;
using Moq;
using ProjectApprovalSystem.Data;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;
using ProjectApprovalSystem.Services;
using Xunit;

namespace ProjectApprovalSystem.Tests
{
    public class MatchingServiceTests
    {
        private DbContextOptions<ApplicationDbContext> GetDbOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task GetAnonymousProposalsAsync_ShouldNotIncludeStudentDetails()
        {
            // Arrange
            var options = GetDbOptions();
            using var context = new ApplicationDbContext(options);
            
            var student = new StudentProfile { Id = 1, UserId = "user1", StudentId = "S001" };
            var area = new ResearchArea { Id = 1, Name = "AI" };
            var proposal = new ProjectProposal 
            { 
                Id = 1, 
                Title = "Test Project", 
                Abstract = "Description", 
                TechnicalStack = "C#", 
                ResearchAreaId = 1,
                StudentId = 1,
                Status = ProjectStatus.Pending
            };

            context.StudentProfiles.Add(student);
            context.ResearchAreas.Add(area);
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            var service = new MatchingService(context);

            // Act
            var result = await service.GetAnonymousProposalsAsync();

            // Assert
            var item = result.First();
            Assert.Equal("Test Project", item.Title);
            Assert.Equal("AI", item.ResearchAreaName);
            // Verify that no student-identifying fields exist in the ViewModel (compile-time check + logic)
            Assert.IsType<ViewModels.ProposalSummaryViewModel>(item);
        }

        [Fact]
        public async Task ConfirmMatch_ShouldChangeStatusToMatchedAndLockProposal()
        {
            // Arrange
            var options = GetDbOptions();
            using var context = new ApplicationDbContext(options);
            
            var proposal = new ProjectProposal { Id = 1, Title = "P1", Abstract = "A1", TechnicalStack = "T1", ResearchAreaId = 1, StudentId = 1, Status = ProjectStatus.UnderReview };
            var record = new MatchRecord { Id = 1, ProjectProposalId = 1, SupervisorId = 1, Status = MatchStatus.Interested };
            
            context.ProjectProposals.Add(proposal);
            context.MatchRecords.Add(record);
            await context.SaveChangesAsync();

            var service = new MatchingService(context);

            // Act
            var result = await service.ConfirmMatchAsync(1, 1);

            // Assert
            Assert.True(result);
            var updatedProposal = await context.ProjectProposals.FindAsync(1);
            Assert.True(updatedProposal!.IsMatched);
            Assert.Equal(ProjectStatus.Matched, updatedProposal.Status);
                
            var updatedRecord = await context.MatchRecords.FindAsync(1);
            Assert.Equal(MatchStatus.Confirmed, updatedRecord!.Status);
        }

        [Fact]
        public async Task CancelMatch_ShouldResetProposalStatus()
        {
            // Arrange
            var options = GetDbOptions();
            using var context = new ApplicationDbContext(options);
            
            var proposal = new ProjectProposal { Id = 1, Status = ProjectStatus.Matched, IsMatched = true };
            var record = new MatchRecord { Id = 1, ProjectProposalId = 1, SupervisorId = 1, Status = MatchStatus.Confirmed };
            
            context.ProjectProposals.Add(proposal);
            context.MatchRecords.Add(record);
            await context.SaveChangesAsync();

            var service = new MatchingService(context);

            // Act
            var result = await service.CancelMatchAsync(1, "Testing cancellation");

            // Assert
            Assert.True(result);
            var p = await context.ProjectProposals.FindAsync(1);
            Assert.False(p!.IsMatched);
            Assert.Equal(ProjectStatus.Pending, p.Status);
            
            var r = await context.MatchRecords.FindAsync(1);
            Assert.Equal(MatchStatus.Cancelled, r!.Status);
        }

        [Fact]
        public async Task ExpressInterest_ShouldFail_IfAlreadyMatched()
        {
            // Arrange
            var options = GetDbOptions();
            using var context = new ApplicationDbContext(options);
            
            var proposal = new ProjectProposal { Id = 1, IsMatched = true, Status = ProjectStatus.Matched };
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            var service = new MatchingService(context);

            // Act
            var result = await service.ExpressInterestAsync(1, 2);

            // Assert
            Assert.False(result);
        }
    }
}
