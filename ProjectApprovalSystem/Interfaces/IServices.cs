using ProjectApprovalSystem.Models;
using ProjectApprovalSystem.Models.Enums;
using ProjectApprovalSystem.ViewModels;

namespace ProjectApprovalSystem.Interfaces
{
    public interface IProposalService
    {
        Task<ProjectProposal> CreateProposalAsync(ProjectProposal proposal);
        Task<ProjectProposal?> GetProposalByIdAsync(int id);
        Task<IEnumerable<ProjectProposal>> GetStudentProposalsAsync(string userId);
        Task<IEnumerable<ProjectProposal>> GetAllProposalsAsync();
        Task<bool> UpdateProposalAsync(ProjectProposal proposal);
        Task<bool> WithdrawProposalAsync(int id, string userId);
        Task<bool> CanEditOrWithdrawAsync(int id);
    }

    public interface IMatchingService
    {
        Task<IEnumerable<ProposalSummaryViewModel>> GetAnonymousProposalsAsync(int? researchAreaId = null);
        Task<bool> ExpressInterestAsync(int proposalId, int supervisorId);
        Task<bool> ConfirmMatchAsync(int proposalId, int supervisorId);
        Task<bool> ReassignMatchAsync(int proposalId, int oldSupervisorId, int newSupervisorId);
        Task<bool> CancelMatchAsync(int proposalId, string reason);
        Task<MatchDetailsViewModel?> GetMatchDetailsAsync(int proposalId);
    }

    public interface IResearchAreaService
    {
        Task<IEnumerable<ResearchArea>> GetAllAsync();
        Task<ResearchArea?> GetByIdAsync(int id);
        Task<ResearchArea> CreateAsync(ResearchArea area);
        Task<bool> UpdateAsync(ResearchArea area);
        Task<bool> DeleteAsync(int id);
    }

    public interface IUserManagementService
    {
        Task<ApplicationUser?> GetUserByIdAsync(string id);
        Task<StudentProfile?> GetStudentProfileAsync(string userId);
        Task<SupervisorProfile?> GetSupervisorProfileAsync(string userId);
        Task<bool> CreateStudentAccountAsync(ApplicationUser user, string password, StudentProfile profile);
        Task<bool> CreateStudentAccountAsync(string userId, string fullName, string studentId);
        Task<bool> CreateSupervisorAccountAsync(ApplicationUser user, string password, SupervisorProfile profile);
        Task<bool> CreateSupervisorAccountAsync(string userId, string fullName, string staffId);
        Task<bool> UpdateSupervisorExpertiseAsync(int supervisorId, List<int> researchAreaIds);
        Task<IEnumerable<ApplicationUser>> GetAllUsersAsync();
    }
}
