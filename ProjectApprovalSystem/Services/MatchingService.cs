using Microsoft.EntityFrameworkCore;
using ProjectApprovalSystem.Data;
using ProjectApprovalSystem.Interfaces;
using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;
using ProjectApprovalSystem.ViewModels;

namespace ProjectApprovalSystem.Services
{
    public class MatchingService : IMatchingService
    {
        private readonly ApplicationDbContext _context;

        public MatchingService(ApplicationDbContext context)
        {
            _context = context;
        }

        public async Task<IEnumerable<ProposalSummaryViewModel>> GetAnonymousProposalsAsync(int? researchAreaId = null)
        {
            var query = _context.ProjectProposals
                .Include(p => p.ResearchArea)
                .Where(p => p.Status == ProjectStatus.Pending || p.Status == ProjectStatus.UnderReview)
                .Where(p => !p.IsMatched);

            if (researchAreaId.HasValue)
            {
                query = query.Where(p => p.ResearchAreaId == researchAreaId.Value);
            }

            // EXPLICIT PROJECTION to ensure NO student identity data is leaked
            return await query.Select(p => new ProposalSummaryViewModel
            {
                Id = p.Id,
                Title = p.Title,
                Abstract = p.Abstract,
                TechnicalStack = p.TechnicalStack,
                ResearchAreaName = p.ResearchArea != null ? p.ResearchArea.Name : "General",
                Status = p.Status,
                SubmittedAt = p.SubmittedAt
            }).ToListAsync();
        }

        public async Task<bool> ExpressInterestAsync(int proposalId, int supervisorId)
        {
            var proposal = await _context.ProjectProposals.FindAsync(proposalId);
            if (proposal == null || proposal.IsMatched || proposal.Status == ProjectStatus.Withdrawn) return false;

            proposal.Status = ProjectStatus.UnderReview;
            
            var matchRecord = new MatchRecord
            {
                ProjectProposalId = proposalId,
                SupervisorId = supervisorId,
                Status = MatchStatus.Interested,
                MatchedAt = DateTime.UtcNow
            };

            _context.MatchRecords.Add(matchRecord);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ConfirmMatchAsync(int proposalId, int supervisorId)
        {
            var proposal = await _context.ProjectProposals.FindAsync(proposalId);
            if (proposal == null || proposal.IsMatched) return false;

            var matchRecord = await _context.MatchRecords
                .FirstOrDefaultAsync(m => m.ProjectProposalId == proposalId && m.SupervisorId == supervisorId);
            
            if (matchRecord == null) return false;

            // Update Proposal
            proposal.IsMatched = true;
            proposal.Status = ProjectStatus.Matched;
            proposal.MatchedAt = DateTime.UtcNow;
            proposal.RevealedAt = DateTime.UtcNow;

            // Update Match Record
            matchRecord.Status = MatchStatus.Confirmed;
            matchRecord.RevealedAt = DateTime.UtcNow;

            // Cancel other interests if any
            var otherInterests = await _context.MatchRecords
                .Where(m => m.ProjectProposalId == proposalId && m.Id != matchRecord.Id)
                .ToListAsync();
            
            foreach (var interest in otherInterests)
            {
                interest.Status = MatchStatus.Cancelled;
                interest.Notes = "Project matched with another supervisor.";
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> ReassignMatchAsync(int proposalId, int oldSupervisorId, int newSupervisorId)
        {
            var proposal = await _context.ProjectProposals.FindAsync(proposalId);
            if (proposal == null) return false;

            var oldMatch = await _context.MatchRecords
                .FirstOrDefaultAsync(m => m.ProjectProposalId == proposalId && m.SupervisorId == oldSupervisorId && m.Status == MatchStatus.Confirmed);
            
            if (oldMatch == null) return false;

            oldMatch.Status = MatchStatus.Reassigned;
            oldMatch.Notes = $"Reassigned to supervisor ID {newSupervisorId}";

            var newMatch = new MatchRecord
            {
                ProjectProposalId = proposalId,
                SupervisorId = newSupervisorId,
                Status = MatchStatus.Confirmed,
                MatchedAt = DateTime.UtcNow,
                RevealedAt = DateTime.UtcNow,
                CreatedBy = "ModuleLeader"
            };

            _context.MatchRecords.Add(newMatch);
            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<bool> CancelMatchAsync(int proposalId, string reason)
        {
            var proposal = await _context.ProjectProposals.Include(p => p.MatchRecords).FirstOrDefaultAsync(p => p.Id == proposalId);
            if (proposal == null) return false;

            proposal.IsMatched = false;
            proposal.Status = ProjectStatus.Pending;
            proposal.MatchedAt = null;
            proposal.RevealedAt = null;

            foreach (var match in proposal.MatchRecords.Where(m => m.Status == MatchStatus.Confirmed || m.Status == MatchStatus.Interested))
            {
                match.Status = MatchStatus.Cancelled;
                match.Notes = reason;
            }

            await _context.SaveChangesAsync();
            return true;
        }

        public async Task<MatchDetailsViewModel?> GetMatchDetailsAsync(int proposalId)
        {
            var proposal = await _context.ProjectProposals
                .Include(p => p.Student).ThenInclude(s => s!.User)
                .Include(p => p.MatchRecords).ThenInclude(m => m.Supervisor).ThenInclude(s => s!.User)
                .FirstOrDefaultAsync(p => p.Id == proposalId);

            if (proposal == null || !proposal.IsMatched) return null;

            var confirmedMatch = proposal.MatchRecords.FirstOrDefault(m => m.Status == MatchStatus.Confirmed);
            if (confirmedMatch == null) return null;

            return new MatchDetailsViewModel
            {
                ProposalId = proposal.Id,
                Title = proposal.Title,
                Abstract = proposal.Abstract,
                Status = proposal.Status,
                MatchedAt = proposal.MatchedAt ?? DateTime.MinValue,
                
                StudentFullName = proposal.Student?.User?.FullName ?? "Unknown",
                StudentEmail = proposal.Student?.User?.Email ?? "Unknown",
                StudentId = proposal.Student?.StudentId ?? "Unknown",
                
                SupervisorFullName = confirmedMatch.Supervisor?.User?.FullName ?? "Unknown",
                SupervisorEmail = confirmedMatch.Supervisor?.User?.Email ?? "Unknown",
                SupervisorStaffId = confirmedMatch.Supervisor?.StaffId ?? "Unknown"
            };
        }
    }
}
