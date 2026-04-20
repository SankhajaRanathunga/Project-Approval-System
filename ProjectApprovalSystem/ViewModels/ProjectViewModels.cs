using ProjectApprovalSystem.Models.Enums;

namespace ProjectApprovalSystem.ViewModels
{
    public class ProposalSummaryViewModel
    {
        public int Id { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Abstract { get; set; } = string.Empty;
        public string TechnicalStack { get; set; } = string.Empty;
        public string ResearchAreaName { get; set; } = string.Empty;
        public ProjectStatus Status { get; set; }
        public DateTime SubmittedAt { get; set; }
    }

    public class MatchDetailsViewModel
    {
        public int ProposalId { get; set; }
        public string Title { get; set; } = string.Empty;
        public string Abstract { get; set; } = string.Empty;
        
        // Identity details (only revealed after match)
        public string StudentFullName { get; set; } = string.Empty;
        public string StudentEmail { get; set; } = string.Empty;
        public string StudentId { get; set; } = string.Empty;
        
        public string SupervisorFullName { get; set; } = string.Empty;
        public string SupervisorEmail { get; set; } = string.Empty;
        public string SupervisorStaffId { get; set; } = string.Empty;
        
        public DateTime MatchedAt { get; set; }
        public ProjectStatus Status { get; set; }
    }
}
