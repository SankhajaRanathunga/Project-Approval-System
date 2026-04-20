using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Data;
using ProjectApprovalSystem.Interfaces;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;

namespace ProjectApprovalSystem.Services
{
    public class ProposalService : IProposalService
    {
        private readonly ApplicationDbContext _context;

        public ProposalService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<ProjectProposal> CreateProposalAsync(ProjectProposal proposal)
        {
            proposal.SubmittedAt = DateTime.UtcNow;
            proposal.UpdatedAt = DateTime.UtcNow;
            proposal.Status = ProjectStatus.Pending;
            proposal.IsMatched = false;

            _context.ProjectProposals.Add(proposal);
            await _context.SaveChangesAsync();
            return proposal;
        }

        public async Task<ProjectProposal?> GetProposalByIdAsync(int id)
        {
            return await _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Include(p => p.Student)
                .ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(p => p.Id == id);
        }

        public async Task<IEnumerable<ProjectProposal>> GetStudentProposalsAsync(string userId)
        {
            return await _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Where(p => p.Student!.UserId == userId)
                .OrderByDescending(p => p.SubmittedAt)
                .ToListAsync();
        }

        public async Task<IEnumerable<ProjectProposal>> GetAllProposalsAsync()
        {
            return await _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Include(p => p.Student)
                .ThenInclude(s => s!.User)
                .ToListAsync();
        }

        public async Task<bool> UpdateProposalAsync(ProjectProposal proposal)
        {
            var existing = await _context.ProjectProposals.FindAsync(proposal.Id);
            if (existing == null || existing.IsMatched) return false;

            existing.Title = proposal.Title;
            existing.Abstract = proposal.Abstract;
            existing.TechnicalStack = proposal.TechnicalStack;
            existing.ResearchAreaId = proposal.ResearchAreaId;
            existing.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> WithdrawProposalAsync(int id, string userId)
        {
            var proposal = await _context.ProjectProposals
                .Include(p => p.Student)
                .FirstOrDefaultAsync(p => p.Id == id && p.Student!.UserId == userId);

            if (proposal == null || proposal.IsMatched) return false;

            proposal.Status = ProjectStatus.Withdrawn;
            proposal.WithdrawnAt = DateTime.UtcNow;
            proposal.UpdatedAt = DateTime.UtcNow;

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CanEditOrWithdrawAsync(int id)
        {
            var proposal = await _context.ProjectProposals.FindAsync(id);
            return proposal != null && !proposal.IsMatched && proposal.Status != ProjectStatus.Withdrawn;
        }
    }
}
