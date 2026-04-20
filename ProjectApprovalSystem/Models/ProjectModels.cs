using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using ProjectApprovalSystem.Models.Enums;

namespace ProjectApprovalSystem.Models
{
    public class ProjectProposal
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [StringLength(200)]
        public string Title { get; set; } = string.Empty;

        [Required]
        public string Abstract { get; set; } = string.Empty;

        [Required]
        public string TechnicalStack { get; set; } = string.Empty;

        [Required]
        public int ResearchAreaId { get; set; }

        [ForeignKey("ResearchAreaId")]
        public virtual ResearchArea? ResearchArea { get; set; }

        [Required]
        public int StudentId { get; set; }

        [ForeignKey("StudentId")]
        public virtual StudentProfile? Student { get; set; }

        public ProjectStatus Status { get; set; } = ProjectStatus.Pending;

        public DateTime SubmittedAt { get; set; } = DateTime.UtcNow;

        public DateTime UpdatedAt { get; set; } = DateTime.UtcNow;

        public DateTime? WithdrawnAt { get; set; }

        public bool IsMatched { get; set; } = false;

        public DateTime? MatchedAt { get; set; }

        public DateTime? RevealedAt { get; set; }

        public virtual ICollection<MatchRecord> MatchRecords { get; set; } = new List<MatchRecord>();
    }

    public class MatchRecord
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public int ProjectProposalId { get; set; }

        [ForeignKey("ProjectProposalId")]
        public virtual ProjectProposal? ProjectProposal { get; set; }

        [Required]
        public int SupervisorId { get; set; }

        [ForeignKey("SupervisorId")]
        public virtual SupervisorProfile? Supervisor { get; set; }

        public DateTime MatchedAt { get; set; } = DateTime.UtcNow;

        public MatchStatus Status { get; set; } = MatchStatus.Interested;

        public DateTime? RevealedAt { get; set; }

        public string? CreatedBy { get; set; }

        public string? Notes { get; set; }
    }

    public class AuditLog
    {
        [Key]
        public int Id { get; set; }

        [Required]
        public string UserId { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string Action { get; set; } = string.Empty;

        [Required]
        [StringLength(100)]
        public string EntityName { get; set; } = string.Empty;

        public string? EntityId { get; set; }

        public DateTime Timestamp { get; set; } = DateTime.UtcNow;

        public string? Details { get; set; }
    }
}
