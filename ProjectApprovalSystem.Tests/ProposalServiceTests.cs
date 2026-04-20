using Microsoft.EntityFrameworkCore;
using Moq;
using ProjectApprovalSystem.Data;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;
using ProjectApprovalSystem.Services;
using Xunit;

namespace ProjectApprovalSystem.Tests
{
    public class ProposalServiceTests
    {
        private DbContextOptions<ApplicationDbContext> GetDbOptions()
        {
            return new DbContextOptionsBuilder<ApplicationDbContext>()
                .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
                .Options;
        }

        [Fact]
        public async Task CreateProposal_ShouldSetInitialStatus()
        {
            // Arrange
            var options = GetDbOptions();
            using var context = new ApplicationDbContext(options);
            var service = new ProposalService(context);
            var proposal = new ProjectProposal { Title = "Test", Abstract = "Abstract", TechnicalStack = "C#" };

            // Act
            var result = await service.CreateProposalAsync(proposal);

            // Assert
            Assert.Equal(ProjectStatus.Pending, result.Status);
            Assert.False(result.IsMatched);
            Assert.True(result.SubmittedAt <= DateTime.UtcNow);
        }

        [Fact]
        public async Task UpdateProposal_ShouldFailIfMatched()
        {
            // Arrange
            var options = GetDbOptions();
            using var context = new ApplicationDbContext(options);
            var proposal = new ProjectProposal { Id = 1, Title = "Old", IsMatched = true };
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            var service = new ProposalService(context);
            var updated = new ProjectProposal { Id = 1, Title = "New" };

            // Act
            var result = await service.UpdateProposalAsync(updated);

            // Assert
            Assert.False(result);
            var p = await context.ProjectProposals.FindAsync(1);
            Assert.Equal("Old", p!.Title);
        }

        [Fact]
        public async Task WithdrawProposal_ShouldSetStatusToWithdrawn()
        {
            // Arrange
            var options = GetDbOptions();
            using var context = new ApplicationDbContext(options);
            var student = new StudentProfile { Id = 1, UserId = "user1" };
            var proposal = new ProjectProposal { Id = 1, StudentId = 1, Status = ProjectStatus.Pending, IsMatched = false };
            
            context.StudentProfiles.Add(student);
            context.ProjectProposals.Add(proposal);
            await context.SaveChangesAsync();

            var service = new ProposalService(context);

            // Act
            var result = await service.WithdrawProposalAsync(1, "user1");

            // Assert
            Assert.True(result);
            var p = await context.ProjectProposals.FindAsync(1);
            Assert.Equal(ProjectStatus.Withdrawn, p!.Status);
            Assert.NotNull(p.WithdrawnAt);
        }
    }
}
